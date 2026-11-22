namespace Heroicsoft.NetForge.Components.Charts;

/// <summary>
/// Names for <c>plugins.tooltip.nfFormat</c> on a Chart.js configuration.
/// </summary>
/// <remarks>
/// Tooltip and tick formatters have to be JavaScript functions, which a JSON config cannot carry.
/// The chart script reads this flag and attaches the function. Set <c>nfCurrency</c> (an ISO code
/// such as <c>AUD</c>) alongside <see cref="Currency"/>, and <c>nfPercentage</c> to add each
/// point's share of the dataset.
/// </remarks>
public static class ChartFormats
{
    /// <summary>Grouped number with up to two fraction digits. Axis ticks use a compact form with no fraction digits.</summary>
    public const string Number = "number";

    /// <summary>Whole numbers. Fraction digits are omitted on the tooltip and on the axis.</summary>
    public const string Integer = "integer";

    /// <summary>Currency. Pair with <c>nfCurrency</c>. The script uses USD when that code is omitted.</summary>
    public const string Currency = "currency";

    /// <summary>Percent. The point value is divided by 100 before formatting, so 25 displays as 25%.</summary>
    public const string Percent = "percent";
}