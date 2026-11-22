using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Status;

/// <summary>
/// Renders a centered icon, title, optional description, and optional actions for a page with nothing else to show.
/// </summary>
public partial class EmptyState
{
    /// <summary>
    /// Primary sentence under the icon. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Bootstrap Icons class, for example <c>bi-inbox</c>. A value without a <c>bi-</c> or <c>bi </c>
    /// prefix gets <c>bi-</c> prepended. Defaults to <c>bi-inbox</c>.
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = "bi-inbox";

    /// <summary>
    /// Secondary sentence under the title. <see langword="null"/> omits it.
    /// </summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>
    /// Markup under the description, such as a button. <see langword="null"/> draws none.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string IconClass => Icon.StartsWith("bi-", StringComparison.Ordinal) || Icon.StartsWith("bi ", StringComparison.Ordinal)
        ? Icon
        : $"bi-{Icon}";
}