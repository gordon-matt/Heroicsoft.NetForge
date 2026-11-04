namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>The column a table is ordered by. <see cref="Field"/> is whatever name the page maps to a query.</summary>
public sealed record TableSort(string Field, bool Descending = false)
{
    /// <summary>
    /// The sort after a header click: the same column flips direction, a new column starts ascending.
    /// </summary>
    public static TableSort Toggle(TableSort? current, string field) =>
        current is not null && string.Equals(current.Field, field, StringComparison.Ordinal)
            ? current with { Descending = !current.Descending }
            : new TableSort(field);
}
