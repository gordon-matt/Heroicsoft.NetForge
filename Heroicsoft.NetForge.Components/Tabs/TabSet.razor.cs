using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tabs;

/// <summary>A row of tabs with one visible panel. Put <see cref="Tab"/> components in the child content.</summary>
public partial class TabSet
{
    private readonly string idPrefix = "nf-tabs-" + Guid.NewGuid().ToString("N");
    private readonly List<Tab> tabs = [];
    private string? activeId;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>The <see cref="Tab.Key"/> of the open tab. The first tab is open when this is empty.</summary>
    [Parameter]
    public string? ActiveTab { get; set; }

    [Parameter]
    public EventCallback<string?> ActiveTabChanged { get; set; }

    /// <summary>
    /// Keeps every panel rendered and hides the closed ones, so inputs and child components keep
    /// their state when the user switches tabs.
    /// </summary>
    [Parameter]
    public bool KeepPanelsMounted { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? Class { get; set; }

    protected override void OnParametersSet() => activeId = ActiveTab;

    protected override void OnAfterRender(bool firstRender)
    {
        // Tabs register while the child content renders, after this component has drawn its list.
        if (firstRender && tabs.Count > 0)
        {
            StateHasChanged();
        }
    }

    internal void Register(Tab tab)
    {
        if (!tabs.Contains(tab))
        {
            tabs.Add(tab);
        }
    }

    internal void Unregister(Tab tab)
    {
        if (tabs.Remove(tab))
        {
            StateHasChanged();
        }
    }

    internal bool IsActive(Tab tab)
    {
        var current = tabs.FirstOrDefault(candidate => !candidate.Disabled && string.Equals(candidate.Key, activeId, StringComparison.Ordinal))
            ?? tabs.FirstOrDefault(candidate => !candidate.Disabled);

        return ReferenceEquals(current, tab);
    }

    internal string TabId(Tab tab) => $"{idPrefix}-tab-{tabs.IndexOf(tab)}";

    internal string PanelId(Tab tab) => $"{idPrefix}-panel-{tabs.IndexOf(tab)}";

    private async Task SelectAsync(Tab tab)
    {
        if (tab.Disabled || IsActive(tab))
        {
            return;
        }

        activeId = tab.Key;
        await ActiveTabChanged.InvokeAsync(tab.Key);
    }
}
