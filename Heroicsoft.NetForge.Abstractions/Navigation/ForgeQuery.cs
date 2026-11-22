namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>The query string as a case-insensitive map of decoded values.</summary>
public static class ForgeQuery
{
    /// <summary>
    /// Splits <paramref name="query"/> into a case-insensitive map of percent-decoded names and values.
    /// A <see langword="null"/> or empty query returns an empty map. One leading <c>?</c> is ignored.
    /// A part with no <c>=</c> has an empty value. A repeated name keeps the last value.
    /// </summary>
    /// <param name="query">Raw query string, with or without a leading <c>?</c>.</param>
    /// <returns>Decoded names and values. Names are compared without case.</returns>
    /// <exception cref="UriFormatException">A name or value contains an invalid <c>%</c> escape.</exception>
    public static Dictionary<string, string> Parse(string? query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(query))
        {
            return result;
        }

        string trimmed = query.StartsWith('?') ? query[1..] : query;

        foreach (string part in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            int equals = part.IndexOf('=');
            string key = Uri.UnescapeDataString(equals < 0 ? part : part[..equals]);
            string value = equals < 0 ? string.Empty : Uri.UnescapeDataString(part[(equals + 1)..]);
            result[key] = value;
        }

        return result;
    }
}