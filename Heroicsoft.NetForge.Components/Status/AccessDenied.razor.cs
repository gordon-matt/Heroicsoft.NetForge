using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Status;

public partial class AccessDenied
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    [Parameter]
    public string Icon { get; set; } = "shield";

    [Parameter]
    public string Title { get; set; } = "You don't have access to this";

    [Parameter]
    public string Description { get; set; } = "Ask an administrator if you think you should.";

    [Parameter]
    public string HomeHref { get; set; } = "/";

    [Parameter]
    public string HomeLabel { get; set; } = "Back home";

    /// <summary>Overrides the browser tab title. Defaults to "Access denied - {Brand}".</summary>
    [Parameter]
    public string? PageTitleText { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void OnParametersSet() =>
        PageTitleText ??= string.IsNullOrWhiteSpace(ShellOptions.Value.Brand)
            ? "Access denied"
            : $"Access denied - {ShellOptions.Value.Brand}";
}