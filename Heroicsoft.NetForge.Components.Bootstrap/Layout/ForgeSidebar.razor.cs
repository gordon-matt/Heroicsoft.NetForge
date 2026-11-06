using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Represents the forge sidebar.
/// </summary>
public partial class ForgeSidebar
{
    /// <summary>
    /// Gets or sets the brand.
    /// </summary>
    [Parameter]
    public string? Brand { get; set; }

    /// <summary>
    /// Gets or sets the brand href.
    /// </summary>
    [Parameter]
    public string BrandHref { get; set; } = "/";

    /// <summary>
    /// Gets or sets the logo URL.
    /// </summary>
    [Parameter]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Gets or sets the logo dark URL.
    /// </summary>
    [Parameter]
    public string? LogoDarkUrl { get; set; }

    /// <summary>
    /// Gets or sets the mark URL.
    /// </summary>
    [Parameter]
    public string? MarkUrl { get; set; }

    /// <summary>
    /// Gets or sets the sections.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ForgeNavSection> Sections { get; set; } = [];

    /// <summary>Replaces <see cref="Sections"/>. Used by the shell when a shared menu is switched on.</summary>
    [Parameter]
    public RenderFragment? Menu { get; set; }

    /// <summary>Draws <see cref="Menu"/> after <see cref="Sections"/>.</summary>
    [Parameter]
    public bool SharedMenuAfterSections { get; set; }

    /// <summary>
    /// Gets or sets the brand content.
    /// </summary>
    [Parameter]
    public RenderFragment? BrandContent { get; set; }

    /// <summary>Pinned under the links.</summary>
    [Parameter]
    public RenderFragment? SidebarFooter { get; set; }
}