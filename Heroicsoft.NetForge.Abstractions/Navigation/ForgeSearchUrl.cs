namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>
/// Turns a search format such as <c>/transactions?q={0}</c> into a URL,
/// and reads that same parameter back out of the current address.
/// </summary>
public static class ForgeSearchUrl
{
    /// <summary>
    /// Returns the current value of the query parameter that contains <c>{0}</c> in <paramref name="format"/>.
    /// The value is percent-decoded. Returns an empty string when <paramref name="format"/> does not name
    /// such a parameter, or when <paramref name="uri"/> does not include it.
    /// </summary>
    /// <param name="format">Search format such as <c>/transactions?q={0}</c>.</param>
    /// <param name="uri">Address whose query string is read.</param>
    /// <returns>The decoded term, or an empty string.</returns>
    public static string ReadTerm(string? format, Uri uri) => !TryGetKey(format, out string key)
            ? string.Empty
            : ForgeQuery.Parse(uri.Query).TryGetValue(key, out string? value) ? value : string.Empty;

    /// <summary>
    /// Substitutes the trimmed, percent-encoded term for <c>{0}</c> in <paramref name="format"/>.
    /// Returns <see langword="null"/> when <paramref name="format"/> is <see langword="null"/>, blank,
    /// or has no <c>{0}</c>. A <see langword="null"/> or blank term keeps the text before <c>?</c>
    /// when <paramref name="format"/> has a query string, and otherwise replaces <c>{0}</c> with an empty string.
    /// </summary>
    /// <param name="format">Search format that contains <c>{0}</c>.</param>
    /// <param name="term">Search text. It is trimmed before encoding.</param>
    /// <returns>The formatted URL, or <see langword="null"/> when <paramref name="format"/> cannot be used.</returns>
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