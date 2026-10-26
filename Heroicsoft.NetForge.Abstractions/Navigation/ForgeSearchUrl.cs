namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>
/// Turns a search format such as <c>/transactions?q={0}</c> into a URL,
/// and reads that same parameter back out of the current address.
/// </summary>
public static class ForgeSearchUrl
{
    public static string ReadTerm(string? format, Uri uri) => !TryGetKey(format, out string key)
            ? string.Empty
            : ForgeQuery.Parse(uri.Query).TryGetValue(key, out string? value) ? value : string.Empty;

    /// <summary>A null or blank <paramref name="term"/> drops the search parameter and keeps the path.</summary>
    public static string? Build(string? format, string? term)
    {
        if (string.IsNullOrWhiteSpace(format) || !format.Contains("{0}", StringComparison.Ordinal))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(term))
        {
            int query = format.IndexOf('?');
            return query < 0 ? format.Replace("{0}", string.Empty, StringComparison.Ordinal) : format[..query];
        }

        return string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            format,
            Uri.EscapeDataString(term.Trim()));
    }

    private static bool TryGetKey(string? format, out string key)
    {
        key = string.Empty;

        if (string.IsNullOrWhiteSpace(format))
        {
            return false;
        }

        int placeholder = format.IndexOf("{0}", StringComparison.Ordinal);
        int query = format.IndexOf('?');

        if (placeholder < 0 || query < 0 || query > placeholder)
        {
            return false;
        }

        int amp = format.LastIndexOf('&', placeholder);
        int start = Math.Max(query, amp) + 1;
        int equals = format.IndexOf('=', start);

        if (equals < 0 || equals > placeholder)
        {
            return false;
        }

        key = format[start..equals];
        return key.Length > 0;
    }
}