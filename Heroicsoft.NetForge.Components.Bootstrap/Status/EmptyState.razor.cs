using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Status;

public partial class EmptyState
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>Bootstrap Icons class, for example <c>bi-inbox</c>.</summary>
    [Parameter]
    public string Icon { get; set; } = "bi-inbox";

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string IconClass => Icon.StartsWith("bi-", StringComparison.Ordinal) || Icon.StartsWith("bi ", StringComparison.Ordinal)
        ? Icon
        : $"bi-{Icon}";
}