using Heroicsoft.NetForge.Components.Icons;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Status;

/// <summary>
/// Represents the empty state.
/// </summary>
public partial class EmptyState
{
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>Icon name from <see cref="IconSet"/>, for example <c>inbox</c>. Bootstrap Icons names are mapped.</summary>
    [Parameter]
    public string Icon { get; set; } = "inbox";

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}