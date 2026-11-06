using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>A trail of links. The last item is the current page.</summary>
public partial class Breadcrumbs<TItem>
{
    /// <summary>
    /// Gets or sets the items.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the aria label.
    /// </summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Breadcrumb";

    /// <summary>
    /// Gets or sets the separator.
    /// </summary>
    [Parameter]
    public string Separator { get; set; } = "/";

    /// <summary>
    /// Gets or sets the class.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the link class.
    /// </summary>
    [Parameter]
    public string? LinkClass { get; set; }

    /// <summary>
    /// Gets or sets the current class.
    /// </summary>
    [Parameter]
    public string? CurrentClass { get; set; }

    /// <summary>
    /// Gets or sets the separator class.
    /// </summary>
    [Parameter]
    public string? SeparatorClass { get; set; }

    private string NavClass => string.IsNullOrWhiteSpace(Class) ? "nf-breadcrumbs" : $"nf-breadcrumbs {Class}";
}