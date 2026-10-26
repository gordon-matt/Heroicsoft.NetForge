using Heroicsoft.NetForge.Components.Icons;
using Heroicsoft.NetForge.Components.Layout;
using Heroicsoft.NetForge.Components.Theming;
using Heroicsoft.NetForge.Components.Toasts;

namespace Heroicsoft.NetForge.Tests;

public class ForgeAccentCssTests
{
    [Fact]
    public void Build_WithNoAccent_EmitsNothing() => Assert.Null(ForgeAccentCss.Build(new ForgeUiOptions()));

    [Fact]
    public void Build_WithOneAccent_UsesItForBothSchemes()
    {
        string? css = ForgeAccentCss.Build(new ForgeUiOptions { AccentColor = "#c026d3" });

        Assert.Equal(
            ":root{--nf-accent:#c026d3;--nf-accent-text:#ffffff;}[data-theme=dark]{--nf-accent:#c026d3;--nf-accent-text:#ffffff;}",
            css);
    }

    [Fact]
    public void Build_WithBothAccents_GivesTheDarkSchemeItsOwnTextColour()
    {
        string? css = ForgeAccentCss.Build(new ForgeUiOptions { AccentColor = "#c026d3", AccentColorDark = "#e879f9" });

        Assert.Equal(
            ":root{--nf-accent:#c026d3;--nf-accent-text:#ffffff;}[data-theme=dark]{--nf-accent:#e879f9;--nf-accent-text:#0d1015;}",
            css);
    }

    [Fact]
    public void Build_WithOnlyADarkAccent_LeavesTheLightDefaultAlone()
    {
        string? css = ForgeAccentCss.Build(new ForgeUiOptions { AccentColorDark = "#e879f9" });

        Assert.Equal("[data-theme=dark]{--nf-accent:#e879f9;--nf-accent-text:#0d1015;}", css);
    }

    [Fact]
    public void Build_WithANonHexColour_SetsTheAccentButNotTheTextColour()
    {
        string? css = ForgeAccentCss.Build(new ForgeUiOptions { AccentColor = "rgb(192 38 211)" });

        Assert.StartsWith(":root{--nf-accent:rgb(192 38 211);}", css);
    }

    [Theory]
    [InlineData("red;}</style><script>alert(1)</script>")]
    [InlineData("url(https://example.test/x)")]
    [InlineData("#fff;background:red")]
    [InlineData("expression(alert(1))")]
    [InlineData("")]
    [InlineData(null)]
    public void Build_RefusesValuesThatCouldBreakOutOfTheStyleElement(string? value) => Assert.Null(ForgeAccentCss.Build(new ForgeUiOptions { AccentColor = value, AccentColorDark = value }));

    [Theory]
    [InlineData("#4f46e5", ForgeAccentCss.LightText)]
    [InlineData("#818cf8", ForgeAccentCss.DarkText)]
    [InlineData("#000", ForgeAccentCss.LightText)]
    [InlineData("#ffffff", ForgeAccentCss.DarkText)]
    [InlineData("#fff8", ForgeAccentCss.DarkText)]
    public void ContrastText_PicksTheMoreReadableColour(string accent, string expected) => Assert.Equal(expected, ForgeAccentCss.ContrastText(accent));

    [Theory]
    [InlineData("rebeccapurple")]
    [InlineData("#12")]
    [InlineData("#zzzzzz")]
    [InlineData(null)]
    public void ContrastText_IsNullWhenTheColourIsNotUsableHex(string? accent) => Assert.Null(ForgeAccentCss.ContrastText(accent));
}

public class IconSetTests
{
    [Theory]
    [InlineData("search", "search")]
    [InlineData("Search", "search")]
    [InlineData("  trash ", "trash")]
    [InlineData("bi-house-door", "home")]
    [InlineData("bi bi-trash", "trash")]
    [InlineData("bi-circle", "circle")]
    [InlineData("bi-box-arrow-up-right", "external")]
    [InlineData("no-such-icon", IconSet.Fallback)]
    [InlineData("", IconSet.Fallback)]
    [InlineData(null, IconSet.Fallback)]
    public void Normalize_ResolvesNamesFromBothIconVocabularies(string? name, string expected) => Assert.Equal(expected, IconSet.Normalize(name));

    [Fact]
    public void EveryIcon_IsParsedIntoAtLeastOneShape()
    {
        Assert.NotEmpty(IconSet.Names);

        foreach (string name in IconSet.Names)
        {
            var shapes = IconSet.GetShapes(name);

            Assert.NotEmpty(shapes);
            Assert.All(shapes, shape => Assert.NotEmpty(shape.Attributes));
        }
    }

    [Theory]
    [InlineData("home")]
    [InlineData("pin-off")]
    [InlineData("library")]
    [InlineData("game")]
    [InlineData("jobs")]
    [InlineData("logs")]
    [InlineData("bell")]
    public void TheAppIconNames_AreStillAvailable(string name) => Assert.Contains(name, IconSet.Names);

    [Fact]
    public void MultiShapeIcons_KeepEveryElementAndAttribute()
    {
        var shapes = IconSet.GetShapes("search");

        Assert.Equal(["circle", "path"], shapes.Select(shape => shape.Tag));
        Assert.Contains(shapes[0].Attributes, attribute => attribute is { Key: "r", Value: "6" });
    }
}

public class ForgeBadgeTests
{
    [Theory]
    [InlineData(null, "nf-badge-muted")]
    [InlineData("", "nf-badge-muted")]
    [InlineData("text-bg-secondary", "nf-badge-muted")]
    [InlineData("text-bg-primary", "nf-badge-accent")]
    [InlineData("text-bg-success rounded-pill", "nf-badge-success")]
    [InlineData("text-bg-danger", "nf-badge-danger")]
    [InlineData("text-bg-warning", "nf-badge-warning")]
    [InlineData("text-bg-info", "nf-badge-info")]
    [InlineData("nf-badge-success", "nf-badge-success")]
    [InlineData("something-else", "nf-badge-muted")]
    public void ToneClass_AcceptsBootstrapAndNativeClasses(string? cssClass, string expected) => Assert.Equal(expected, ForgeBadge.ToneClass(cssClass));
}

public class ToastServiceShownTests
{
    [Fact]
    public void Show_RaisesShownAndKeepsTheToneUnderBothNames()
    {
        using var toasts = new ToastService();
        ToastService.Toast? shown = null;
        toasts.Shown += toast => shown = toast;

        toasts.Show("Saved", "success");

        Assert.NotNull(shown);
        Assert.Equal("Saved", shown.Message);
        Assert.Equal("success", shown.Tone);
        Assert.Equal(shown.Level, shown.Tone);
        Assert.Single(toasts.Toasts);
    }

    [Fact]
    public void Show_DefaultsToInfo()
    {
        using var toasts = new ToastService();

        toasts.Show("FYI");

        Assert.Equal("info", Assert.Single(toasts.Toasts).Level);
    }

    [Fact]
    public void Show_RemovesTheToastWhenItsDurationElapses()
    {
        using var toasts = new ToastService();
        using var dismissed = new ManualResetEventSlim();
        toasts.Changed += () =>
        {
            if (toasts.Toasts.Count == 0)
            {
                dismissed.Set();
            }
        };

        toasts.Show("Short lived", "info", TimeSpan.FromMilliseconds(20));

        Assert.True(dismissed.Wait(TimeSpan.FromSeconds(5)));
    }
}