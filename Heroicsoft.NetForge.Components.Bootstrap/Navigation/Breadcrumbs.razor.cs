using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Navigation;

/// <summary>A trail of links. The last item is the current page.</summary>
/// <typeparam name="TItem">The type of one crumb.</typeparam>
public partial class Breadcrumbs<TItem>
{
    /// <summary>
    /// Crumbs in order. The last item is the current page and is never a link.
    /// <see langword="null"/> or an empty list renders nothing.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<TItem>? Items { get; set; }

    /// <summary>Text for one crumb.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, string> Text { get; set; } = _ => string.Empty;

    /// <summary>
    /// Address for a crumb. <see langword="null"/> leaves every crumb as text or a button.
    /// An empty address for one crumb does the same for that crumb. The last item is never a link.
    /// </summary>
    [Parameter]
    public Func<TItem, string?>? Href { get; set; }

    /// <summary>Called instead of following <see cref="Href"/>. The address stays on the link for middle-click and copy.</summary>
    [Parameter]
    public EventCallback<TItem> OnNavigate { get; set; }

    /// <summary>Markup drawn before each crumb's text, such as a home icon.</summary>
    [Parameter]
    public RenderFragment<TItem>? Leading { get; set; }

    /// <summary>
    /// Accessible name of the <c>nav</c>. Defaults to <c>Breadcrumb</c>.
    /// </summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Breadcrumb";

    /// <summary>
    /// Divider between crumbs. A slash uses the Bootstrap default. Any other value sets
    /// <c>--bs-breadcrumb-divider</c>. Defaults to <c>/</c>.
    /// </summary>
    [Parameter]
    public string Separator { get; set; } = "/";

    /// <summary>
    /// Extra CSS classes on the <c>breadcrumb mb-0</c> list. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Extra CSS classes on each earlier crumb's link or button. <see langword="null"/> adds none.
    /// Not applied to the current page.
    /// </summary>
    [Parameter]
    public string? LinkClass { get; set; }

    /// <summary>
    /// Extra CSS classes on the current page item, which already has <c>breadcrumb-item active</c> and
    /// <c>aria-current="page"</c>. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? CurrentClass { get; set; }

    private string NavClass => string.IsNullOrWhiteSpace(Class) ? "breadcrumb mb-0" : $"breadcrumb mb-0 {Class}";

    private string? DividerStyle => Separator == "/" ? null : $"--bs-breadcrumb-divider: '{Separator}'";

    private static string? CurrentValue(bool last) => last ? "page" : null;

    private string ItemClass(bool last)
    {
        string classes = last ? "breadcrumb-item active" : "breadcrumb-item";
        if (last && !string.IsNullOrWhiteSpace(CurrentClass))
        {
            classes = $"{classes} {CurrentClass}";
        }

        return classes;
    }
}