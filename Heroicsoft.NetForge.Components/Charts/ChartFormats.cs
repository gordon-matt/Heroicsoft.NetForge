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
    public const string Number = "number";

    public const string Integer = "integer";

    public const string Currency = "currency";

    public const string Percent = "percent";
}
