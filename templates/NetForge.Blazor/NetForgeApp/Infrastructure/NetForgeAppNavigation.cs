using Heroicsoft.NetForge.Abstractions.Navigation;

namespace NetForgeApp.Infrastructure;

/// <summary>
/// Sidebar for the sample notes page, plus the log viewer for administrators.
/// </summary>
public sealed class NetForgeAppNavigation : IShellNavigation
{
    public Task<IReadOnlyList<ForgeNavSection>> GetSectionsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ForgeNavSection> sections =
        [
            new ForgeNavSection
            {
                Items =
                [
                    new ForgeNavItem
                    {
                        Title = "Notes",
                        Href = "/",
                        Icon = "library",
                        MatchAll = true,
                    },
                ],
            },
            new ForgeNavSection
            {
                Heading = "Administration",
                Roles = WellKnownRoles.Administrator,
                Items =
                [
                    new ForgeNavItem
                    {
                        Title = "Logs",
                        Href = "/logvue",
                        Icon = "logs",
                        External = true,
                        Tooltip = "Log viewer",
                    },
                ],
            },
        ];

        return Task.FromResult(sections);
    }
}
