using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tabs;

/// <summary>One tab and its panel inside a <see cref="TabSet"/>.</summary>
public partial class Tab : ComponentBase, IDisposable
{
    [CascadingParameter]
    private TabSet? Parent { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether this instance is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    internal string Key => string.IsNullOrEmpty(Id) ? Title : Id;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (Parent is null)
        {
            throw new InvalidOperationException($"{nameof(Tab)} must be placed inside a {nameof(TabSet)}.");
        }

        Parent.Register(this);
    }

    /// <summary>
    /// Releases the resources used by this instance.
    /// </summary>
    public void Dispose() => Parent?.Unregister(this);
}
