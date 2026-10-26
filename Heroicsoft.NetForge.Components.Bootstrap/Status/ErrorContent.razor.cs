using System.Diagnostics;
using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Bootstrap.Status;

public partial class ErrorContent
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    private string? requestId;

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    [Parameter]
    public string Title { get; set; } = "Something went wrong";

    [Parameter]
    public string Message { get; set; } = "The error has been logged. Trying again often works.";

    [Parameter]
    public string HomeHref { get; set; } = "/";

    [Parameter]
    public string HomeLabel { get; set; } = "Back home";

    [Parameter]
    public string HomeIcon { get; set; } = "bi-house-door";

    /// <summary>Overrides the browser tab title. Defaults to "Error - {Brand}".</summary>
    [Parameter]
    public string? PageTitleText { get; set; }

    /// <summary>
    /// The correlation id an administrator can search the log for. Taken from the ambient activity
    /// first, because <c>HttpContext</c> is null once the page is rendering interactively, then
    /// from the request when this renders during the initial server pass.
    /// </summary>
    protected override void OnInitialized() =>
        requestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;

    protected override void OnParametersSet() =>
        PageTitleText ??= string.IsNullOrWhiteSpace(ShellOptions.Value.Brand)
            ? "Error"
            : $"Error - {ShellOptions.Value.Brand}";
}