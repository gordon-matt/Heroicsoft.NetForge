namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>
/// Decides whether an icon string is a font class list. NetForge and Bootstrap Icons names stay
/// with the icon component; <c>fa fa-heart</c> is written out as an <c>i</c> element.
/// </summary>
public static class NetForgeMenuIcons
{
    /// <summary>
    /// Determines whether this instance is font class.
    /// </summary>
    /// <param name="icon">The icon.</param>
    /// <returns>true when the condition is met; otherwise, false.</returns>
    public static bool IsFontClass(string? icon)
    {
        if (string.IsNullOrWhiteSpace(icon))
        {
            return false;
        }

        string value = icon.Trim();

        return value.Contains(' ')
            ? !IsBootstrapClassList(value)
            : value.StartsWith("fa-", StringComparison.OrdinalIgnoreCase)
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

        return IsFontClass(value)
            ? value
            : value.StartsWith("bi-", StringComparison.Ordinal) || value.StartsWith("bi ", StringComparison.Ordinal) ? value : "bi-" + value;
    }

    private static bool IsBootstrapClassList(string value) =>
        value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .All(part => part.Equals("bi", StringComparison.OrdinalIgnoreCase)
                || part.StartsWith("bi-", StringComparison.OrdinalIgnoreCase));
}