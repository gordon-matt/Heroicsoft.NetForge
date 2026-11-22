using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>Drops links the current user cannot see, without changing the menu stored in DI.</summary>
public static class NetForgeMenuFilter
{
    /// <summary>
    /// Returns a copy of <paramref name="sections"/> with hidden groups and links removed. A group or link whose roles are empty is kept. A link with no remaining children and no address is dropped.
    /// </summary>
    /// <param name="sections">The menu groups to filter.</param>
    /// <param name="user">The user whose roles and visibility callback are checked.</param>
    /// <returns>The groups that still have something to show.</returns>
    public static IReadOnlyList<ForgeNavSection> Apply(IReadOnlyList<ForgeNavSection> sections, ClaimsPrincipal user)
    {
        var visible = new List<ForgeNavSection>();

        foreach (var section in sections)
        {
            if (!CanSeeRoles(user, section.Roles))
            {
                continue;
            }

            var items = section.Items
                .Select(item => FilterItem(item, user))
                .Where(item => item is not null)
                .Cast<ForgeNavItem>()
                .ToList();

            if (items.Count == 0)
            {
                continue;
            }

            visible.Add(new ForgeNavSection
            {
                Heading = section.Heading,
                Roles = section.Roles,
                InTopbar = section.InTopbar,
                Items = items,
            });
        }

        return visible;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="user"/> passes the item's roles and its visibility callback is <see langword="null"/> or returns <see langword="true"/>.
    /// </summary>
    /// <param name="item">The link to test.</param>
    /// <param name="user">The user who would see it.</param>
    /// <returns><see langword="true"/> when the link should be rendered.</returns>
    public static bool CanSee(ForgeNavItem item, ClaimsPrincipal user) => CanSeeRoles(user, item.Roles) && (item.IsVisible is null || item.IsVisible(user));

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="roles"/> is <see langword="null"/> or whitespace, or <paramref name="user"/> is in any role in the comma-separated list.
    /// </summary>
    /// <param name="user">The user to test.</param>
    /// <param name="roles">Comma-separated roles. Empty shows the item to everyone.</param>
    /// <returns><see langword="true"/> when the roles allow the user through.</returns>
    public static bool CanSeeRoles(ClaimsPrincipal user, string? roles) => string.IsNullOrWhiteSpace(roles) || roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(user.IsInRole);

    private static ForgeNavItem? FilterItem(ForgeNavItem item, ClaimsPrincipal user)
    {
        if (!CanSee(item, user))
        {
            return null;
        }

        var children = (item.Children ?? [])
            .Select(child => FilterItem(child, user))
            .Where(child => child is not null)
            .Cast<ForgeNavItem>()
            .ToList();

        return children.Count == 0 && string.IsNullOrWhiteSpace(item.Href)
            ? null
            : new ForgeNavItem
            {
                Title = item.Title,
                Href = item.Href,
                Icon = item.Icon,
                Tooltip = item.Tooltip,
                TagIcon = item.TagIcon,
                Count = item.Count,
                Badge = item.Badge,
                BadgeCssClass = item.BadgeCssClass,
                BadgeComponent = item.BadgeComponent,
                BadgeParameters = item.BadgeParameters,
                MatchAll = item.MatchAll,
                ActivePaths = item.ActivePaths,
                ActiveQuery = item.ActiveQuery,
                External = item.External,
                Roles = item.Roles,
                Children = children,
                IsVisible = item.IsVisible,
                CssClass = item.CssClass,
            };
    }
}