using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>A trail of links. The last item is the current page.</summary>
/// <typeparam name="TItem">One crumb.</typeparam>
public partial class Breadcrumbs<TItem>
{
    /// <summary>
    /// The crumbs, in order. The last one is the current page and is not a link. <see langword="null"/> or empty renders nothing. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<TItem>? Items { get; set; }

    /// <summary>Text for one crumb.</summary>
    [Parameter, EditorRequired]
    public Func<TItem, string> Text { get; set; } = _ => string.Empty;

    /// <summary>Address for a crumb. The last item is never a link.</summary>
    [Parameter]
    public Func<TItem, string?>? Href { get; set; }

    /// <summary>
    /// Called instead of following <see cref="Href"/>. The address stays on the link for middle-click and copy.
    /// The argument is the crumb. A crumb with no address is a button. The last crumb never raises this.
    /// </summary>
    [Parameter]
    public EventCallback<TItem> OnNavigate { get; set; }

    /// <summary>Markup drawn before each crumb's text, such as a home icon.</summary>
    [Parameter]
    public RenderFragment<TItem>? Leading { get; set; }

    /// <summary>
    /// Accessible name of the trail. Defaults to <c>Breadcrumb</c>.
    /// </summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Breadcrumb";

    /// <summary>
    /// Text between crumbs. Defaults to <c>/</c>. Hidden from assistive technology.
    /// </summary>
    [Parameter]
    public string Separator { get; set; } = "/";

    /// <summary>
    /// Extra classes after <c>nf-breadcrumbs</c>. <see langword="null"/> or whitespace leaves only that class.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Classes on each link or button. The current page uses <see cref="CurrentClass"/> instead. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? LinkClass { get; set; }

    /// <summary>
    /// Classes on the current page. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? CurrentClass { get; set; }

    /// <summary>
    /// Classes on the separator. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? SeparatorClass { get; set; }

    private string NavClass => string.IsNullOrWhiteSpace(Class) ? "nf-breadcrumbs" : $"nf-breadcrumbs {Class}";
}