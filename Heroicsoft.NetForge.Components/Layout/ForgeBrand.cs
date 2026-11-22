namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Host branding for every NetForge surface. Libraries read <see cref="ForgeShellOptions"/>
/// through this, so one logo and name cover the shell and every Razor Class Library.
/// A library document's base href is its mount path, so a relative logo would be requested
/// from there. These helpers root it on the host site instead.
/// </summary>
public static class ForgeBrand
{
    /// <summary>
    /// Root-relative or absolute form of <paramref name="path"/>. Empty is no image.
    /// <c>~/logo.png</c>, <c>img/logo.png</c>, and <c>/img/logo.png</c> all become a site-root path.
    /// </summary>
    public static string? Url(string? path)
    {
        path = path?.Trim();

        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (path.StartsWith("~/", StringComparison.Ordinal))
        {
            path = path[1..];
        }

        return path.StartsWith('/') || Uri.TryCreate(path, UriKind.Absolute, out _) ? path : "/" + path;
    }

    /// <summary>
    /// Root-relative or absolute form of <see cref="ForgeShellOptions.LogoUrl"/>. <see langword="null"/> when the host set no logo.
    /// </summary>
    /// <param name="options">Shell branding to read.</param>
    /// <returns>The logo address, or <see langword="null"/>.</returns>
    public static string? Logo(ForgeShellOptions options) => Url(options.LogoUrl);

    /// <summary>
    /// Root-relative or absolute form of <see cref="ForgeShellOptions.LogoDarkUrl"/>. <see langword="null"/> when the host set no dark logo.
    /// </summary>
    /// <param name="options">Shell branding to read.</param>
    /// <returns>The dark-theme logo address, or <see langword="null"/>.</returns>
    public static string? LogoDark(ForgeShellOptions options) => Url(options.LogoDarkUrl);

    /// <summary>
    /// Root-relative or absolute form of <see cref="ForgeShellOptions.MarkUrl"/>. <see langword="null"/> when the host set no mark.
    /// </summary>
    /// <param name="options">Shell branding to read.</param>
    /// <returns>The mark address, or <see langword="null"/>.</returns>
    public static string? Mark(ForgeShellOptions options) => Url(options.MarkUrl);

    /// <summary>Brand text. Empty when the host has not set <see cref="ForgeShellOptions.Brand"/>.</summary>
    public static string? Name(ForgeShellOptions options) =>
        string.IsNullOrWhiteSpace(options.Brand) ? null : options.Brand.Trim();

    /// <summary>
    /// Where the logo goes. <see cref="ForgeShellOptions.BrandHref"/>, rooted so a library document's
    /// base href cannot keep the click inside the library. Empty is the host root.
    /// </summary>
    public static string Home(ForgeShellOptions options)
    {
        string? href = options.BrandHref?.Trim();

        return string.IsNullOrEmpty(href) || href is "." or "./" ? "/" : Url(href) ?? "/";
    }
}