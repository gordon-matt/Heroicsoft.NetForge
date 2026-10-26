using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

public partial class PageHeader
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string IconClass => Icon is null
        ? string.Empty
        : Icon.StartsWith("bi-", StringComparison.Ordinal) || Icon.StartsWith("bi ", StringComparison.Ordinal)
            ? Icon
            : $"bi-{Icon}";
}