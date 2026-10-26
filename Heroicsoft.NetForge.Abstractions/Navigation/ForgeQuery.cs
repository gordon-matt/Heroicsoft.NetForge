namespace Heroicsoft.NetForge.Abstractions.Navigation;

/// <summary>The query string as a case-insensitive map of decoded values.</summary>
public static class ForgeQuery
{
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