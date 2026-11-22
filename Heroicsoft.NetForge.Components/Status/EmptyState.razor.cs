using Heroicsoft.NetForge.Components.Icons;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Status;

/// <summary>
/// Renders an icon, a heading, an optional description, and optional extra content for a list or page with nothing to show.
/// </summary>
public partial class EmptyState
{
    /// <summary>
    /// Heading. Required. The default is empty, so the heading is blank until a title is set.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>Icon name from <see cref="IconSet"/>, for example <c>inbox</c>. Bootstrap Icons names are mapped. Defaults to <c>inbox</c>.</summary>
    [Parameter]
    public string Icon { get; set; } = "inbox";

    /// <summary>
    /// Paragraph under the heading. <see langword="null"/> hides it.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Markup under the description, such as a button. <see langword="null"/> hides that block.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}