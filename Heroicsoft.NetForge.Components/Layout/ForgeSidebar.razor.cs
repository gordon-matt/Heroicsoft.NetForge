using Heroicsoft.NetForge.Abstractions.Navigation;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Renders the brand and the sidebar links, with the shared menu before or after the host sections.
/// </summary>
public partial class ForgeSidebar
{
    /// <summary>
    /// Name shown when there is no logo and no <see cref="BrandContent"/>. Also the alt text of the logo. <see langword="null"/> or whitespace hides the text.
    /// </summary>
    [Parameter]
    public string? Brand { get; set; }

    /// <summary>
    /// Address of the brand link. Defaults to <c>/</c>.
    /// </summary>
    [Parameter]
    public string BrandHref { get; set; } = "/";

    /// <summary>
    /// <see langword="false"/> makes the brand link a full page load. Defaults to <see langword="true"/>, which leaves enhanced navigation on.
    /// </summary>
    [Parameter]
    public bool EnhanceBrandNavigation { get; set; } = true;

    /// <summary>
    /// Image shown as the brand. <see langword="null"/> falls through to the brand text when <see cref="BrandContent"/> is <see langword="null"/>.
    /// </summary>
    [Parameter]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Image used in the dark theme. <see langword="null"/> uses <see cref="LogoUrl"/> for both themes.
    /// </summary>
    [Parameter]
    public string? LogoDarkUrl { get; set; }

    /// <summary>
    /// Compact mark shown beside the logo. <see langword="null"/> hides it.
    /// </summary>
    [Parameter]
    public string? MarkUrl { get; set; }

    /// <summary>
    /// Host sections under the brand. A section with roles is shown only to users in those roles. Defaults to an empty list.
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
    /// Replaces the logo and brand text. <see langword="null"/> uses the logo or the name.
    /// </summary>
    [Parameter]
    public RenderFragment? BrandContent { get; set; }

    /// <summary>Pinned under the links, for example a user name and sign-out button. Hidden when the rail is collapsed.</summary>
    [Parameter]
    public RenderFragment? SidebarFooter { get; set; }
}