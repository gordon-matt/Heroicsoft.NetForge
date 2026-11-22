using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Status;

/// <summary>
/// Renders a small spinner and a sentence, centered, while authorization is in progress.
/// </summary>
public partial class AuthorizingMessage
{
    /// <summary>
    /// Sentence after the spinner. Defaults to <c>Checking your access…</c>.
    /// </summary>
    [Parameter]
    public string Message { get; set; } = "Checking your access…";
}