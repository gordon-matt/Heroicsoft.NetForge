using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Renders the shell sidebar: a brand link, navigation sections, an optional shared menu, and an optional footer.
/// </summary>
public partial class ForgeSidebar
{
    /// <summary>
    /// Brand text, shown when <see cref="BrandContent"/> and <see cref="LogoUrl"/> are not set.
    /// Also the alt text of <see cref="LogoUrl"/>. <see langword="null"/> or blank omits the text.
    /// </summary>
    [Parameter]
    public string? Brand { get; set; }

    /// <summary>
    /// Address of the brand link. Defaults to <c>/</c>.
    /// </summary>
    [Parameter]
    public string BrandHref { get; set; } = "/";

    /// <summary>
    /// Logo image. <see langword="null"/> falls through to <see cref="Brand"/> text when
    /// <see cref="BrandContent"/> is also not set.
    /// </summary>
    [Parameter]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Dark-theme logo shown with <see cref="LogoUrl"/>. <see langword="null"/> omits it.
    /// Ignored when <see cref="LogoUrl"/> is <see langword="null"/>.
    /// </summary>
    [Parameter]
    public string? LogoDarkUrl { get; set; }

    /// <summary>
    /// Extra image drawn with <see cref="LogoUrl"/>, using <c>bf-brand-mark</c>. <see langword="null"/> omits it.
    /// Ignored when <see cref="LogoUrl"/> is <see langword="null"/>.
    /// </summary>
    [Parameter]
    public string? MarkUrl { get; set; }

    /// <summary>
    /// Navigation groups, each rendered by <see cref="ForgeNavSectionView"/>. An empty list, the default,
    /// draws no host sections. A section whose <see cref="ForgeNavSection.Roles"/> is set is omitted for
    /// users outside those roles.
    /// </summary>
    [Parameter]
    public IReadOnlyList<ForgeNavSection> Sections { get; set; } = [];

    /// <summary>
    /// Extra navigation rendered with <see cref="Sections"/>. <see langword="null"/> draws nothing.
    /// </summary>
    [Parameter]
    public RenderFragment? Menu { get; set; }

    /// <summary>
    /// Draws <see cref="Menu"/> after <see cref="Sections"/>. Defaults to <see langword="false"/>, which draws the menu first.
    /// </summary>
    [Parameter]
    public bool SharedMenuAfterSections { get; set; }

    /// <summary>
    /// Replaces the logo and brand text inside the brand link. <see langword="null"/> uses
    /// <see cref="LogoUrl"/> or <see cref="Brand"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? BrandContent { get; set; }

    /// <summary>
    /// Pinned under the links, inside a footer with a top border. <see langword="null"/> omits the footer.
    /// </summary>
    [Parameter]
    public RenderFragment? SidebarFooter { get; set; }
}