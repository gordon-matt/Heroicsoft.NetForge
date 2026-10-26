using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Theming;

public partial class ThemeToggle
{
    [Inject]
    private IOptions<ForgeUiOptions> UiOptions { get; set; } = default!;

    /// <summary>Icon size in CSS pixels.</summary>
    [Parameter]
    public int Size { get; set; } = 18;
}