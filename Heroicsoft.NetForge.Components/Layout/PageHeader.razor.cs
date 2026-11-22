using Heroicsoft.NetForge.Components.Icons;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Renders the page title, an optional icon and description, optional extra text, and an actions slot.
/// </summary>
public partial class PageHeader
{
    /// <summary>
    /// Text of the heading. Required. The default is empty.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>Icon name from <see cref="IconSet"/>. Bootstrap Icons names are mapped. <see langword="null"/> hides the icon.</summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// Paragraph under the title. <see langword="null"/> hides it.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Buttons at the end of the header. <see langword="null"/> hides the actions block.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Markup under the description, in the text column. <see langword="null"/> renders nothing there.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}