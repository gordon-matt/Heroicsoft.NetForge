using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Heroicsoft.NetForge.Components;

/// <summary>
/// The inline SVG icons behind <c>Icon</c>: the union of the Snippetz and ReelSoon sets plus what
/// the shared components need. All icons share a 24x24 viewBox and are drawn with a round 1.8px
/// stroke, so only the inner shapes are stored here.
/// </summary>
public static partial class IconSet
{
    /// <summary>Icon drawn when a name is unknown, so a typo is visible but harmless.</summary>
    public const string Fallback = "circle";

    private static readonly FrozenDictionary<string, string> Markup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["circle"] = """<circle cx="12" cy="12" r="8" />""",
        ["home"] = """<path d="M4 10.5 12 4l8 6.5V20a1 1 0 0 1-1 1h-5v-6H10v6H5a1 1 0 0 1-1-1z" />""",
        ["library"] = """<path d="M5 4h10a2 2 0 0 1 2 2v14H7a2 2 0 0 0-2 2V4z" /><path d="M7 4h10a2 2 0 0 1 2 2v14" />""",
        ["plus"] = """<path d="M12 5v14M5 12h14" />""",
        ["search"] = """<circle cx="11" cy="11" r="6" /><path d="m20 20-3.5-3.5" />""",
        ["pin"] = """<path d="M9 4h6l-1 6 3 3v2H7v-2l3-3z" /><path d="M12 15v5" />""",
        ["pin-off"] = """<path d="M9 4h6l-.4 2.2M8.2 13H7v2h6M12 15v5M4 4l16 16" />""",
        ["sun"] = """<circle cx="12" cy="12" r="3.5" /><path d="M12 3v2M12 19v2M3 12h2M19 12h2M5.6 5.6l1.4 1.4M17 17l1.4 1.4M18.4 5.6 17 7M7 17l-1.4 1.4" />""",
        ["moon"] = """<path d="M20 14.5A7.5 7.5 0 1 1 9.5 4 6 6 0 0 0 20 14.5z" />""",
        ["menu"] = """<path d="M4 7h16M4 12h16M4 17h16" />""",
        ["dots"] = """<circle cx="12" cy="6" r="1.4" /><circle cx="12" cy="12" r="1.4" /><circle cx="12" cy="18" r="1.4" />""",
        ["copy"] = """<rect x="8" y="8" width="12" height="12" rx="2" /><path d="M4 16V6a2 2 0 0 1 2-2h10" />""",
        ["trash"] = """<path d="M5 7h14M9 7V5h6v2M8 7l1 13h6l1-13" />""",
        ["edit"] = """<path d="M4 20h4l11-11-4-4L4 16z" />""",
        ["rectangle"] = """<rect x="5" y="7" width="14" height="10" rx="1.5" />""",
        ["polygon"] = """<path d="M12 3.5 20.5 9.2 17.2 20H6.8L3.5 9.2z" />""",
        ["save"] = """<path d="M5 4h11l3 3v13H5z" /><path d="M8 4v5h7" /><path d="M8 20v-6h8v6" />""",
        ["logout"] = """<path d="M10 7V5a1 1 0 0 1 1-1h8v16h-8a1 1 0 0 1-1-1v-2" /><path d="M4 12h11M12 8l4 4-4 4" />""",
        ["code"] = """<path d="m8 8-4 4 4 4M16 8l4 4-4 4" />""",
        ["alert"] = """<circle cx="12" cy="12" r="8" /><path d="M12 8v5M12 16h.01" />""",
        ["bell"] = """<path d="M6 9a6 6 0 1 1 12 0c0 7 2 7 2 7H4s2 0 2-7" /><path d="M10 19a2 2 0 0 0 4 0" />""",
        ["film"] = """<rect x="3" y="5" width="18" height="14" rx="2" /><path d="M7 5v14M17 5v14M3 9h4M3 15h4M17 9h4M17 15h4" />""",
        ["tv"] = """<rect x="3" y="6" width="18" height="12" rx="2" /><path d="M8 21h8M9 6 12 3l3 3" />""",
        ["game"] = """<rect x="3" y="8" width="18" height="10" rx="4" /><path d="M8 13h4M10 11v4M16.5 12h.01M18 14.5h.01" />""",
        ["x"] = """<path d="M6 6l12 12M18 6 6 18" />""",
        ["jobs"] = """<rect x="4" y="4" width="16" height="16" rx="2" /><path d="M8 9h8M8 12h8M8 15h5" />""",
        ["logs"] = """<path d="M7 3.5h8l4 4V20a1.5 1.5 0 0 1-1.5 1.5h-10.5A1.5 1.5 0 0 1 5.5 20V5A1.5 1.5 0 0 1 7 3.5z" /><path d="M15 3.5V8h4.5M9 12h6M9 16h6" />""",
        ["link"] = """<path d="M10 13a5 5 0 0 0 7.5.5l2-2a5 5 0 0 0-7-7l-1.2 1.2" /><path d="M14 11a5 5 0 0 0-7.5-.5l-2 2a5 5 0 0 0 7 7l1.2-1.2" />""",
        ["chat"] = """<path d="M5 6h14a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1H9l-4 3v-3H5a1 1 0 0 1-1-1V7a1 1 0 0 1 1-1z" />""",
        ["people"] = """<circle cx="9" cy="8" r="2.5" /><path d="M4.5 18a4.5 4.5 0 0 1 9 0" /><circle cx="16" cy="9" r="2" /><path d="M15 18a3.8 3.8 0 0 1 5-3.6" />""",
        ["diagram"] = """<circle cx="6" cy="6" r="2" /><circle cx="18" cy="6" r="2" /><circle cx="12" cy="18" r="2" /><path d="M8 7.2 10.8 16M16 7.2 13.2 16M8 6h8" />""",
        ["archive"] = """<path d="M4 7h16v11a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1z" /><path d="M3 4h18v3H3zM10 11h4" />""",
        ["clock"] = """<circle cx="12" cy="12" r="8" /><path d="M12 8v4.5l3 2" />""",
        ["star"] = """<path d="m12 3.5 2.1 4.4 4.8.7-3.5 3.4.8 4.8L12 14.8 7.8 16.8l.8-4.8L5.1 8.6l4.8-.7z" />""",
        ["heart"] = """<path d="M12 19.2s-6.5-4-6.5-8.2a3.6 3.6 0 0 1 6.5-2 3.6 3.6 0 0 1 6.5 2c0 4.2-6.5 8.2-6.5 8.2z" />""",
        ["flower"] = """<circle cx="12" cy="12" r="2" /><circle cx="12" cy="6.2" r="2.1" /><circle cx="12" cy="17.8" r="2.1" /><circle cx="6.2" cy="12" r="2.1" /><circle cx="17.8" cy="12" r="2.1" />""",
        ["tree"] = """<path d="M12 21v-4M7 17h10M5.5 13h13L12 4z" />""",
        ["fire"] = """<path d="M12 21a4.5 4.5 0 0 0 4.5-4.5c0-2.6-1.8-3.6-1.8-6.2-1.8 1.8-2.7 1.6-3.6-.2 0 2.6-3.6 3.4-3.6 7.1A4.5 4.5 0 0 0 12 21z" />""",
        ["droplet"] = """<path d="M12 3.5S6.5 10 6.5 14a5.5 5.5 0 0 0 11 0c0-4-5.5-10.5-5.5-10.5z" />""",
        ["airplane"] = """<path d="M21 12 3.5 5.5l2.2 6.5-2.2 6.5L21 12zM6 12h9" />""",
        ["flag"] = """<path d="M6 21V3.5M6 4h11l-2.2 3.5L17 11H6" />""",
        ["megaphone"] = """<path d="M4 10v4l11 3.5V6.5zM15 9.2a3.2 3.2 0 0 1 0 5.6M7.5 14.2V17a1.8 1.8 0 0 0 1.8 1.8h.8" />""",
        ["briefcase"] = """<rect x="3" y="8" width="18" height="12" rx="2" /><path d="M9 8V6.2A1.8 1.8 0 0 1 10.8 4.4h2.4A1.8 1.8 0 0 1 15 6.2V8M3 13h18" />""",
        ["mortarboard"] = """<path d="M2 10 12 5l10 5-10 5zM6 12.2V16c2 1.6 10 1.6 12 0v-3.8" />""",
        ["book"] = """<path d="M4 19.2A2.2 2.2 0 0 1 6.2 17H20" /><path d="M6.2 3H20v18H6.2A2.2 2.2 0 0 1 4 18.8V5.2A2.2 2.2 0 0 1 6.2 3z" />""",
        ["calendar"] = """<rect x="4" y="5" width="16" height="15" rx="2" /><path d="M8 3.5v4M16 3.5v4M4 10h16" />""",
        ["x-circle"] = """<circle cx="12" cy="12" r="8" /><path d="m9 9 6 6M15 9l-6 6" />""",

        ["arrow-left"] = """<path d="M19 12H5" /><path d="m12 19-7-7 7-7" />""",
        ["layout"] = """<rect x="3" y="3" width="7" height="9" rx="1" /><rect x="14" y="3" width="7" height="5" rx="1" /><rect x="14" y="12" width="7" height="9" rx="1" /><rect x="3" y="16" width="7" height="5" rx="1" />""",
        ["chevron-left"] = """<path d="m15 6-6 6 6 6" />""",
        ["chevron-right"] = """<path d="m9 6 6 6-6 6" />""",
        ["chevron-down"] = """<path d="m6 9 6 6 6-6" />""",
        ["chevrons-left"] = """<path d="m11 7-5 5 5 5M18 7l-5 5 5 5" />""",
        ["chevrons-right"] = """<path d="m6 7 5 5-5 5M13 7l5 5-5 5" />""",
        ["check"] = """<path d="m5 12.5 4.5 4.5L19 7.5" />""",
        ["check-circle"] = """<circle cx="12" cy="12" r="8" /><path d="m8.5 12.5 2.5 2.5 4.5-5" />""",
        ["info"] = """<circle cx="12" cy="12" r="8" /><path d="M12 11v5M12 8h.01" />""",
        ["warning"] = """<path d="M10.3 4.6 2.8 18a2 2 0 0 0 1.7 3h15a2 2 0 0 0 1.7-3L13.7 4.6a2 2 0 0 0-3.4 0z" /><path d="M12 9.5v4M12 17h.01" />""",
        ["danger"] = """<path d="M8.2 3h7.6L21 8.2v7.6L15.8 21H8.2L3 15.8V8.2z" /><path d="M12 8v5M12 16h.01" />""",
        ["user"] = """<circle cx="12" cy="8" r="3.5" /><path d="M5 20a7 7 0 0 1 14 0" />""",
        ["external"] = """<path d="M14 4h6v6M20 4l-9 9M18 14v4a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4" />""",
        ["shield"] = """<path d="M12 3 5 6v5c0 4.5 3 8 7 10 4-2 7-5.5 7-10V6z" /><rect x="9.5" y="11" width="5" height="4.5" rx="1" /><path d="M10.5 11V9.8a1.5 1.5 0 0 1 3 0V11" />""",
        ["inbox"] = """<path d="M4 13h4l1.5 2.5h5L16 13h4M4 13l2-8h12l2 8v6a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1z" />""",
        ["signpost"] = """<path d="M12 3v18" /><path d="M5 5h11l3 3-3 3H5z" /><path d="M19 13H8l-3 3 3 3h11z" />""",
        ["settings"] = """<path d="M4 7h10M18 7h2M4 17h2M10 17h10" /><circle cx="16" cy="7" r="2" /><circle cx="8" cy="17" r="2" />""",
        ["bookmark"] = """<path d="M7 4h10a1 1 0 0 1 1 1v15l-6-3.5L6 20V5a1 1 0 0 1 1-1z" />""",
        ["bug"] = """<path d="M9 9V7.5a3 3 0 0 1 6 0V9" /><path d="M8 13h8v1a4 4 0 0 1-8 0z" /><path d="M12 20v-6M7 13H4M20 13h-3M6.5 8.5 4.5 6.5M17.5 8.5l2 2M6.5 17.5 4.5 19.5M17.5 17.5l2 2" />""",
        ["database"] = """<ellipse cx="12" cy="6" rx="7" ry="3" /><path d="M5 6v12c0 1.7 3.1 3 7 3s7-1.3 7-3V6" /><path d="M5 12c0 1.7 3.1 3 7 3s7-1.3 7-3" />""",
        ["equal"] = """<path d="M6 9h12M6 15h12" />""",
        ["not-equal"] = """<path d="M6 9h12M6 15h12M16 5 8 19" />""",
        ["globe"] = """<circle cx="12" cy="12" r="8" /><path d="M12 4a12 12 0 0 1 0 16 12 12 0 0 1 0-16M4 12h16" />""",
        ["help"] = """<circle cx="12" cy="12" r="8" /><path d="M9.5 9.5a2.5 2.5 0 1 1 3.2 2.4c-.6.3-1.2.8-1.2 1.6V14M12 17h.01" />""",
        ["refresh"] = """<path d="M20 12a8 8 0 1 1-2.3-5.6M20 4v5h-5" />""",
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenDictionary<string, IReadOnlyList<IconShape>> Shapes =
        Markup.ToFrozenDictionary(pair => pair.Key, pair => Parse(pair.Value), StringComparer.OrdinalIgnoreCase);

    // Names the Bootstrap component set (and ForgeNavItem.Icon, which defaults to "bi-circle") use.
    // Keeping them here means a nav definition written for one component set renders in the other.
    private static readonly FrozenDictionary<string, string> Aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["house"] = "home",
        ["house-door"] = "home",
        ["list"] = "menu",
        ["three-dots-vertical"] = "dots",
        ["three-dots"] = "dots",
        ["person"] = "user",
        ["person-circle"] = "user",
        ["box-arrow-right"] = "logout",
        ["log-out"] = "logout",
        ["users"] = "people",
        ["box-arrow-up-right"] = "external",
        ["pencil"] = "edit",
        ["pencil-square"] = "edit",
        ["x-lg"] = "x",
        ["close"] = "x",
        ["plus-lg"] = "plus",
        ["plus-circle"] = "plus",
        ["exclamation-circle"] = "alert",
        ["exclamation-triangle"] = "warning",
        ["exclamation-octagon"] = "danger",
        ["info-circle"] = "info",
        ["signpost-2"] = "signpost",
        ["shield-lock"] = "shield",
        ["gear"] = "settings",
        ["sliders"] = "settings",
        ["files"] = "copy",
        ["clipboard"] = "copy",
        ["journal-text"] = "logs",
        ["diagram-3"] = "diagram",
        ["geo-alt"] = "pin",
        ["chat"] = "chat",
        ["chat-dots"] = "chat",
        ["people"] = "people",
        ["archive"] = "archive",
        ["clock-history"] = "clock",
        ["star-fill"] = "star",
        ["heart-fill"] = "heart",
        ["heart-half"] = "heart",
        ["flower1"] = "flower",
        ["tree-fill"] = "tree",
        ["droplet-half"] = "droplet",
        ["airplane-fill"] = "airplane",
        ["house-heart"] = "home",
        ["shield-check"] = "shield",
        ["file-earmark-text"] = "logs",
        ["calendar-event"] = "calendar",
        ["link-45deg"] = "link",
        ["controller"] = "game",
        ["collection"] = "library",
        ["pin-angle"] = "pin",
        ["chevron-double-left"] = "chevrons-left",
        ["chevron-double-right"] = "chevrons-right",
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>Every icon name that has its own drawing, sorted.</summary>
    public static IReadOnlyList<string> Names { get; } = [.. Markup.Keys.Order(StringComparer.Ordinal)];

    /// <summary>
    /// Resolves what callers write (<c>home</c>, <c>bi-house-door</c>, <c>bi bi-trash</c>) to a
    /// canonical icon name. Unknown names resolve to <see cref="Fallback"/>.
    /// </summary>
    public static string Normalize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Fallback;
        }

        string value = name.Trim();

        // "bi bi-trash" and "bi-trash" both mean the Bootstrap Icons name "trash".
        foreach (string part in value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part.Equals("bi", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            value = part.StartsWith("bi-", StringComparison.OrdinalIgnoreCase) ? part[3..] : part;
            break;
        }

        if (Aliases.TryGetValue(value, out string? aliased))
        {
            value = aliased;
        }

        return Markup.ContainsKey(value) ? value.ToLowerInvariant() : Fallback;
    }

    /// <summary>The SVG elements that draw a name, resolved through <see cref="Normalize"/>.</summary>
    public static IReadOnlyList<IconShape> GetShapes(string? name) => Shapes[Normalize(name)];

    // Shapes are authored as markup because that is easy to read and copy from an icon editor, but
    // they are rendered as real elements: Blazor builds SVG children reliably only that way.
    private static IReadOnlyList<IconShape> Parse(string markup) =>
        [.. ElementPattern().Matches(markup).Select(element => new IconShape(
            element.Groups["tag"].Value,
            [.. AttributePattern().Matches(element.Groups["attributes"].Value).Select(attribute =>
                new KeyValuePair<string, object>(attribute.Groups["name"].Value, attribute.Groups["value"].Value))]))];

    [GeneratedRegex(@"<(?<tag>[a-z]+)(?<attributes>[^>]*?)\s*/>")]
    private static partial Regex ElementPattern();

    [GeneratedRegex("(?<name>[a-z-]+)=\"(?<value>[^\"]*)\"")]
    private static partial Regex AttributePattern();
}

/// <summary>One drawn element of an icon, for example a <c>path</c> and its attributes.</summary>
public sealed record IconShape(string Tag, IReadOnlyList<KeyValuePair<string, object>> Attributes);
