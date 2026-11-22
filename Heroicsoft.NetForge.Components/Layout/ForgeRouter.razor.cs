using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>
/// Renders the router, moves focus after navigation, shows <see cref="Heroicsoft.NetForge.Components.Status.AuthorizingMessage"/> while authorizing, and sends anonymous users to sign-in or signed-in users to <see cref="Heroicsoft.NetForge.Components.Status.AccessDenied"/>.
/// </summary>
public partial class ForgeRouter
{
    /// <summary>
    /// Assembly whose routes are discovered. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public Assembly AppAssembly { get; set; } = default!;

    /// <summary>
    /// More assemblies to scan for routes. <see langword="null"/> scans only <see cref="AppAssembly"/>.
    /// </summary>
    [Parameter]
    public IEnumerable<Assembly>? AdditionalAssemblies { get; set; }

    /// <summary>
    /// Page shown when no route matches. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public Type NotFoundPage { get; set; } = default!;

    /// <summary>
    /// Layout for pages that do not declare one. Defaults to <see cref="ShellLayout"/>.
    /// </summary>
    [Parameter]
    public Type DefaultLayout { get; set; } = typeof(ShellLayout);

    /// <summary>
    /// Element focused after navigation. Defaults to <c>h1</c>.
    /// </summary>
    [Parameter]
    public string FocusSelector { get; set; } = "h1";

    /// <summary>
    /// Message shown while authorization is in progress. Defaults to <c>Checking your access.</c>
    /// </summary>
    [Parameter]
    public string AuthorizingText { get; set; } = "Checking your access.";

    /// <summary>Replaces the default: sign-in for anonymous users, <c>AccessDenied</c> for signed-in ones.</summary>
    [Parameter]
    public RenderFragment? NotAuthorized { get; set; }
}