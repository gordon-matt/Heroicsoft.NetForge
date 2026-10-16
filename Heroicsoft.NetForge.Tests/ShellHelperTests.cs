using Heroicsoft.NetForge.Authentication;
using Heroicsoft.NetForge.Components.Services;

namespace Heroicsoft.NetForge.Tests;

public class ShellHelperTests
{
    [Theory]
    [InlineData("/setup/done", true)]
    [InlineData("/favicon.ico", true)]
    [InlineData("/_blazor/negotiate", true)]
    [InlineData("/api/items", false)]
    [InlineData("/", false)]
    public void SetupPaths_ExemptTheWizardAndTheAssetsItNeeds(string path, bool exempt)
    {
        string[] prefixes = ["/setup", "/_blazor", "/favicon"];

        Assert.Equal(exempt, SetupPathRules.IsExempt(path, prefixes));
    }

    [Fact]
    public void Navigation_IsAGetThatAcceptsHtml()
    {
        Assert.True(SetupPathRules.IsNavigation("GET", ["text/html,application/xhtml+xml"]));
        Assert.False(SetupPathRules.IsNavigation("POST", ["text/html"]));
        Assert.False(SetupPathRules.IsNavigation("GET", ["application/json"]));
    }

    [Fact]
    public void VisiblePages_StaysAWindowAroundTheCurrentPage()
    {
        Assert.Equal([1, 2, 3, 4, 5], PagerMath.VisiblePages(1, 40));
        Assert.Equal([8, 9, 10, 11, 12], PagerMath.VisiblePages(10, 12));
        Assert.Equal(1, PagerMath.PageCount(0, 24));
        Assert.Equal(2, PagerMath.PageCount(25, 24));
    }
}