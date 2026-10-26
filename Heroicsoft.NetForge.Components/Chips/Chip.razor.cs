using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Heroicsoft.NetForge.Components.Chips;

/// <summary>One filter chip. Renders a link when <see cref="Href"/> is set, otherwise a button.</summary>
public partial class Chip
{
    [Parameter]
    public string? Href { get; set; }

    [Parameter]
    public bool Selected { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    private string CssClass
    {
        get
        {
            var classes = new List<string> { "nf-chip" };
            if (Selected)
            {
                classes.Add("is-active");
            }

            if (!string.IsNullOrWhiteSpace(Class))
            {
                classes.Add(Class);
            }

            return string.Join(' ', classes);
        }
    }
}
