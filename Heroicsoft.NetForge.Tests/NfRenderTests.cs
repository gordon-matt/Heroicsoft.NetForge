using Heroicsoft.NetForge.Components.Icons;
using Heroicsoft.NetForge.Components.Paging;
using Heroicsoft.NetForge.Components.Status;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Heroicsoft.NetForge.Tests;

/// <summary>Renders components to HTML to catch markup that compiles but emits the wrong thing.</summary>
public class NfRenderTests
{
    private static async Task<string> RenderAsync<TComponent>(Dictionary<string, object?> parameters)
        where TComponent : IComponent
    {
        await using var services = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
            .BuildServiceProvider();

        await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters));
            return output.ToHtmlString();
        });
    }

    [Fact]
    public async Task Icon_RendersAnSvgWithEveryShapeAndIsHiddenFromAssistiveTechnology()
    {
        string html = await RenderAsync<Icon>(new() { ["Name"] = "search", ["Size"] = 20, ["Class"] = "extra" });

        Assert.Contains("<svg", html);
        Assert.Contains("class=\"nf-icon extra\"", html);
        Assert.Contains("width=\"20\"", html);
        Assert.Contains("<circle", html);
        Assert.Contains("<path", html);
        Assert.Contains("aria-hidden=\"true\"", html);
    }

    [Fact]
    public async Task Icon_WithATitle_IsExposedAsAnImage()
    {
        string html = await RenderAsync<Icon>(new() { ["Name"] = "bell", ["Title"] = "Notifications" });

        Assert.Contains("role=\"img\"", html);
        Assert.Contains("<title>Notifications</title>", html);
        Assert.DoesNotContain("aria-hidden", html);
    }

    [Fact]
    public async Task Icon_WithAnUnknownName_DrawsTheFallbackCircle()
    {
        string html = await RenderAsync<Icon>(new() { ["Name"] = "definitely-not-an-icon" });

        Assert.Contains("<circle", html);
    }

    [Fact]
    public async Task Pager_ShowsTheRangeAndOnlyAWindowOfPageButtons()
    {
        string html = await RenderAsync<Pager>(new()
        {
            ["PageNumber"] = 2,
            ["PageSize"] = 10,
            ["TotalCount"] = 95,
        });

        Assert.Contains("11-20", html);
        Assert.Contains("of 95", html);
        Assert.Contains("aria-current=\"page\"", html);
        Assert.Contains("aria-label=\"Next page\"", html);
        Assert.DoesNotContain("nf-pager-size", html);
    }

    [Fact]
    public async Task Pager_DisablesPreviousOnTheFirstPage()
    {
        string html = await RenderAsync<Pager>(new()
        {
            ["PageNumber"] = 1,
            ["PageSize"] = 10,
            ["TotalCount"] = 95,
        });

        int previous = html.IndexOf("aria-label=\"Previous page\"", StringComparison.Ordinal);
        string button = html[html.LastIndexOf("<button", previous, StringComparison.Ordinal)..previous];

        Assert.Contains("disabled", button);
    }

    [Fact]
    public async Task EmptyState_RendersTitleDescriptionAndIcon()
    {
        string html = await RenderAsync<EmptyState>(new()
        {
            ["Title"] = "Nothing here",
            ["Description"] = "Add something.",
            ["Icon"] = "bi-inbox",
        });

        Assert.Contains("Nothing here", html);
        Assert.Contains("Add something.", html);
        Assert.Contains("nf-empty-icon", html);
    }
}