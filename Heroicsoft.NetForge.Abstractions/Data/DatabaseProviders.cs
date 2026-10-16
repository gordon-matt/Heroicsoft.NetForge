namespace Heroicsoft.NetForge.Abstractions.Data;

/// <summary>
/// Provider names used by <c>Database:Provider</c> across the self-hosted apps. The EF
/// registration itself stays in the app, because each one has its own context type.
/// </summary>
public static class DatabaseProviders
{
    public const string ConfigurationKey = "Database:Provider";

    public const string Sqlite = "Sqlite";

    public const string SqlServer = "SqlServer";

    public const string Npgsql = "Npgsql";

    public const string MySql = "MySql";

    public static readonly IReadOnlyList<string> All = [Sqlite, SqlServer, Npgsql, MySql];

    /// <summary>
    /// Returns the canonical name from <paramref name="allowed"/>, or throws when the configured
    /// value is not one of them. Failing at startup beats silently opening the wrong database.
    /// </summary>
    public static string Resolve(string? configured, string fallback, params string[] allowed)
    {
        if (allowed.Length == 0)
        {
            throw new ArgumentException("At least one database provider must be allowed.", nameof(allowed));
        }

        string provider = string.IsNullOrWhiteSpace(configured) ? fallback : configured.Trim();
        string? canonical = allowed.FirstOrDefault(name =>
            string.Equals(name, provider, StringComparison.OrdinalIgnoreCase));

        return canonical is null
            ? throw new InvalidOperationException(
                $"Unknown {ConfigurationKey} '{provider}'. Valid values: {string.Join(", ", allowed)}.")
            : canonical;
    }
}