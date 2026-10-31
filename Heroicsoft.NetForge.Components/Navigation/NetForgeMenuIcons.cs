namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>
/// Decides whether an icon string is a font class list. NetForge and Bootstrap Icons names stay
/// with the icon component; <c>fa fa-heart</c> is written out as an <c>i</c> element.
/// </summary>
public static class NetForgeMenuIcons
{
    public static bool IsFontClass(string? icon)
    {
        if (string.IsNullOrWhiteSpace(icon))
        {
            return false;
        }

        string value = icon.Trim();

        if (value.Contains(' '))
        {
            return !IsBootstrapClassList(value);
        }

        return value.StartsWith("fa-", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("fas", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("far", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("fal", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("fab", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The class attribute for a Bootstrap icon, or the original font classes.</summary>
    public static string BootstrapClass(string? icon)
    {
        if (string.IsNullOrWhiteSpace(icon))
        {
            return "bi-circle";
        }

        string value = icon.Trim();

        if (IsFontClass(value))
        {
            return value;
        }

        if (value.StartsWith("bi-", StringComparison.Ordinal) || value.StartsWith("bi ", StringComparison.Ordinal))
        {
            return value;
        }

        return "bi-" + value;
    }

    private static bool IsBootstrapClassList(string value) =>
        value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .All(part => part.Equals("bi", StringComparison.OrdinalIgnoreCase)
                || part.StartsWith("bi-", StringComparison.OrdinalIgnoreCase));
}
