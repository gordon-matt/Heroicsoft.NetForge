using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tabs;

/// <summary>One tab and its panel inside a <see cref="TabSet"/>.</summary>
public partial class Tab : ComponentBase, IDisposable
{
    [CascadingParameter]
    private TabSet? Parent { get; set; }

    /// <summary>
    /// Text of the tab button. Required. Also the key when <see cref="Id"/> is <see langword="null"/> or empty. The default is empty.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>Identifies the tab for <see cref="TabSet.ActiveTab"/>. Defaults to the title.</summary>
    [Parameter]
    public string? Id { get; set; }

    /// <summary>Icon name from <see cref="Icons.IconSet"/>, shown before the title. <see langword="null"/> or empty hides the icon.</summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>Short text after the title, for example a count. <see langword="null"/> or empty hides the badge.</summary>
    [Parameter]
    public string? Badge { get; set; }

    /// <summary>
    /// Disables the tab button and keeps the tab from becoming active. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// The panel. Hidden while another tab is active, unless the parent keeps every panel mounted.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    internal string Key => string.IsNullOrEmpty(Id) ? Title : Id;

    /// <summary>
    /// Registers this tab with the parent <see cref="TabSet"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">This tab is not inside a <see cref="TabSet"/>.</exception>
    protected override void OnInitialized()
    {
        if (Parent is null)
        {
            throw new InvalidOperationException($"{nameof(Tab)} must be placed inside a {nameof(TabSet)}.");
        }

        Parent.Register(this);
    }

    /// <summary>Removes this tab from the parent tab set.</summary>
    public void Dispose() => Parent?.Unregister(this);
}
