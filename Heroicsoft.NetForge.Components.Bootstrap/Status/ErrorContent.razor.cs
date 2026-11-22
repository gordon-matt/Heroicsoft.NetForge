using System.Diagnostics;
using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Bootstrap.Status;

/// <summary>
/// Renders a centered error page with a danger octagon icon, a home button, and a request reference
/// when one is available. Sets the browser tab title.
/// </summary>
public partial class ErrorContent
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    private string? requestId;

    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    /// <summary>
    /// Heading under the icon. Defaults to <c>Something went wrong</c>.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "Something went wrong";

    /// <summary>
    /// Sentence under the heading. Defaults to <c>The error has been logged. Trying again often works.</c>
    /// </summary>
    [Parameter]
    public string Message { get; set; } = "The error has been logged. Trying again often works.";

    /// <summary>
    /// Address of the home button. Defaults to <c>/</c>.
    /// </summary>
    [Parameter]
    public string HomeHref { get; set; } = "/";

    /// <summary>
    /// Text of the home button. Defaults to <c>Back home</c>. The button uses <c>btn btn-primary</c>.
    /// </summary>
    [Parameter]
    public string HomeLabel { get; set; } = "Back home";

    /// <summary>
    /// Icon classes on the home button, after the <c>bi</c> class. Defaults to <c>bi-house-door</c>.
    /// </summary>
    [Parameter]
    public string HomeIcon { get; set; } = "bi-house-door";

    /// <summary>
    /// Overrides the browser tab title. <see langword="null"/> defaults to "Error - {Brand}",
    /// or <c>Error</c> when the shell brand is blank.
    /// </summary>
    [Parameter]
    public string? PageTitleText { get; set; }

    /// <summary>
    /// The correlation id an administrator can search the log for. Taken from the ambient activity
    /// first, because <see cref="Microsoft.AspNetCore.Http.HttpContext"/> is <see langword="null"/> once the page is rendering interactively, then
    /// from the request when this renders during the initial server pass.
    /// </summary>
    protected override void OnInitialized() =>
        requestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;

    /// <summary>
    /// Fills <see cref="PageTitleText"/> from the shell brand when the caller did not set it.
    /// </summary>
    protected override void OnParametersSet() =>
        PageTitleText ??= string.IsNullOrWhiteSpace(ShellOptions.Value.Brand)
            ? "Error"
            : $"Error - {ShellOptions.Value.Brand}";
}