using Heroicsoft.NetForge.Abstractions.Data;
using Heroicsoft.NetForge.Abstractions.Theming;

namespace Heroicsoft.NetForge.Tests;

public class BootswatchAndDatabaseTests
{
    [Fact]
    public void DarkThemes_AreTheOnesTheShellTreatsAsDark()
    {
        Assert.True(BootswatchThemes.IsDarkColorScheme("vapor"));
        Assert.False(BootswatchThemes.IsDarkColorScheme("flatly"));
        Assert.False(BootswatchThemes.IsDarkColorScheme(null));
        Assert.Contains("solar", BootswatchThemes.DarkThemes);
        Assert.Contains("flatly", BootswatchThemes.LightThemes);
    }

    [Fact]
    public void TryNormalize_RejectsUnknownThemes()
    {
        Assert.False(BootswatchThemes.TryNormalize("nope", out string normalized));
        Assert.Equal(BootswatchThemes.Default, normalized);

        Assert.True(BootswatchThemes.TryNormalize("  VAPOR ", out normalized));
        Assert.Equal("vapor", normalized);
    }

    [Fact]
    public void Resolve_UsesFallbackAndRejectsUnknownProviders()
    {
        Assert.Equal(
            DatabaseProviders.Npgsql,
            DatabaseProviders.Resolve(null, DatabaseProviders.Npgsql, DatabaseProviders.Npgsql));

        Assert.Equal(
            DatabaseProviders.Sqlite,
            DatabaseProviders.Resolve("sqlite", DatabaseProviders.Npgsql, DatabaseProviders.All.ToArray()));

        var error = Assert.Throws<InvalidOperationException>(() =>
            DatabaseProviders.Resolve("Oracle", DatabaseProviders.Sqlite, DatabaseProviders.Sqlite));

        Assert.Contains("Oracle", error.Message);
    }
}