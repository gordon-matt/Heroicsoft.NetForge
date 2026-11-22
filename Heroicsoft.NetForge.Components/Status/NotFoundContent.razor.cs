using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Status;

/// <summary>
/// Renders a missing-page message, an icon, and a link home, and sets the browser tab title.
/// </summary>
public partial class NotFoundContent
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    /// <summary>
    /// Icon above the heading. Defaults to <c>signpost</c>.
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = "signpost";

    /// <summary>
    /// Heading. Defaults to <c>That page isn't here</c>.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "That page isn't here";

    /// <summary>
    /// Paragraph under the heading. Defaults to a note that the link may be out of date or the target removed.
    /// </summary>
    [Parameter]
    public string Message { get; set; } = "The link may be out of date, or the thing it pointed at may have been removed.";

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

    /// <summary>Overrides the browser tab title. Defaults to "Not found - {Brand}".</summary>
    [Parameter]
    public string? PageTitleText { get; set; }

    /// <summary>
    /// Fills <see cref="PageTitleText"/> when it is <see langword="null"/>: <c>Not found - {brand}</c> when the shell brand is set, otherwise <c>Not found</c>.
    /// </summary>
    protected override void OnParametersSet() =>
        PageTitleText ??= string.IsNullOrWhiteSpace(ShellOptions.Value.Brand)
            ? "Not found"
            : $"Not found - {ShellOptions.Value.Brand}";
}