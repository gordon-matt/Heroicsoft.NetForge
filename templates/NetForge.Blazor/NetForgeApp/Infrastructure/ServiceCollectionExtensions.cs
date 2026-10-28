using NetForgeApp.Data.MySql;
using NetForgeApp.Data.Npgsql;
using NetForgeApp.Data.Sql;
using NetForgeApp.Data.Sqlite;

namespace NetForgeApp.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Reads <c>Database:Provider</c> and delegates to the matching provider project.
    /// </summary>
    public static IServiceCollection AddNetForgeAppDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string provider = DatabaseProviders.Resolve(
            configuration[DatabaseProviders.ConfigurationKey],
            DatabaseProviders.Sqlite,
            [.. DatabaseProviders.All]);

        return provider switch
        {
            DatabaseProviders.Sqlite => services.AddNetForgeAppSqlite(configuration),
            DatabaseProviders.SqlServer => services.AddNetForgeAppSqlServer(configuration),
            DatabaseProviders.Npgsql => services.AddNetForgeAppNpgsql(configuration),
            DatabaseProviders.MySql => services.AddNetForgeAppMySql(configuration),
            _ => throw new InvalidOperationException($"Unsupported {DatabaseProviders.ConfigurationKey} '{provider}'."),
        };
    }

    public static IServiceCollection AddNetForgeAppRepositories(this IServiceCollection services)
    {
        services.AddEntityFrameworkRepository();
        return services;
    }

    /// <summary>
    /// Wires up whichever authentication provider is configured and returns which one it was, so
    /// <c>Program.cs</c> can decide whether Identity's Razor Pages need mapping.
    /// </summary>
    public static AuthProvider AddNetForgeAppAuthentication(
        this IServiceCollection services,
        IConfiguration configuration) =>
        // Identity, Keycloak, and None — including IUserContextService — come from NetForge.
        // This host does not register an IUserInfoService.
        services.AddNetForgeAuthentication<ApplicationUser, ApplicationRole, ApplicationDbContextBase>(
            configuration);
}
