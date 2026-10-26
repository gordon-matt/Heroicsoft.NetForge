using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Status;

public partial class NotFoundContent
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    [Parameter]
    public string Icon { get; set; } = "signpost";

    [Parameter]
    public string Title { get; set; } = "That page isn't here";

    [Parameter]
    public string Message { get; set; } = "The link may be out of date, or the thing it pointed at may have been removed.";

    [Parameter]
    public string HomeHref { get; set; } = "/";

    [Parameter]
    public string HomeLabel { get; set; } = "Back home";

    [Parameter]
    public string HomeIcon { get; set; } = "home";

    /// <summary>Overrides the browser tab title. Defaults to "Not found - {Brand}".</summary>
    [Parameter]
    public string? PageTitleText { get; set; }

    protected override void OnParametersSet() =>
        PageTitleText ??= string.IsNullOrWhiteSpace(ShellOptions.Value.Brand)
            ? "Not found"
            : $"Not found - {ShellOptions.Value.Brand}";
}