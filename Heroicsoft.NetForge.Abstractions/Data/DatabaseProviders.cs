namespace Heroicsoft.NetForge.Abstractions.Data;

/// <summary>
/// Provider names used by <c>Database:Provider</c> across the self-hosted apps. The EF
/// registration itself stays in the app, because each one has its own context type.
/// </summary>
public static class DatabaseProviders
{
    /// <summary>Configuration key <c>Database:Provider</c>.</summary>
    public const string ConfigurationKey = "Database:Provider";

    /// <summary>SQLite provider name, <c>Sqlite</c>.</summary>
    public const string Sqlite = "Sqlite";

    /// <summary>SQL Server provider name, <c>SqlServer</c>.</summary>
    public const string SqlServer = "SqlServer";

    /// <summary>Npgsql provider name, <c>Npgsql</c>.</summary>
    public const string Npgsql = "Npgsql";

    /// <summary>MySQL provider name, <c>MySql</c>.</summary>
    public const string MySql = "MySql";

    /// <summary><see cref="Sqlite"/>, <see cref="SqlServer"/>, <see cref="Npgsql"/>, and <see cref="MySql"/>, in that order.</summary>
    public static readonly IReadOnlyList<string> All = [Sqlite, SqlServer, Npgsql, MySql];

    /// <summary>
    /// Returns the canonical name from <paramref name="allowed"/>, or throws when the configured
    /// value is not one of them. A blank <paramref name="configured"/> uses <paramref name="fallback"/>.
    /// The match ignores case. Failing at startup beats silently opening the wrong database.
    /// </summary>
    /// <param name="configured">Provider name from configuration. <see langword="null"/> or whitespace selects <paramref name="fallback"/>.</param>
    /// <param name="fallback">Name used when <paramref name="configured"/> is blank. It is not trimmed.</param>
    /// <param name="allowed">Accepted spellings. The returned text is one of these values. At least one is required.</param>
    /// <exception cref="ArgumentException"><paramref name="allowed"/> is empty.</exception>
    /// <exception cref="InvalidOperationException">The selected name is not in <paramref name="allowed"/>.</exception>
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