using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tabs;

/// <summary>One tab and its panel inside a <see cref="TabSet"/>.</summary>
public partial class Tab : ComponentBase, IDisposable
{
    [CascadingParameter]
    private TabSet? Parent { get; set; }

    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>Identifies the tab for <see cref="TabSet.ActiveTab"/>. Defaults to the title.</summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>Icon name from <see cref="Icons.IconSet"/>, shown before the title.</summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>Short text after the title, for example a count.</summary>
    [Parameter]
    public string? Badge { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    internal string Key => string.IsNullOrEmpty(Id) ? Title : Id;

    protected override void OnInitialized()
    {
        if (Parent is null)
        {
            throw new InvalidOperationException($"{nameof(Tab)} must be placed inside a {nameof(TabSet)}.");
        }

        Parent.Register(this);
    }

    public void Dispose() => Parent?.Unregister(this);
}
