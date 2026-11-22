using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Status;

/// <summary>
/// Renders a spinner and a status message while authorization is in progress.
/// </summary>
public partial class AuthorizingMessage
{
    /// <summary>
    /// Text beside the spinner. Defaults to <c>Checking your access.</c>
    /// </summary>
    [Parameter]
    public string Message { get; set; } = "Checking your access.";
}