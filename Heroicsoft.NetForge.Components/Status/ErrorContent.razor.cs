using System.Diagnostics;
using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Status;

/// <summary>
/// Renders an error heading, a message, a log reference when one is available, and a link home, and sets the browser tab title.
/// </summary>
public partial class ErrorContent
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    private string? requestId;

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    /// <summary>
    /// Heading. Defaults to <c>Something went wrong</c>.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Something went wrong";

    /// <summary>
    /// Paragraph under the heading. Defaults to a note that the error was logged and trying again often works.
    /// </summary>
    [Parameter]
    public string Message { get; set; } = "The error has been logged. Trying again often works.";

    /// <summary>
    /// Address of the home link. Defaults to <c>/</c>.
    /// </summary>
    [Parameter]
    public string HomeHref { get; set; } = "/";

    /// <summary>
    /// Text of the home link. Defaults to <c>Back home</c>.
    /// </summary>
    [Parameter]
    public string HomeLabel { get; set; } = "Back home";

    /// <summary>
    /// Icon inside the home link. Defaults to <c>home</c>.
    /// </summary>
    [Parameter]
    public string HomeIcon { get; set; } = "home";

    /// <summary>Overrides the browser tab title. Defaults to "Error - {Brand}".</summary>
    [Parameter]
    public string? PageTitleText { get; set; }

    /// <summary>
    /// The correlation id an administrator can search the log for. Taken from the ambient activity
    /// first, because <see cref="HttpContext"/> is null once the page is rendering interactively, then
    /// from the request when this renders during the initial server pass.
    /// </summary>
    protected override void OnInitialized() =>
        requestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;

    /// <summary>
    /// Fills <see cref="PageTitleText"/> when it is <see langword="null"/>: <c>Error - {brand}</c> when the shell brand is set, otherwise <c>Error</c>.
    /// </summary>
    protected override void OnParametersSet() =>
        PageTitleText ??= string.IsNullOrWhiteSpace(ShellOptions.Value.Brand)
            ? "Error"
            : $"Error - {ShellOptions.Value.Brand}";
}