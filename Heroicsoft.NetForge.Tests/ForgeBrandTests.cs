using Heroicsoft.NetForge.Components.Layout;

namespace Heroicsoft.NetForge.Tests;

public class ForgeBrandTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("/brand/logo.png", "/brand/logo.png")]
    [InlineData("~/brand/logo.png", "/brand/logo.png")]
    [InlineData("brand/logo.png", "/brand/logo.png")]
    [InlineData("https://cdn.example.com/logo.png", "https://cdn.example.com/logo.png")]
    public void Url_NormalizesThePath(string? path, string? expected)
    {
        Assert.Equal(expected, ForgeBrand.Url(path));
    }

    [Fact]
    public void FromShell_ReadsTheHostLogoAndName()
    {
        var shell = new ForgeShellOptions
        {
            Brand = " Kinnect ",
            LogoUrl = "img/Logo.png",
            LogoDarkUrl = "~/img/Logo-Dark.png",
            MarkUrl = "/img/Icon.png",
        };

        Assert.Equal("/img/Logo.png", ForgeBrand.Logo(shell));
        Assert.Equal("/img/Logo-Dark.png", ForgeBrand.LogoDark(shell));
        Assert.Equal("/img/Icon.png", ForgeBrand.Mark(shell));
        Assert.Equal("Kinnect", ForgeBrand.Name(shell));
    }

    [Theory]
    [InlineData(null, "/")]
    [InlineData("", "/")]
    [InlineData("./", "/")]
    [InlineData("/", "/")]
    [InlineData("movies", "/movies")]
    [InlineData("/movies", "/movies")]
    [InlineData("~/movies", "/movies")]
    public void Home_IsTheHostAddress(string? href, string expected)
    {
        Assert.Equal(expected, ForgeBrand.Home(new ForgeShellOptions { BrandHref = href! }));
    }
}
