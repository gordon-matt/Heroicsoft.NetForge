using Heroicsoft.NetForge.Components.Icons;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Represents the page header.
/// </summary>
public partial class PageHeader
{
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>Icon name from <see cref="IconSet"/>. Bootstrap Icons names are mapped.</summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the actions.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}