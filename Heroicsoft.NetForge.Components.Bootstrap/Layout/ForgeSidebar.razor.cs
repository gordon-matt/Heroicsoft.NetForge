using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

public partial class ForgeSidebar
{
    [Parameter]
    public string? Brand { get; set; }

    [Parameter]
    public string BrandHref { get; set; } = "/";

    [Parameter]
    public string? LogoUrl { get; set; }

    [Parameter]
    public string? LogoDarkUrl { get; set; }

    [Parameter]
    public string? MarkUrl { get; set; }

    [Parameter]
    public IReadOnlyList<ForgeNavSection> Sections { get; set; } = [];

    [Parameter]
    public RenderFragment? BrandContent { get; set; }

    /// <summary>Pinned under the links.</summary>
    [Parameter]
    public RenderFragment? SidebarFooter { get; set; }
}