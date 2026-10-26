using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Tables;

/// <summary>Scrolling table. Put the head and body in the child content.</summary>
public partial class Table
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool Hover { get; set; } = true;

    [Parameter]
    public bool Dense { get; set; }

    [Parameter]
    public bool Striped { get; set; }

    [Parameter]
    public bool Bordered { get; set; }

    [Parameter]
    public bool Borderless { get; set; }

    [Parameter]
    public bool Responsive { get; set; } = true;

    private string TableClass
    {
        get
        {
            var classes = new List<string> { "table", "align-middle", "mb-0" };
            if (Hover)
            {
                classes.Add("table-hover");
            }

            if (Dense)
            {
                classes.Add("table-sm");
            }

            if (Striped)
            {
                classes.Add("table-striped");
            }

            if (Bordered)
            {
                classes.Add("table-bordered");
            }

            if (Borderless)
            {
                classes.Add("table-borderless");
            }

            if (!string.IsNullOrWhiteSpace(Class))
            {
                classes.Add(Class);
            }

            return string.Join(' ', classes);
        }
    }
}
