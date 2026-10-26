using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Navigation;

/// <summary>A trail of links. The last item is the current page.</summary>
public partial class Breadcrumbs<TItem>
{
    [Parameter, EditorRequired]
    public IReadOnlyList<TItem>? Items { get; set; }

    /// <summary>Text for one crumb.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, string> Text { get; set; } = _ => string.Empty;

    /// <summary>Address for a crumb. The last item is never a link.</summary>
    [Parameter]
    public Func<TItem, string?>? Href { get; set; }

    /// <summary>Called instead of following <see cref="Href"/>. The address stays on the link for middle-click and copy.</summary>
    [Parameter]
    public EventCallback<TItem> OnNavigate { get; set; }

    /// <summary>Markup drawn before each crumb's text, such as a home icon.</summary>
    [Parameter]
    public RenderFragment<TItem>? Leading { get; set; }

    [Parameter]
    public string AriaLabel { get; set; } = "Breadcrumb";

    /// <summary>Divider between crumbs. A slash uses the Bootstrap default.</summary>
    [Parameter]
    public string Separator { get; set; } = "/";

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? LinkClass { get; set; }

    [Parameter]
    public string? CurrentClass { get; set; }

    private string NavClass => string.IsNullOrWhiteSpace(Class) ? "breadcrumb mb-0" : $"breadcrumb mb-0 {Class}";

    private string? DividerStyle => Separator == "/" ? null : $"--bs-breadcrumb-divider: '{Separator}'";

    private static string? CurrentValue(bool last) => last ? "page" : null;

    private string ItemClass(bool last)
    {
        var classes = last ? "breadcrumb-item active" : "breadcrumb-item";
        if (last && !string.IsNullOrWhiteSpace(CurrentClass))
        {
            classes = $"{classes} {CurrentClass}";
        }

        return classes;
    }
}
