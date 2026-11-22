namespace Heroicsoft.NetForge.Components.Tables;

/// <summary>The column a table is ordered by. <see cref="Field"/> is whatever name the page maps to a query.</summary>
/// <param name="Field">Column name the page maps to a query.</param>
/// <param name="Descending"><see langword="true"/> sorts high to low. The default, <see langword="false"/>, sorts low to high.</param>
public sealed record TableSort(string Field, bool Descending = false)
{
    /// <summary>
    /// The sort after a header click: the same column flips direction, a new column starts ascending.
    /// </summary>
    /// <param name="current">The sort in effect. <see langword="null"/> starts an ascending sort on <paramref name="field"/>.</param>
    /// <param name="field">Column the header represents. Compared ordinally with <see cref="Field"/>.</param>
    /// <returns>The same column with the direction flipped, or a new ascending sort on <paramref name="field"/>.</returns>
    public static TableSort Toggle(TableSort? current, string field) =>
        current is not null && string.Equals(current.Field, field, StringComparison.Ordinal)
            ? current with { Descending = !current.Descending }
            : new TableSort(field);
}
