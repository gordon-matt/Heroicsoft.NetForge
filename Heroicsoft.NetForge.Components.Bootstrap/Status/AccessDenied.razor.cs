using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Bootstrap.Status;

/// <summary>
/// Represents the access denied.
/// </summary>
public partial class AccessDenied
{
    [Inject]
    private IOptions<ForgeShellOptions> ShellOptions { get; set; } = default!;

    /// <summary>
    /// Gets or sets the icon.
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = "bi-shield-lock";

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = "You don't have access to this";

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [Parameter]
    public string Description { get; set; } = "Ask an administrator if you think you should.";

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

    /// <summary>Overrides the browser tab title. Defaults to "Access denied - {Brand}".</summary>
    [Parameter]
    public string? PageTitleText { get; set; }

    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <inheritdoc/>
    protected override void OnParametersSet() =>
        PageTitleText ??= string.IsNullOrWhiteSpace(ShellOptions.Value.Brand)
            ? "Access denied"
            : $"Access denied - {ShellOptions.Value.Brand}";
}