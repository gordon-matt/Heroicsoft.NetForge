using Heroicsoft.NetForge.Abstractions.Theming;

namespace Heroicsoft.NetForge.Tests;

public class ThemeStorageTests
{
    [Theory]
    [InlineData(null, null, "netforge.theme")]
    [InlineData("  ", "", "netforge.theme")]
    [InlineData("kinnect-theme", null, "kinnect-theme")]
    [InlineData(" kinnect-theme ", "  ", "kinnect-theme")]
    [InlineData("kinnect-theme", "Admin", "kinnect-theme.admin")]
    [InlineData("kinnect-theme", " admin ", "kinnect-theme.admin")]
    [InlineData("myapp.theme", "Docs", "myapp.theme.docs")]
    public void KeyFor_KeepsTheBaseKeyOnTheDefaultArea_AndSuffixesANamedArea(string? baseKey, string? area, string expected)
    {
        Assert.Equal(expected, ThemeStorage.KeyFor(baseKey, area));
    }

    [Fact]
    public void ResolveArea_UsesAnExplicitArea_IncludingAnExplicitDefault()
    {
        string[] named = ["Admin"];

        Assert.Equal("admin", ThemeStorage.ResolveArea("Admin", "Identity", "/Identity/Account/Login", named));
        Assert.Equal("", ThemeStorage.ResolveArea("", "Admin", "/Admin/users", named));
    }

    [Fact]
    public void ResolveArea_UsesAKnownRouteArea_BeforeThePath()
    {
        string[] named = ["Admin"];

        Assert.Equal("admin", ThemeStorage.ResolveArea(null, "Admin", "/somewhere", named));
    }

    [Fact]
    public void ResolveArea_IgnoresAnUnlistedRouteArea_AndCanStillMatchThePath()
    {
        string[] named = ["Admin"];

        Assert.Equal("", ThemeStorage.ResolveArea(null, "Identity", "/Identity/Account/Login", named));
        Assert.Equal("admin", ThemeStorage.ResolveArea(null, "Identity", "/Admin/identity/users", named));
    }

    [Theory]
    [InlineData("/Admin", "admin")]
    [InlineData("/admin/users", "admin")]
    [InlineData("/administration", "")]
    [InlineData("/", "")]
    [InlineData(null, "")]
    public void ResolveArea_MatchesTheFirstPathSegment_OnlyWhenThatAreaIsListed(string? path, string expected)
    {
        Assert.Equal(expected, ThemeStorage.ResolveArea(null, null, path, ["Admin"]));
    }

    [Fact]
    public void ResolveArea_WithNoNamedAreas_StaysOnTheDefaultArea()
    {
        Assert.Equal("", ThemeStorage.ResolveArea(null, "Admin", "/Admin/users", null));
    }

    [Fact]
    public void ValueFor_UsesTheAreaDefault_ThenTheAppDefault()
    {
        var areas = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Admin"] = "dark",
            ["Docs"] = "  ",
        };

        Assert.Equal("dark", ThemeStorage.ValueFor("admin", areas, "light"));
        Assert.Equal("light", ThemeStorage.ValueFor("docs", areas, "light"));
        Assert.Equal("system", ThemeStorage.ValueFor("", areas, "system"));
        Assert.Equal("flatly", ThemeStorage.ValueFor("missing", areas, "flatly"));
    }

    [Fact]
    public void AreasAttribute_IsNullUntilAnAreaIsRegistered()
    {
        Assert.Null(ThemeStorage.AreasAttribute(null));
        Assert.Null(ThemeStorage.AreasAttribute(new Dictionary<string, string?>()));
        Assert.Null(ThemeStorage.AreasAttribute(new Dictionary<string, string?> { [""] = "dark" }));
    }

    [Fact]
    public void AreasAttribute_ListsEachNamedArea_AndKeepsABlankDefault()
    {
        var areas = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["Admin"] = "Dark",
            ["Docs"] = null,
        };

        Assert.Equal("{\"admin\":\"dark\",\"docs\":\"\"}", ThemeStorage.AreasAttribute(areas));
    }
}
