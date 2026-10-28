using LogVue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using NetForgeApp.Components;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogVue(options =>
    builder.Configuration.GetSection(LogVueOptions.SectionName).Bind(options));

builder.Services.AddSerilog((services, configuration) => configuration
    .MinimumLevel.Information()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.LogVue(services));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.AddNetForgeComponents(ui =>
{
    ui.ThemeStorageKey = NetForgeApp.Constants.ClientThemeLocalStorageKey;
    ui.DefaultColorScheme = "system";
    ui.AccentColor = "#4f46e5";
    ui.AccentColorDark = "#818cf8";
});

builder.Services.Configure<ForgeShellOptions>(shell =>
{
    shell.Brand = "NetForgeApp";
    shell.LogoUrl = "img/Icon.png";
    shell.MarkUrl = "img/Icon.png";
});
builder.Services.AddScoped<IShellNavigation, NetForgeAppNavigation>();

builder.Services.AddSingleton(TimeProvider.System);

string databaseProvider = DatabaseProviders.Resolve(
    builder.Configuration[DatabaseProviders.ConfigurationKey],
    DatabaseProviders.Sqlite,
    [.. DatabaseProviders.All]);

builder.Services.AddNetForgeAppDatabase(builder.Configuration);
builder.Services.AddNetForgeAppRepositories();

var authProvider = builder.Services.AddNetForgeAppAuthentication(builder.Configuration);

builder.Services.AddNetForgeAppServices();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

var forwardedHeaders = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
forwardedHeaders.KnownIPNetworks.Clear();
forwardedHeaders.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaders);

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapStaticAssets();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapLogVue().RequireAuthorization(new AuthorizeAttribute
{
    Roles = WellKnownRoles.Administrator
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .WithStaticAssets();

app.MapControllers();

if (authProvider == AuthProvider.Identity)
{
    app.MapRazorPages().WithStaticAssets();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

await using (var scope = app.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider, authProvider, app.Configuration, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed");
        throw;
    }
}

Log.Information(
    "NetForgeApp {Version} started with the {DatabaseProvider} database provider and {AuthProvider} authentication",
    AppVersion.Current,
    databaseProvider,
    authProvider);

app.Run();
