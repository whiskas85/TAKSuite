using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
using TAKSuite.Data.Models;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

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
builder.Services.AddSingleton<CachedDataService>();      // Cached Data Service
builder.Services.AddSingleton<CoTManager>();      // CoTManager
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
builder.Services.AddTransient<TaskService>();
builder.Services.AddTransient<TaskPrioritiesService>();
builder.Services.AddTransient<EventEntityService>();
builder.Services.AddTransient<MissionSuiteEntityServices>();
builder.Services.AddTransient<TaskTemplateService>();
builder.Services.AddTransient<AiCoordinatesService>();
builder.Services.AddTransient<TakSettingsService>();
builder.Services.AddTransient<TakSubscriptionService>();
builder.Services.AddSingleton<PhotoAutoJoinService>();



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
var photoAutoJoinSvc = app.Services.GetRequiredService<PhotoAutoJoinService>(); // sottoscrive CoTManager in ctor

lifetime.ApplicationStarted.Register(() =>
{
    var cancellationToken = lifetime.ApplicationStopping;

    Task.Run(async () =>
    {
        // Carica impostazioni TAK dal DB e riconfigura il client
        try { await takProvider.InitializeFromDbAsync(); }
        catch (Exception ex) { Console.WriteLine($"[Startup] InitializeFromDbAsync: {ex.Message}"); }

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
                catch (Exception ex) { Console.WriteLine($"[Startup] Ri-sottoscrizione '{sub.MissionName}' fallita: {ex.Message}"); }
            }
            if (subscriptions.Count > 0)
                Console.WriteLine($"[Startup] Ripristinate {subscriptions.Count} sottoscrizioni.");
        }
        catch (Exception ex) { Console.WriteLine($"[Startup] Ripristino sottoscrizioni: {ex.Message}"); }

        // Avvia lo streaming CoT
        await cotApiClient.StartListening(cancellationToken);
    });

    Task.Run(() => cotApiClient.StartKeepAliveLoop(cancellationToken));
});

// Configura il ciclo di vita dell'app
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

//app.MapControllers(); // 🚀 Questo attiva il controller API

app.Run();
