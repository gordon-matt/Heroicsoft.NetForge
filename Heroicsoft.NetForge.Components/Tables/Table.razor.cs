using Microsoft.AspNetCore.Components;

namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>Bordered, scrolling table. Put the head and body in the child content.</summary>
public partial class Table
{
    /// <summary>
    /// The table head and body. <see langword="null"/> renders an empty table.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Extra classes after <c>nf-table</c>. <see langword="null"/> or whitespace leaves only <c>nf-table</c>.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>Draws the bordered frame. Turn this off when the table already sits in a card.</summary>
    [Parameter]
    public bool Framed { get; set; } = true;

    private string TableClass => string.IsNullOrWhiteSpace(Class) ? "nf-table" : $"nf-table {Class}";
}