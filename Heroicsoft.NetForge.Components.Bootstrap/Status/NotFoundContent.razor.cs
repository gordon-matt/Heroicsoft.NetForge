using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Bootstrap.Status;

/// <summary>
/// Renders a centered not-found page with a large icon, a short explanation, and a home button.
/// Sets the browser tab title.
/// </summary>
public partial class NotFoundContent
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    /// <summary>
    /// Icon classes on the large icon, after the <c>bi</c> class. Defaults to <c>bi-signpost-2</c>.
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = "bi-signpost-2";

    /// <summary>
    /// Heading under the icon. Defaults to <c>That page isn't here</c>.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "That page isn't here";

    /// <summary>
    /// Sentence under the heading. Defaults to a note that the link may be out of date or the target removed.
    /// </summary>
    [Parameter]
    public string Message { get; set; } = "The link may be out of date, or the thing it pointed at may have been removed.";

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
    /// Overrides the browser tab title. <see langword="null"/> defaults to "Not found - {Brand}",
    /// or <c>Not found</c> when the shell brand is blank.
    /// </summary>
    [Parameter]
    public string? PageTitleText { get; set; }

    /// <summary>
    /// Fills <see cref="PageTitleText"/> from the shell brand when the caller did not set it.
    /// </summary>
    protected override void OnParametersSet() =>
        PageTitleText ??= string.IsNullOrWhiteSpace(ShellOptions.Value.Brand)
            ? "Not found"
            : $"Not found - {ShellOptions.Value.Brand}";
}