using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Components.Theming;

/// <summary>
/// Renders a button that switches the document between light and dark. The page script handles the click, so the button works without a circuit.
/// </summary>
public partial class ThemeToggle
{
    [Inject]
    private IOptions<ForgeUiOptions> UiOptions { get; set; } = default!;

    /// <summary>Icon size in CSS pixels. Defaults to 18.</summary>
    [Parameter]
    public int Size { get; set; } = 18;
}