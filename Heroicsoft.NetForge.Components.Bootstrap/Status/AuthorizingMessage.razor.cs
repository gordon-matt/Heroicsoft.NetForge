using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Status;

/// <summary>
/// Represents the authorizing message.
/// </summary>
public partial class AuthorizingMessage
{
    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    [Parameter]
    public string Message { get; set; } = "Checking your access…";
}