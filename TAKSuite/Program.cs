using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TAKSuite.Components;
using TAKSuite.Components.Account;
using TAKSuite.Data;
using TAKSuite.TAK.CoT;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using TAKSuite.TAK.Helper;
using TAKSuite.Data.Services;
using TAKSuite.Data.ServicesTak;
using BlazorPro.BlazorSize;
using TAKSuite.Data.Seeder;
using TAKSuite.Data.Services.BaseDataManagement;

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
builder.Services.AddSingleton<CoTApiClient>(); // Singleton per la connessione persistente
builder.Services.AddSingleton<MartiApiClient>();
builder.Services.AddSingleton<CachedDataService>();      // Cached Data Service
builder.Services.AddSingleton<CoTManager>();      // CoTManager
builder.Services.AddTransient<WebSocketManagerCustom>();


// Add services
//ServicesRegistration.ConfigureServices(builder.Services);

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


builder.Services.AddScoped<RadioChannelSeeder>();


builder.Services.AddMediaQueryService();

builder.Services.AddScoped<TAKSuite.Helpers.JsInteropHelper>();

builder.Services.AddScoped<RegistrationCodeService>();

builder.Services.AddHttpClient();



builder.Services.AddHttpContextAccessor();


// Aggiungi i servizi necessari per i controller
//builder.Services.AddControllers();

builder.Services.AddBlazorBootstrap();

var app = builder.Build();

// Avvio del client CoTApiClient in background quando l'app è pronta
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var cotApiClient = app.Services.GetRequiredService<CoTApiClient>();

lifetime.ApplicationStarted.Register(() =>
{
    var cancellationToken = lifetime.ApplicationStopping;
    Task.Run(() => cotApiClient.StartListening(cancellationToken)); // Avvia il client in background
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
