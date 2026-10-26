using Heroicsoft.NetForge.Components.Icons;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Layout;

public partial class PageHeader
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>Icon name from <see cref="IconSet"/>. Bootstrap Icons names are mapped.</summary>
    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}