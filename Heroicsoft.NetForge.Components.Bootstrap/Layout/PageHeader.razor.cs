using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Renders the page title row: an <c>h1</c>, an optional description, optional body content, and actions aligned to the end.
/// </summary>
public partial class PageHeader
{
    /// <summary>
    /// Text of the <c>h1</c>. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Bootstrap Icons class drawn before the title in <c>text-primary</c>. A value without a <c>bi-</c> or
    /// <c>bi </c> prefix gets <c>bi-</c> prepended. <see langword="null"/> omits the icon.
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// Secondary sentence under the title. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Markup aligned to the end of the header. <see langword="null"/> omits that group.
    /// </summary>
    [Parameter]
    public RenderFragment? Actions { get; set; }

    /// <summary>
    /// Markup under the description, still in the title column. <see langword="null"/> draws nothing there.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string IconClass => Icon is null
        ? string.Empty
        : Icon.StartsWith("bi-", StringComparison.Ordinal) || Icon.StartsWith("bi ", StringComparison.Ordinal)
            ? Icon
            : $"bi-{Icon}";
}