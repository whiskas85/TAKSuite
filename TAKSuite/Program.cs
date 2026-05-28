using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TAKSuite.Components;
using TAKSuite.Components.Account;
using TAKSuite.Data;
using TAKSuite.TAK;
using TAKSuite.TAK.CoT;
using TAKSuite.TAK.MartiApi;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using TAKSuite.TAK.Helper;
using TAKSuite.Data.Services;
using TAKSuite.Data.ServicesTak;
using BlazorPro.BlazorSize;
using TAKSuite.Data.Seeder;
using TAKSuite.Data.Services.BaseDataManagement;
using TAKSuite.Data.Services.AI;
using TAKSuite.Data.Models;
using Serilog;

var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
Directory.CreateDirectory(logDir);
if (!System.Diagnostics.Debugger.IsAttached)
    Console.SetOut(new ConsoleToSerilog());

var sessionLogFile = Path.Combine(logDir, $"session-{DateTime.Now:yyyyMMdd-HHmmss}.log");
const string logTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.File(Path.Combine(logDir, "app-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: logTemplate)
    .WriteTo.File(sessionLogFile, outputTemplate: logTemplate)
    .CreateBootstrapLogger();

try
{

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
builder.Host.UseSerilog((ctx, cfg) => cfg
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.File(Path.Combine(logDir, "app-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: logTemplate)
    .WriteTo.File(sessionLogFile, outputTemplate: logTemplate));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.AddScoped(sp =>
{
    var navManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navManager.BaseUri) };
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// Factory (Singleton) usato da tutti i servizi per context isolati per operazione
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
           .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

// DbContext Scoped per Identity — creato dalla factory, non da AddDbContext
builder.Services.AddScoped<ApplicationDbContext>(p =>
    p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite    = SameSiteMode.Lax;
});

// TAK Connectivity
builder.Services.AddSingleton<TakClientProvider>();  // TakLib client (mTLS HTTP + TCP/SSL streaming)
builder.Services.AddSingleton<CoTApiClient>();
builder.Services.AddSingleton<MartiApiClient>();
builder.Services.AddSingleton<MartiHttpClientProvider>();
builder.Services.AddSingleton(sp => new MissionApiClient(sp.GetRequiredService<MartiHttpClientProvider>().HttpClient));
builder.Services.AddSingleton(sp => new SubscriptionApiClient(sp.GetRequiredService<MartiHttpClientProvider>().HttpClient));
builder.Services.AddSingleton(sp => new TAKSuite.TAK.MartiApi.CotApiClient(sp.GetRequiredService<MartiHttpClientProvider>().HttpClient));
builder.Services.AddSingleton(sp => new FileManagerApiClient(sp.GetRequiredService<MartiHttpClientProvider>().HttpClient));
builder.Services.AddSingleton(sp => new GroupsApiClient(sp.GetRequiredService<MartiHttpClientProvider>().HttpClient));
builder.Services.AddSingleton(sp => new VideoConnectionManagerV2Client(sp.GetRequiredService<MartiHttpClientProvider>().HttpClient));
builder.Services.AddSingleton(sp => new DataFeedApiClient(sp.GetRequiredService<MartiHttpClientProvider>().HttpClient));
builder.Services.AddSingleton(sp => new ContactsApiClient(sp.GetRequiredService<MartiHttpClientProvider>().HttpClient));
builder.Services.AddSingleton<TakTrafficLogger>();
builder.Services.AddSingleton<CachedDataService>();      // Cached Data Service (server-connected SA)
builder.Services.AddSingleton<ProtoCacheService>();      // Cache persistente punti protobuf (non connessi)
builder.Services.AddSingleton<CoTManager>();             // CoTManager
builder.Services.AddTransient<WebSocketManagerCustom>();



builder.Services.AddSingleton<UserSession>();      // CoTManager

// Add services
//ServicesRegistration.ConfigureServices(builder.Services);

builder.Services.AddTransient<IDataProvider, TeamService>();
builder.Services.AddTransient<IDataProvider, TaskPrioritiesService>();
builder.Services.AddTransient<TeamService>();
builder.Services.AddTransient<RegistrationCodeService>();
builder.Services.AddTransient<ParckingService>();
builder.Services.AddTransient<WaypointService>();
builder.Services.AddTransient<AttachmentService>();
builder.Services.AddTransient<MedevacService>();
builder.Services.AddTransient<RadioChannelService>();
builder.Services.AddTransient<TeamRadioChannelService>();
builder.Services.AddTransient<UserServiceAtak>();
builder.Services.AddTransient<DocumentationService>();
builder.Services.AddTransient<DocumentTypeService>();
builder.Services.AddTransient<TaskService>();
builder.Services.AddTransient<TaskPrioritiesService>();
builder.Services.AddTransient<EventEntityService>();
builder.Services.AddTransient<MissionSuiteEntityServices>();
builder.Services.AddTransient<TaskTemplateService>();
builder.Services.AddTransient<CotTemplateService>();
builder.Services.AddTransient<MissionRadioContactService>();
builder.Services.AddTransient<PhoneContactService>();
builder.Services.AddTransient<MissionPhoneContactService>();
builder.Services.AddTransient<AiCoordinatesService>();
builder.Services.AddTransient<AiSettingsService>();
builder.Services.AddTransient<AiService>();
builder.Services.AddTransient<TaskScoreService>();
builder.Services.AddTransient<ScoreConfigService>();
builder.Services.AddTransient<TakSettingsService>();
builder.Services.AddTransient<TakSubscriptionService>();
builder.Services.AddSingleton<PhotoAutoJoinService>();
builder.Services.AddSingleton<NavRefreshService>();
builder.Services.AddSingleton<AtakMapsService>();



builder.Services.AddScoped<RadioChannelSeeder>();
builder.Services.AddScoped<TaskPrioritySeeder>();
builder.Services.AddScoped<EPSGSeeder>();


builder.Services.AddMediaQueryService();

builder.Services.AddScoped<TAKSuite.Helpers.JsInteropHelper>();

builder.Services.AddScoped<RegistrationCodeService>();

builder.Services.AddHttpClient();



builder.Services.AddMemoryCache();


builder.Services.AddHttpContextAccessor();


// Aggiungi i servizi necessari per i controller
//builder.Services.AddControllers();

builder.Services.AddBlazorBootstrap();

var app = builder.Build();

// Avvio del client CoTApiClient in background quando l'app è pronta
var lifetime         = app.Services.GetRequiredService<IHostApplicationLifetime>();
var cotApiClient     = app.Services.GetRequiredService<CoTApiClient>();
var takProvider      = app.Services.GetRequiredService<TakClientProvider>();
var protoCache       = app.Services.GetRequiredService<ProtoCacheService>();
var photoAutoJoinSvc = app.Services.GetRequiredService<PhotoAutoJoinService>(); // sottoscrive CoTManager in ctor

lifetime.ApplicationStarted.Register(() =>
{
    var cancellationToken = lifetime.ApplicationStopping;

    Task.Run(async () =>
    {
        // Applica migrazioni EF Core pendenti (crea tabelle nuove se non esistono)
        try
        {
            await using var db = await app.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();
            await db.Database.MigrateAsync();
            Log.Information("[Startup] Migrazioni DB applicate");
        }
        catch (Exception ex) { Log.Warning(ex, "[Startup] Migrazione DB fallita"); }

        // Guard SQL idempotente: aggiunge MissionName a CachedCoTEntries se mancante.
        // Bypassa il meccanismo EF Migration — garantisce la colonna anche senza Designer file.
        try
        {
            await using var dbGuard = await app.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();
            await dbGuard.Database.ExecuteSqlRawAsync(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CachedCoTEntries')
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                                   WHERE TABLE_NAME = 'CachedCoTEntries' AND COLUMN_NAME = 'MissionName')
                    BEGIN
                        ALTER TABLE CachedCoTEntries ADD MissionName NVARCHAR(450) NULL;
                        IF NOT EXISTS (SELECT 1 FROM sys.indexes
                                       WHERE name = 'IX_CachedCoTEntries_MissionName'
                                         AND object_id = OBJECT_ID('CachedCoTEntries'))
                            CREATE INDEX IX_CachedCoTEntries_MissionName ON CachedCoTEntries(MissionName);
                    END
                END");
            Log.Information("[Startup] Schema CachedCoTEntries verificato");
        }
        catch (Exception ex) { Log.Warning(ex, "[Startup] Schema guard CachedCoTEntries fallito"); }

        // Carica impostazioni TAK dal DB e riconfigura il client
        try { await takProvider.InitializeFromDbAsync(); }
        catch (Exception ex) { Log.Warning(ex, "[Startup] InitializeFromDbAsync fallito"); }

        // Inizializza cache protobuf (carica da DB, pulisce scaduti)
        try
        {
            using var scope    = app.Services.CreateScope();
            var settingsSvc    = scope.ServiceProvider.GetRequiredService<TakSettingsService>();
            var settings       = await settingsSvc.GetOrCreateAsync();
            await protoCache.InitializeAsync(settings.ProtoDeleteMinutes);
            Log.Information("[Startup] ProtoCacheService inizializzato ({Count} punti caricati)", protoCache.Count);
        }
        catch (Exception ex) { Log.Warning(ex, "[Startup] ProtoCacheService init fallito"); }

        // Ripristina sottoscrizioni missioni salvate
        try
        {
            using var scope   = app.Services.CreateScope();
            var subSvc        = scope.ServiceProvider.GetRequiredService<TakSubscriptionService>();
            var martiClient   = app.Services.GetRequiredService<MartiApiClient>();
            var subscriptions = await subSvc.GetAllAsync();
            foreach (var sub in subscriptions)
            {
                try { await martiClient.SubscribeMissionAsync(sub.MissionName); }
                catch (Exception ex) { Log.Warning(ex, "[Startup] Ri-sottoscrizione '{MissionName}' fallita", sub.MissionName); }
            }
            if (subscriptions.Count > 0)
                Log.Information("[Startup] Ripristinate {Count} sottoscrizioni", subscriptions.Count);
        }
        catch (Exception ex) { Log.Warning(ex, "[Startup] Ripristino sottoscrizioni fallito"); }

        // Avvia lo streaming CoT
        await cotApiClient.StartListening(cancellationToken);
    });

    Task.Run(() => cotApiClient.StartKeepAliveLoop(cancellationToken));
    var udpLogFile = Path.Combine(logDir, $"udp-cot-{DateTime.Now:yyyyMMdd-HHmmss}.log");
    Task.Run(() => cotApiClient.StartUdpListeningAsync(8089, cancellationToken, logFile: udpLogFile));
    Task.Run(() => cotApiClient.StartUdpListeningAsync(6969, cancellationToken, "239.2.3.1", udpLogFile));
});

// Configura il ciclo di vita dell'app
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Document file serving endpoint
app.MapGet("/api/doc/{id:guid}/file/{filename?}", async (Guid id, string? filename, DocumentationService docService, IWebHostEnvironment env, HttpContext httpCtx) =>
{
    var doc = await docService.GetAsync(id);
    if (doc == null) return Results.NotFound();
    var absolutePath = System.IO.Path.IsPathRooted(doc.Path)
        ? doc.Path
        : System.IO.Path.Combine(env.ContentRootPath, doc.Path);
    if (!System.IO.File.Exists(absolutePath)) return Results.NotFound();
    var mime = string.IsNullOrWhiteSpace(doc.Type) ? "application/octet-stream" : doc.Type;
    var fileName = string.IsNullOrWhiteSpace(doc.Name) ? "file" : doc.Name;
    httpCtx.Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"; filename*=UTF-8''{Uri.EscapeDataString(fileName)}";
    return Results.File(absolutePath, mime, enableRangeProcessing: true);
}).RequireAuthorization();

app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Applicazione terminata inaspettatamente");
}
finally
{
    Log.CloseAndFlush();
}

class ConsoleToSerilog : System.IO.TextWriter
{
    public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

    public override void WriteLine(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            Log.Information("{ConsoleMessage}", value);
    }

    public override void Write(string? value) { }
    public override void Write(char value) { }
}
