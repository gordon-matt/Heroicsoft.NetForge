using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Tables;

/// <summary>Scrolling table. Put the head and body in the child content.</summary>
public partial class Table
{
    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the class.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is hover.
    /// </summary>
    [Parameter]
    public bool Hover { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this instance is dense.
    /// </summary>
    [Parameter]
    public bool Dense { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is striped.
    /// </summary>
    [Parameter]
    public bool Striped { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is bordered.
    /// </summary>
    [Parameter]
    public bool Bordered { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is borderless.
    /// </summary>
    [Parameter]
    public bool Borderless { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is responsive.
    /// </summary>
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