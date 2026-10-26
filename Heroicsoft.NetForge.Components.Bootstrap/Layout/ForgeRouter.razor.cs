using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Layout;

public partial class ForgeRouter
{
    [Parameter, EditorRequired]
    public Assembly AppAssembly { get; set; } = default!;

    [Parameter]
    public IEnumerable<Assembly>? AdditionalAssemblies { get; set; }

    [Parameter, EditorRequired]
    public Type NotFoundPage { get; set; } = default!;

    [Parameter]
    public Type DefaultLayout { get; set; } = typeof(ShellLayout);

    [Parameter]
    public string FocusSelector { get; set; } = "h1";

    [Parameter]
    public string AuthorizingText { get; set; } = "Checking your access…";

    [Parameter]
    public RenderFragment? NotAuthorized { get; set; }
}