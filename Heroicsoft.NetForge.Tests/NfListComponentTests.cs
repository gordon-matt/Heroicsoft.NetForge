using Heroicsoft.NetForge.Components.Forms;
using Heroicsoft.NetForge.Components.Menus;
using Heroicsoft.NetForge.Components.Tables;
using Heroicsoft.NetForge.Components.Tabs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Tests;

/// <summary>Tabs, switches, sortable headers, tree tables and action menus rendered to HTML.</summary>
public class NfListComponentTests
{
    private sealed record Node(string Name, params Node[] Children);

    private sealed class NoJSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => ValueTask.FromResult(default(TValue)!);
    }

    private static async Task<string> RenderAsync<TComponent>(Dictionary<string, object?> parameters)
        where TComponent : IComponent
    {
        await using var services = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
            .AddSingleton<IJSRuntime, NoJSRuntime>()
            .BuildServiceProvider();

        await using var renderer = new HtmlRenderer(services, NullLoggerFactory.Instance);

        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters));
            return output.ToHtmlString();
        });
    }

    private static RenderFragment TwoTabs => builder =>
    {
        AddTab(builder, 0, "first", "First body");
        AddTab(builder, 10, "second", "Second body");
    };

    private static void AddTab(RenderTreeBuilder builder, int sequence, string id, string body)
    {
        builder.OpenComponent<Tab>(sequence);
        builder.AddAttribute(sequence + 1, nameof(Tab.Title), id);
        builder.AddAttribute(sequence + 2, nameof(Tab.Id), id);
        builder.AddAttribute(sequence + 3, nameof(Tab.ChildContent), (RenderFragment)(content => content.AddContent(0, body)));
        builder.CloseComponent();
    }

    [Fact]
    public void Toggle_SameField_FlipsDirection()
    {
        var sort = TableSort.Toggle(new TableSort("Name"), "Name");

        Assert.Equal(new TableSort("Name", Descending: true), sort);
    }

    [Fact]
    public void Toggle_NewField_StartsAscending()
    {
        var sort = TableSort.Toggle(new TableSort("Name", Descending: true), "Created");

        Assert.Equal(new TableSort("Created"), sort);
    }

    [Fact]
    public async Task SortHeader_SortedDescending_ReportsItToAssistiveTechnology()
    {
        string html = await RenderAsync<SortHeader>(new()
        {
            ["Field"] = "Name",
            ["Sort"] = new TableSort("Name", Descending: true),
        });

        Assert.Contains("aria-sort=\"descending\"", html);
        Assert.Contains("is-desc", html);
    }

    [Fact]
    public async Task SortHeader_OtherColumnSorted_ReportsNone()
    {
        string html = await RenderAsync<SortHeader>(new()
        {
            ["Field"] = "Name",
            ["Sort"] = new TableSort("Created"),
        });

        Assert.Contains("aria-sort=\"none\"", html);
    }

    [Fact]
    public async Task Switch_On_RendersACheckedSwitchRole()
    {
        string html = await RenderAsync<Switch>(new() { ["Value"] = true, ["Label"] = "Enabled", ["Description"] = "Runs on schedule" });

        Assert.Contains("role=\"switch\"", html);
        Assert.Contains("checked", html);
        Assert.Contains("Enabled", html);
        Assert.Contains("Runs on schedule", html);
    }

    [Fact]
    public async Task TabSet_NoActiveTab_ShowsOnlyTheFirstPanel()
    {
        string html = await RenderAsync<TabSet>(new() { ["ChildContent"] = TwoTabs });

        Assert.Contains("First body", html);
        Assert.DoesNotContain("Second body", html);
    }

    [Fact]
    public async Task TabSet_ActiveTabSet_ShowsThatPanel()
    {
        string html = await RenderAsync<TabSet>(new() { ["ChildContent"] = TwoTabs, ["ActiveTab"] = "second" });

        Assert.Contains("Second body", html);
        Assert.DoesNotContain("First body", html);
    }

    [Fact]
    public async Task TabSet_KeepPanelsMounted_RendersClosedPanelsHidden()
    {
        string html = await RenderAsync<TabSet>(new() { ["ChildContent"] = TwoTabs, ["KeepPanelsMounted"] = true });

        Assert.Contains("First body", html);
        Assert.Contains("Second body", html);
        Assert.Contains("hidden", html[..html.IndexOf("Second body", StringComparison.Ordinal)]);
    }

    private static Dictionary<string, object?> TreeParameters(IEnumerable<Node> roots, bool expanded) => new()
    {
        ["Items"] = roots,
        ["ChildrenOf"] = (Func<Node, IEnumerable<Node>>)(node => node.Children),
        ["KeyOf"] = (Func<Node, object>)(node => node.Name),
        ["ExpandedByDefault"] = expanded,
        ["RowTemplate"] = (RenderFragment<TreeTableRow<Node>>)(row => builder =>
        {
            builder.OpenElement(0, "tr");
            builder.AddContent(1, $"{row.Item.Name}@{row.Depth}");
            builder.CloseElement();
        }),
    };

    [Fact]
    public async Task TreeTable_ExpandedByDefault_RendersEveryLevelWithItsDepth()
    {
        Node[] roots = [new("home", new Node("about", new Node("team"))), new("contact")];

        string html = await RenderAsync<TreeTable<Node>>(TreeParameters(roots, expanded: true));

        Assert.Contains("home@0", html);
        Assert.Contains("about@1", html);
        Assert.Contains("team@2", html);
        Assert.Contains("contact@0", html);
        Assert.True(html.IndexOf("about@1", StringComparison.Ordinal) < html.IndexOf("contact@0", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TreeTable_CollapsedByDefault_RendersOnlyRoots()
    {
        Node[] roots = [new("home", new Node("about"))];

        string html = await RenderAsync<TreeTable<Node>>(TreeParameters(roots, expanded: false));

        Assert.Contains("home@0", html);
        Assert.DoesNotContain("about@1", html);
    }

    [Fact]
    public async Task ActionMenu_Closed_RendersOnlyTheTrigger()
    {
        string html = await RenderAsync<ActionMenu>(new() { ["Label"] = "Page actions" });

        Assert.Contains("aria-label=\"Page actions\"", html);
        Assert.Contains("aria-expanded=\"false\"", html);
        Assert.DoesNotContain("role=\"menu\"", html);
    }

    [Fact]
    public async Task ActionMenuItem_WithHref_RendersALink()
    {
        string html = await RenderAsync<ActionMenuItem>(new() { ["Text"] = "Design", ["Href"] = "/pages/1", ["Icon"] = "edit" });

        Assert.Contains("<a", html);
        Assert.Contains("href=\"/pages/1\"", html);
        Assert.Contains("Design", html);
    }

    [Fact]
    public async Task ActionMenuItem_Danger_UsesTheDangerTone()
    {
        string html = await RenderAsync<ActionMenuItem>(new() { ["Text"] = "Delete", ["Danger"] = true });

        Assert.Contains("<button", html);
        Assert.Contains("is-danger", html);
    }
}
