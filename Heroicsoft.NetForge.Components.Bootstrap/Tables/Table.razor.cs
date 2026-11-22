using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Bootstrap.Tables;

/// <summary>Scrolling table. Put the head and body in the child content.</summary>
public partial class Table
{
    /// <summary>
    /// Table head, body, and other rows. <see langword="null"/> renders an empty table.
    /// The table always includes <c>table</c>, <c>align-middle</c>, and <c>mb-0</c>.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra CSS classes after the Bootstrap table classes. <see langword="null"/> adds none.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the default, adds <c>table-hover</c>.
    /// </summary>
    [Parameter]
    public bool Hover { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, adds <c>table-sm</c>. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Dense { get; set; }

    /// <summary>
    /// When <see langword="true"/>, adds <c>table-striped</c>. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Striped { get; set; }

    /// <summary>
    /// When <see langword="true"/>, adds <c>table-bordered</c>. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Bordered { get; set; }

    /// <summary>
    /// When <see langword="true"/>, adds <c>table-borderless</c>. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Borderless { get; set; }

    /// <summary>
    /// When <see langword="true"/>, the default, wraps the table in <c>table-responsive</c> so it scrolls horizontally.
    /// When <see langword="false"/>, the table is rendered directly.
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