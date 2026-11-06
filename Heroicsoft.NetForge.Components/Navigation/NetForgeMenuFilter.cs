using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>Drops links the current user cannot see, without changing the menu stored in DI.</summary>
public static class NetForgeMenuFilter
{
    /// <summary>
    /// Applies the value.
    /// </summary>
    /// <param name="sections">The sections.</param>
    /// <param name="user">The user.</param>
    /// <returns>The forge nav sections.</returns>
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
    /// Determines whether this instance can see.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="user">The user.</param>
    /// <returns>true when the condition is met; otherwise, false.</returns>
    public static bool CanSee(ForgeNavItem item, ClaimsPrincipal user) => CanSeeRoles(user, item.Roles) && (item.IsVisible is null || item.IsVisible(user));

    /// <summary>
    /// Determines whether this instance can see roles.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roles">The roles.</param>
    /// <returns>true when the condition is met; otherwise, false.</returns>
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