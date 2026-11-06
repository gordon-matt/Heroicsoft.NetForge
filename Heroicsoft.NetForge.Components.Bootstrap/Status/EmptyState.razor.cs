using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Status;

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

    /// <summary>Bootstrap Icons class, for example <c>bi-inbox</c>.</summary>
    [Parameter]
    public string Icon { get; set; } = "bi-inbox";

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

    private string IconClass => Icon.StartsWith("bi-", StringComparison.Ordinal) || Icon.StartsWith("bi ", StringComparison.Ordinal)
        ? Icon
        : $"bi-{Icon}";
}