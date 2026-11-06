using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

/// <summary>
/// Represents the forge router.
/// </summary>
public partial class ForgeRouter
{
    /// <summary>
    /// Gets or sets the app assembly.
    /// </summary>
    [Parameter, EditorRequired]
    public Assembly AppAssembly { get; set; } = default!;

    /// <summary>
    /// Gets or sets the additional assemblies.
    /// </summary>
    [Parameter]
    public IEnumerable<Assembly>? AdditionalAssemblies { get; set; }

    /// <summary>
    /// Gets or sets the not found page.
    /// </summary>
    [Parameter, EditorRequired]
    public Type NotFoundPage { get; set; } = default!;

    /// <summary>
    /// Gets or sets the default layout.
    /// </summary>
    [Parameter]
    public Type DefaultLayout { get; set; } = typeof(ShellLayout);

    /// <summary>
    /// Gets or sets the focus selector.
    /// </summary>
    [Parameter]
    public string FocusSelector { get; set; } = "h1";

    /// <summary>
    /// Gets or sets the authorizing text.
    /// </summary>
    [Parameter]
    public string AuthorizingText { get; set; } = "Checking your access…";

    /// <summary>
    /// Gets or sets the not authorized.
    /// </summary>
    [Parameter]
    public RenderFragment? NotAuthorized { get; set; }
}