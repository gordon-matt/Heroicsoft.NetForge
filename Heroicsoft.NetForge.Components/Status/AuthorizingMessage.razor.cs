using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Status;

public partial class AuthorizingMessage
{
    [Parameter]
    public string Message { get; set; } = "Checking your access.";
}