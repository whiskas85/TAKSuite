using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TAKSuite.Data;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services;

/// <summary>
/// Cache persistente (DB + memoria) per tutti i punti SA ricevuti.
/// Usa SQL raw per tutte le operazioni DB, così funziona anche se la migrazione
/// AddCachedCoTMission non è stata ancora applicata (colonna MissionName opzionale).
/// </summary>
public class ProtoCacheService : IDisposable
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly Dictionary<string, CachedCoTEntry> _cache = new();
    private readonly object _lock = new();
    private Timer? _cleanupTimer;
    private bool _missionNameColumnExists = false;

    public event Action? CacheChanged;

    public ProtoCacheService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // Boot: carica da DB, pulisce scaduti, avvia timer
    public async Task InitializeAsync(int deleteMinutes)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();

        // Verifica se la colonna MissionName esiste
        _missionNameColumnExists = await ColumnExistsAsync(conn, "CachedCoTEntries", "MissionName");

        // Elimina le entry scadute
        var cutoff = DateTime.UtcNow.AddMinutes(-deleteMinutes);
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "DELETE FROM CachedCoTEntries WHERE LastSeen < @cutoff";
            cmd.Parameters.Add(new SqlParameter("@cutoff", cutoff));
            await cmd.ExecuteNonQueryAsync();
        }

        // Carica tutto in memoria
        var entries = new List<CachedCoTEntry>();
        using (var cmd = conn.CreateCommand())
        {
            var missionCol = _missionNameColumnExists ? ", MissionName" : "";
            cmd.CommandText = $"SELECT Uid, Callsign, CotType, Lat, Lon, Hae, Team, Role, RawXml, FirstSeen, LastSeen{missionCol} FROM CachedCoTEntries";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                entries.Add(new CachedCoTEntry
                {
                    Uid         = reader.GetString(0),
                    Callsign    = reader.IsDBNull(1)  ? null : reader.GetString(1),
                    CotType     = reader.IsDBNull(2)  ? null : reader.GetString(2),
                    Lat         = reader.GetDouble(3),
                    Lon         = reader.GetDouble(4),
                    Hae         = reader.GetDouble(5),
                    Team        = reader.IsDBNull(6)  ? null : reader.GetString(6),
                    Role        = reader.IsDBNull(7)  ? null : reader.GetString(7),
                    RawXml      = reader.IsDBNull(8)  ? "" : reader.GetString(8),
                    FirstSeen   = reader.GetDateTime(9),
                    LastSeen    = reader.GetDateTime(10),
                    MissionName = _missionNameColumnExists && !reader.IsDBNull(11) ? reader.GetString(11) : null
                });
            }
        }

        lock (_lock)
        {
            foreach (var e in entries)
                _cache[e.Uid] = e;
        }

        _cleanupTimer = new Timer(_ => _ = CleanupAsync(), null,
            TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));

        Console.WriteLine($"[ProtoCacheService] Init: {_cache.Count} punti caricati, MissionName={_missionNameColumnExists}");
    }

    // Aggiunge o aggiorna un punto.
    // missionName: non-null se CoT da missione TAK; null per SA Multicast.
    public async Task<bool> AddOrUpdateAsync(
        string uid, string rawXml,
        string? callsign, string? cotType,
        double lat, double lon, double hae,
        string? team, string? role,
        string? missionName = null)
    {
        var now = DateTime.UtcNow;
        bool isNew;

        lock (_lock)
        {
            isNew = !_cache.ContainsKey(uid);
            if (isNew)
                _cache[uid] = new CachedCoTEntry { Uid = uid, FirstSeen = now };

            var e = _cache[uid];
            e.Callsign = callsign;
            e.CotType  = cotType;
            e.Lat      = lat;
            e.Lon      = lon;
            e.Hae      = hae;
            e.Team     = team;
            e.Role     = role;
            e.RawXml   = rawXml;
            e.LastSeen = now;
            if (missionName != null)
                e.MissionName = missionName;
        }

        await PersistAsync(uid, rawXml, callsign, cotType, lat, lon, hae, team, role, missionName, now, isNew);
        CacheChanged?.Invoke();
        return isNew;
    }

    // Tutti i punti (per sidebar, nessun filtro)
    public List<(CachedCoTEntry Entry, bool IsStale)> GetAll(int staleMinutes)
    {
        var staleCutoff = DateTime.UtcNow.AddMinutes(-staleMinutes);
        lock (_lock)
        {
            return _cache.Values
                .Select(e => (e, e.LastSeen < staleCutoff))
                .OrderByDescending(x => x.e.LastSeen)
                .ToList();
        }
    }

    // Punti filtrati per missione (per mappa)
    public List<(CachedCoTEntry Entry, bool IsStale)> GetFiltered(int staleMinutes, IReadOnlyCollection<string>? missionFilter)
    {
        var staleCutoff  = DateTime.UtcNow.AddMinutes(-staleMinutes);
        bool applyFilter = missionFilter is { Count: > 0 };
        lock (_lock)
        {
            return _cache.Values
                .Where(e => !applyFilter || e.MissionName == null || missionFilter!.Contains(e.MissionName))
                .Select(e => (e, e.LastSeen < staleCutoff))
                .OrderByDescending(x => x.e.LastSeen)
                .ToList();
        }
    }

    public int Count { get { lock (_lock) return _cache.Count; } }

    // Rimuove un singolo dispositivo dalla cache in memoria e dal DB
    public async Task RemoveAsync(string uid)
    {
        lock (_lock) { _cache.Remove(uid); }
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM CachedCoTEntries WHERE Uid = @uid";
            var p = cmd.CreateParameter(); p.ParameterName = "@uid"; p.Value = uid;
            cmd.Parameters.Add(p);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { Console.WriteLine($"[ProtoCacheService] Remove error uid={uid}: {ex.Message}"); }
        CacheChanged?.Invoke();
    }

    // Svuota completamente la cache in memoria e nel DB
    public async Task ClearAllAsync()
    {
        lock (_lock) { _cache.Clear(); }
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM CachedCoTEntries";
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { Console.WriteLine($"[ProtoCacheService] ClearAll error: {ex.Message}"); }
        CacheChanged?.Invoke();
    }

    // ── Privato ───────────────────────────────────────────────────────────────────

    private async Task PersistAsync(
        string uid, string rawXml, string? callsign, string? cotType,
        double lat, double lon, double hae, string? team, string? role,
        string? missionName, DateTime lastSeen, bool isNew)
    {
        try
        {
            // Tronca RawXml se troppo lungo per nvarchar(max) su alcune configurazioni
            if (rawXml.Length > 8000) rawXml = rawXml[..8000];

            await using var db = await _dbFactory.CreateDbContextAsync();
            var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            if (isNew)
            {
                // INSERT
                var missionCol = _missionNameColumnExists ? ", MissionName" : "";
                var missionVal = _missionNameColumnExists ? ", @missionName" : "";
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
                    INSERT INTO CachedCoTEntries
                        (Uid, Callsign, CotType, Lat, Lon, Hae, Team, Role, RawXml, FirstSeen, LastSeen{missionCol})
                    VALUES
                        (@uid, @callsign, @cotType, @lat, @lon, @hae, @team, @role, @rawXml, @lastSeen, @lastSeen{missionVal})";
                AddParams(cmd, uid, callsign, cotType, lat, lon, hae, team, role, rawXml, lastSeen, missionName);
                await cmd.ExecuteNonQueryAsync();
            }
            else
            {
                // UPDATE
                var missionSet = _missionNameColumnExists && missionName != null ? ", MissionName=@missionName" : "";
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $@"
                    UPDATE CachedCoTEntries SET
                        Callsign=@callsign, CotType=@cotType,
                        Lat=@lat, Lon=@lon, Hae=@hae,
                        Team=@team, Role=@role, RawXml=@rawXml, LastSeen=@lastSeen{missionSet}
                    WHERE Uid=@uid";
                AddParams(cmd, uid, callsign, cotType, lat, lon, hae, team, role, rawXml, lastSeen, missionName);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProtoCacheService] Persist error uid={uid}: {ex.Message}");
        }
    }

    private static void AddParams(System.Data.IDbCommand cmd,
        string uid, string? callsign, string? cotType,
        double lat, double lon, double hae,
        string? team, string? role, string rawXml,
        DateTime lastSeen, string? missionName)
    {
        void Add(string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
        Add("@uid",         uid);
        Add("@callsign",    callsign);
        Add("@cotType",     cotType);
        Add("@lat",         lat);
        Add("@lon",         lon);
        Add("@hae",         hae);
        Add("@team",        team);
        Add("@role",        role);
        Add("@rawXml",      rawXml);
        Add("@lastSeen",    lastSeen);
        Add("@missionName", missionName);
    }

    private async Task CleanupAsync()
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var settings      = await db.TakSettings.FindAsync(1);
            int deleteMinutes = settings?.ProtoDeleteMinutes ?? 30;
            var cutoff        = DateTime.UtcNow.AddMinutes(-deleteMinutes);

            List<string> toDelete;
            lock (_lock)
            {
                toDelete = _cache.Values
                    .Where(e => e.LastSeen < cutoff)
                    .Select(e => e.Uid)
                    .ToList();
                foreach (var u in toDelete)
                    _cache.Remove(u);
            }

            if (toDelete.Count > 0)
            {
                var conn = db.Database.GetDbConnection();
                await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                // Batch delete — parametri dinamici
                var ids = string.Join(",", toDelete.Select((_, i) => $"@id{i}"));
                cmd.CommandText = $"DELETE FROM CachedCoTEntries WHERE Uid IN ({ids})";
                for (int i = 0; i < toDelete.Count; i++)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = $"@id{i}";
                    p.Value = toDelete[i];
                    cmd.Parameters.Add(p);
                }
                await cmd.ExecuteNonQueryAsync();
                CacheChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProtoCacheService] Cleanup error: {ex.Message}");
        }
    }

    private static async Task<bool> ColumnExistsAsync(System.Data.Common.DbConnection conn, string table, string column)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_NAME=@t AND COLUMN_NAME=@c";
        var p1 = cmd.CreateParameter(); p1.ParameterName = "@t"; p1.Value = table;  cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter(); p2.ParameterName = "@c"; p2.Value = column; cmd.Parameters.Add(p2);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public void Dispose() => _cleanupTimer?.Dispose();
}
