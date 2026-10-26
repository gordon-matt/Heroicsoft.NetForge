namespace Heroicsoft.NetForge.Components.Layout;

/// <summary>Maps badge class names onto the <c>nf-badge-*</c> tones.</summary>
public static class ForgeBadge
{
    public const string Default = "nf-badge-muted";

    /// <summary>
    /// <c>ForgeNavItem.BadgeCssClass</c> defaults to a Bootstrap class (<c>text-bg-secondary</c>).
    /// This accepts that form, so one nav definition works in both component sets, and the native
    /// <c>nf-badge-*</c> form, which passes through unchanged.
    /// </summary>
    public static string ToneClass(string? cssClass)
    {
        if (string.IsNullOrWhiteSpace(cssClass))
        {
            return Default;
        }

        string[] parts = cssClass.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string? native = parts.FirstOrDefault(part => part.StartsWith("nf-badge-", StringComparison.Ordinal));

        return native is not null ? native : parts.Select(Tone).FirstOrDefault(tone => tone is not null) ?? Default;

        static string? Tone(string part)
        {
            return part switch
            {
                "text-bg-primary" or "bg-primary" => "nf-badge-accent",
                "text-bg-success" or "bg-success" => "nf-badge-success",
                "text-bg-danger" or "bg-danger" => "nf-badge-danger",
                "text-bg-warning" or "bg-warning" => "nf-badge-warning",
                "text-bg-info" or "bg-info" => "nf-badge-info",
                "text-bg-secondary" or "bg-secondary" or "text-bg-light" or "text-bg-dark" => "nf-badge-muted",
                _ => null,
            };
        }
    }
}