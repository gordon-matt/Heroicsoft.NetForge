using Heroicsoft.NetForge.Components.Icons;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Status;

public partial class EmptyState
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>Icon name from <see cref="IconSet"/>, for example <c>inbox</c>. Bootstrap Icons names are mapped.</summary>
    [Parameter]
    public string Icon { get; set; } = "inbox";

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}