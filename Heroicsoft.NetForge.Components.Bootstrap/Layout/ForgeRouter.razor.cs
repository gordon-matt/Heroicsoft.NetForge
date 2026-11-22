using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Renders the app router with authorization. A signed-in user who lacks the role sees
/// <see cref="Heroicsoft.NetForge.Components.Bootstrap.Status.AccessDenied"/>. A signed-out user is
/// sent to sign in. While authorization is running, <see cref="Heroicsoft.NetForge.Components.Bootstrap.Status.AuthorizingMessage"/>
/// shows <see cref="AuthorizingText"/>. After navigation, focus moves to <see cref="FocusSelector"/>.
/// </summary>
public partial class ForgeRouter
{
    /// <summary>
    /// Assembly whose routable components are discovered. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public Assembly AppAssembly { get; set; } = default!;

    /// <summary>
    /// More assemblies to scan for routes. <see langword="null"/> scans only <see cref="AppAssembly"/>.
    /// </summary>
    [Parameter]
    public IEnumerable<Assembly>? AdditionalAssemblies { get; set; }

    /// <summary>
    /// Component shown when no route matches. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public Type NotFoundPage { get; set; } = default!;

    /// <summary>
    /// Layout applied to pages that do not declare one. Defaults to <see cref="ShellLayout"/>.
    /// </summary>
    [Parameter]
    public Type DefaultLayout { get; set; } = typeof(ShellLayout);

    /// <summary>
    /// CSS selector focused after each navigation. Defaults to <c>h1</c>.
    /// </summary>
    [Parameter]
    public string FocusSelector { get; set; } = "h1";

    /// <summary>
    /// Sentence beside the spinner while authorization is in progress. Defaults to <c>Checking your access…</c>.
    /// </summary>
    [Parameter]
    public string AuthorizingText { get; set; } = "Checking your access…";

    /// <summary>
    /// Replaces the default unauthorized content. <see langword="null"/> shows an access-denied page for a
    /// signed-in user and redirects a signed-out user to sign in.
    /// </summary>
    [Parameter]
    public RenderFragment? NotAuthorized { get; set; }
}