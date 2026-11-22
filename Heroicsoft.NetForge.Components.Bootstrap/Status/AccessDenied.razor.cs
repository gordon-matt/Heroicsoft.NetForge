using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Bootstrap.Status;

/// <summary>
/// Renders an <see cref="EmptyState"/> for a signed-in user who lacks a role, plus a home link unless
/// <see cref="ChildContent"/> replaces it. Sets the browser tab title.
/// </summary>
public partial class AccessDenied
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    /// <summary>
    /// Bootstrap Icons class passed to <see cref="EmptyState"/>. Defaults to <c>bi-shield-lock</c>.
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = "bi-shield-lock";

    /// <summary>
    /// Heading in the empty state. Defaults to <c>You don't have access to this</c>.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "You don't have access to this";

    /// <summary>
    /// Sentence under the heading. Defaults to <c>Ask an administrator if you think you should.</c>
    /// </summary>
    [Parameter]
    public string Description { get; set; } = "Ask an administrator if you think you should.";

    /// <summary>
    /// Address of the home link shown when <see cref="ChildContent"/> is not set. Defaults to <c>/</c>.
    /// </summary>
    [Parameter]
    public string HomeHref { get; set; } = "/";

    /// <summary>
    /// Text of that home link. Defaults to <c>Back home</c>. The link uses <c>btn btn-sm btn-outline-secondary</c>.
    /// </summary>
    [Parameter]
    public string HomeLabel { get; set; } = "Back home";

    /// <summary>
    /// Overrides the browser tab title. <see langword="null"/> defaults to "Access denied - {Brand}",
    /// or <c>Access denied</c> when the shell brand is blank.
    /// </summary>
    [Parameter]
    public string? PageTitleText { get; set; }

    /// <summary>
    /// Replaces the home link. <see langword="null"/> shows the link to <see cref="HomeHref"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Fills <see cref="PageTitleText"/> from the shell brand when the caller did not set it.
    /// </summary>
    protected override void OnParametersSet() =>
        PageTitleText ??= string.IsNullOrWhiteSpace(ShellOptions.Value.Brand)
            ? "Access denied"
            : $"Access denied - {ShellOptions.Value.Brand}";
}