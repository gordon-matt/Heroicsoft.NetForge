using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Status;

/// <summary>
/// Represents the not found content.
/// </summary>
public partial class NotFoundContent
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    /// <summary>
    /// Gets or sets the icon.
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = "signpost";

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "That page isn't here";

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    [Parameter]
    public string Message { get; set; } = "The link may be out of date, or the thing it pointed at may have been removed.";

    /// <summary>
    /// Gets or sets the home href.
    /// </summary>
    [Parameter]
    public string HomeHref { get; set; } = "/";

    /// <summary>
    /// Gets or sets the home label.
    /// </summary>
    [Parameter]
    public string HomeLabel { get; set; } = "Back home";

    /// <summary>
    /// Gets or sets the home icon.
    /// </summary>
    [Parameter]
    public string HomeIcon { get; set; } = "home";

    /// <summary>Overrides the browser tab title. Defaults to "Not found - {Brand}".</summary>
    [Parameter]
    public string? PageTitleText { get; set; }

    /// <inheritdoc/>
    protected override void OnParametersSet() =>
        PageTitleText ??= string.IsNullOrWhiteSpace(ShellOptions.Value.Brand)
            ? "Not found"
            : $"Not found - {ShellOptions.Value.Brand}";
}