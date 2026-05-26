using Microsoft.EntityFrameworkCore;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class ScoreConfigService(IDbContextFactory<ApplicationDbContext> factory)
    {
        // ── Lettura ───────────────────────────────────────────────────────────

        public async Task<ScoreConfig> GetDefaultAsync()
        {
            using var ctx = factory.CreateDbContext();
            var cfg = await ctx.ScoreConfigs
                .FirstOrDefaultAsync(c => c.TaskEntityId == null && c.MissionTAKSuiteId == null);
            if (cfg != null) return cfg;
            cfg = new ScoreConfig { Id = Guid.NewGuid() };
            ctx.ScoreConfigs.Add(cfg);
            await ctx.SaveChangesAsync();
            return cfg;
        }

        public async Task<ScoreConfig?> GetByMissionAsync(Guid missionId)
        {
            using var ctx = factory.CreateDbContext();
            return await ctx.ScoreConfigs
                .FirstOrDefaultAsync(c => c.MissionTAKSuiteId == missionId && c.TaskEntityId == null);
        }

        public async Task<ScoreConfig?> GetByTaskAsync(Guid taskId)
        {
            using var ctx = factory.CreateDbContext();
            return await ctx.ScoreConfigs.FirstOrDefaultAsync(c => c.TaskEntityId == taskId);
        }

        // Gerarchia: obiettivo → missione → DEFAULT globale
        public async Task<ScoreConfig> GetEffectiveAsync(Guid taskId, Guid? missionId = null)
        {
            var perTask = await GetByTaskAsync(taskId);
            if (perTask != null) return perTask;
            if (missionId.HasValue)
            {
                var perMission = await GetByMissionAsync(missionId.Value);
                if (perMission != null) return perMission;
            }
            return await GetDefaultAsync();
        }

        // ── Scrittura ─────────────────────────────────────────────────────────

        public async Task SaveDefaultAsync(ScoreConfig cfg)
        {
            cfg.TaskEntityId    = null;
            cfg.MissionTAKSuiteId = null;
            await UpsertAsync(cfg);
        }

        public async Task SaveForMissionAsync(ScoreConfig cfg, Guid missionId)
        {
            cfg.MissionTAKSuiteId = missionId;
            cfg.TaskEntityId    = null;
            await UpsertAsync(cfg);
        }

        public async Task SaveForTaskAsync(ScoreConfig cfg, Guid taskId)
        {
            cfg.TaskEntityId = taskId;
            await UpsertAsync(cfg);
        }

        public async Task DeleteForTaskAsync(Guid taskId)
        {
            using var ctx = factory.CreateDbContext();
            var cfg = await ctx.ScoreConfigs.FirstOrDefaultAsync(c => c.TaskEntityId == taskId);
            if (cfg != null) { ctx.ScoreConfigs.Remove(cfg); await ctx.SaveChangesAsync(); }
        }

        public async Task DeleteForMissionAsync(Guid missionId)
        {
            using var ctx = factory.CreateDbContext();
            var cfg = await ctx.ScoreConfigs
                .FirstOrDefaultAsync(c => c.MissionTAKSuiteId == missionId && c.TaskEntityId == null);
            if (cfg != null) { ctx.ScoreConfigs.Remove(cfg); await ctx.SaveChangesAsync(); }
        }

        // ── Upsert interno ────────────────────────────────────────────────────

        private async Task UpsertAsync(ScoreConfig cfg)
        {
            using var ctx = factory.CreateDbContext();

            ScoreConfig? existing;
            if (cfg.TaskEntityId.HasValue)
                existing = await ctx.ScoreConfigs.FirstOrDefaultAsync(c => c.TaskEntityId == cfg.TaskEntityId);
            else if (cfg.MissionTAKSuiteId.HasValue)
                existing = await ctx.ScoreConfigs.FirstOrDefaultAsync(
                    c => c.MissionTAKSuiteId == cfg.MissionTAKSuiteId && c.TaskEntityId == null);
            else
                existing = await ctx.ScoreConfigs.FirstOrDefaultAsync(
                    c => c.TaskEntityId == null && c.MissionTAKSuiteId == null);

            if (existing == null)
            {
                cfg.Id = Guid.NewGuid();
                ctx.ScoreConfigs.Add(cfg);
            }
            else
            {
                CopyFields(cfg, existing);
            }
            await ctx.SaveChangesAsync();
        }

        private static void CopyFields(ScoreConfig src, ScoreConfig dst)
        {
            dst.MaxBivacco                 = src.MaxBivacco;
            dst.MaxAiutoCartografico       = src.MaxAiutoCartografico;
            dst.MaxOperatoreNonDichiarato  = src.MaxOperatoreNonDichiarato;
            dst.MaxMarcaturaAsgAssente     = src.MaxMarcaturaAsgAssente;
            dst.MaxFasciaNonEsposta        = src.MaxFasciaNonEsposta;
            dst.MaxOperatoreSqualificato   = src.MaxOperatoreSqualificato;
            dst.MaxInterferenzaArbitrale   = src.MaxInterferenzaArbitrale;
            dst.MaxComportamentoAntisportivo = src.MaxComportamentoAntisportivo;
            dst.MaxAsgOverJoule            = src.MaxAsgOverJoule;
            dst.MaxSaccoRifiuti            = src.MaxSaccoRifiuti;
            dst.MaxDifensoriEliminati      = src.MaxDifensoriEliminati;
            dst.MaxRibelliColpiti          = src.MaxRibelliColpiti;
            dst.MaxCiviliColpiti           = src.MaxCiviliColpiti;
            dst.MostraFasiENulle           = src.MostraFasiENulle;
            dst.FasiEJson                  = src.FasiEJson;
        }
    }
}
