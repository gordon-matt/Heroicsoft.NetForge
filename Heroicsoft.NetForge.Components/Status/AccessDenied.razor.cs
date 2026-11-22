using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Status;

/// <summary>
/// Renders an empty state for a signed-in user who lacks the role, with a link home unless <see cref="ChildContent"/> replaces it, and sets the browser tab title.
/// </summary>
public partial class AccessDenied
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    /// <summary>
    /// Icon above the heading. Defaults to <c>shield</c>.
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = "shield";

    /// <summary>
    /// Heading. Defaults to <c>You don't have access to this</c>.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "You don't have access to this";

    /// <summary>
    /// Paragraph under the heading. Defaults to a note to ask an administrator.
    /// </summary>
    [Parameter]
    public string Description { get; set; } = "Ask an administrator if you think you should.";

    /// <summary>
    /// Address of the home link shown when <see cref="ChildContent"/> is <see langword="null"/>. Defaults to <c>/</c>.
    /// </summary>
    [Parameter]
    public string HomeHref { get; set; } = "/";

    /// <summary>
    /// Text of that home link. Defaults to <c>Back home</c>.
    /// </summary>
    [Parameter]
    public string HomeLabel { get; set; } = "Back home";

    /// <summary>Overrides the browser tab title. Defaults to "Access denied - {Brand}".</summary>
    [Parameter]
    public string? PageTitleText { get; set; }

    /// <summary>
    /// Replaces the home link. <see langword="null"/> shows the link from <see cref="HomeHref"/> and <see cref="HomeLabel"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Fills <see cref="PageTitleText"/> when it is <see langword="null"/>: <c>Access denied - {brand}</c> when the shell brand is set, otherwise <c>Access denied</c>.
    /// </summary>
    protected override void OnParametersSet() =>
        PageTitleText ??= string.IsNullOrWhiteSpace(ShellOptions.Value.Brand)
            ? "Access denied"
            : $"Access denied - {ShellOptions.Value.Brand}";
}