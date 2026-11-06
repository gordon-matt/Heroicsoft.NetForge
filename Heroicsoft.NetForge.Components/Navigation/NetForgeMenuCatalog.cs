using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>Builds every <see cref="NetForgeMenuModel"/> the first time something asks for one.</summary>
public sealed class NetForgeMenuCatalog : INetForgeMenuCatalog
{
    private readonly Dictionary<string, NetForgeMenuModel> menus;

    private NetForgeMenuCatalog(Dictionary<string, NetForgeMenuModel> menus)
    {
        this.menus = menus;
        Names = menus.Keys.ToList();
    }

    /// <summary>
    /// Gets the names.
    /// </summary>
    public IReadOnlyCollection<string> Names { get; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The net forge menu.</returns>
    public NetForgeMenuModel Get(string? name)
    {
        string key = string.IsNullOrWhiteSpace(name) ? NetForgeMenus.DefaultName : name.Trim();

        return menus.TryGetValue(key, out var menu) ? menu : NetForgeMenuModel.Empty(key);
    }

    internal static NetForgeMenuCatalog Create(
        IServiceProvider services,
        IEnumerable<NetForgeMenuRegistration> registrations,
        IEnumerable<INetForgeMenuContributor> contributors)
    {
        var state = new MenuBuildState(services, contributors.ToList());

        foreach (var registration in registrations)
        {
            var builder = new NetForgeMenuBuilder(state, registration.Name);
            registration.Configure(builder);
        }

        return new NetForgeMenuCatalog(state.ToMenus());
    }
}

internal sealed class NetForgeMenuRegistration
{
    public NetForgeMenuRegistration(string name, Action<INetForgeMenuBuilder> configure)
    {
        Name = name;
        Configure = configure;
    }

    public string Name { get; }

    public Action<INetForgeMenuBuilder> Configure { get; }
}

internal sealed class MenuBuildState
{
    private readonly Dictionary<string, INetForgeMenuContributor> byName;

    public MenuBuildState(IServiceProvider services, IReadOnlyList<INetForgeMenuContributor> contributors)
    {
        Services = services;
        Contributors = contributors;
        byName = new Dictionary<string, INetForgeMenuContributor>(StringComparer.OrdinalIgnoreCase);

        foreach (var contributor in contributors)
        {
            if (string.IsNullOrWhiteSpace(contributor.Name))
            {
                throw new InvalidOperationException($"{contributor.GetType().Name} does not set a menu contributor name.");
            }

            if (!byName.TryAdd(contributor.Name, contributor))
            {
                throw new InvalidOperationException($"Menu contributor '{contributor.Name}' is registered more than once.");
            }
        }
    }

    public IServiceProvider Services { get; }

    public IReadOnlyList<INetForgeMenuContributor> Contributors { get; }

    public HashSet<string> Contributed { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<PendingSection> Sections { get; } = [];

    public Dictionary<string, NetForgeMenuPlacement> Placements { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetContributor(string name, out INetForgeMenuContributor? contributor) =>
        byName.TryGetValue(name, out contributor);

    public Dictionary<string, NetForgeMenuModel> ToMenus()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var section in Sections)
        {
            names.Add(section.Name);
        }

        foreach (string name in Placements.Keys)
        {
            names.Add(name);
        }

        var menus = new Dictionary<string, NetForgeMenuModel>(StringComparer.OrdinalIgnoreCase);

        foreach (string name in names)
        {
            var sections = Sections
                .Select((section, index) => (section, index))
                .Where(pair => string.Equals(pair.section.Name, name, StringComparison.OrdinalIgnoreCase))
                .OrderBy(pair => pair.section.Options.Order)
                .ThenBy(pair => pair.index)
                .Select(pair => new ForgeNavSection
                {
                    Heading = pair.section.Title,
                    Roles = pair.section.Options.Roles,
                    InTopbar = pair.section.Options.Placement switch
                    {
                        NetForgeMenuPlacement.Topbar => true,
                        NetForgeMenuPlacement.Sidebar => false,
                        _ => null,
                    },
                    Items = Freeze(pair.section.Items, pair.section.Title),
                })
                .ToList();

            var placement = Placements.TryGetValue(name, out var set)
                ? set
                : NetForgeMenuPlacement.Sidebar;

            menus.Add(name, new NetForgeMenuModel(name, true, placement, sections));
        }

        return menus;
    }

    private static IReadOnlyList<ForgeNavItem> Freeze(List<NetForgeMenuItemOptions> items, string? section) => items
            .OrderBy(item => item.Order)
            .Select(item => FreezeItem(item, section))
            .ToList();

    private static ForgeNavItem FreezeItem(NetForgeMenuItemOptions item, string? section)
    {
        string title = item.Title.Trim();

        if (title.Length == 0)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(section)
                    ? "A menu item needs a title."
                    : $"A menu item in '{section}' needs a title.");
        }

        string url = item.Url.Trim();

        if (url == "#")
        {
            url = string.Empty;
        }

        return url.Length == 0 && item.ChildItems.Count == 0
            ? throw new InvalidOperationException($"Menu item '{title}' needs a url or child items.")
            : new ForgeNavItem
            {
                Title = title,
                Href = url,
                Icon = string.IsNullOrWhiteSpace(item.Icon) ? "circle" : item.Icon.Trim(),
                Tooltip = item.Tooltip,
                TagIcon = item.TagIcon,
                Count = item.Count,
                Badge = item.Badge,
                BadgeCssClass = string.IsNullOrWhiteSpace(item.BadgeCssClass) ? "text-bg-secondary" : item.BadgeCssClass,
                MatchAll = item.MatchAll,
                ActivePaths = item.ActivePaths,
                ActiveQuery = item.ActiveQuery,
                External = item.External,
                Roles = item.Roles,
                Children = Freeze(item.ChildItems, title),
                IsVisible = item.IsVisible,
                CssClass = item.CssClass,
            };
    }
}

internal sealed class PendingSection
{
    public required string Name { get; init; }

    public string? Title { get; init; }

    public NetForgeMenuGroupOptions Options { get; } = new();

    public List<NetForgeMenuItemOptions> Items { get; } = [];
}

internal sealed class NetForgeMenuBuilder : INetForgeMenuBuilder
{
    private readonly MenuBuildState state;
    private readonly string name;

    public NetForgeMenuBuilder(MenuBuildState state, string name)
    {
        this.state = state;
        this.name = name;
    }

    public IServiceProvider Services => state.Services;

    public INetForgeMenuBuilder AsSidebar(string? name = null)
    {
        state.Placements[ResolveName(name)] = NetForgeMenuPlacement.Sidebar;
        return this;
    }

    public INetForgeMenuBuilder AsTopbar(string? name = null)
    {
        state.Placements[ResolveName(name)] = NetForgeMenuPlacement.Topbar;
        return this;
    }

    public INetForgeMenuGroupBuilder AddMenu(string title, Action<NetForgeMenuGroupOptions>? configure = null) =>
        AddMenu(title, name, configure);

    public INetForgeMenuGroupBuilder AddMenu(string title, string name, Action<NetForgeMenuGroupOptions>? configure = null) => string.IsNullOrWhiteSpace(title)
            ? throw new ArgumentException("A menu needs a title.", nameof(title))
            : AddSection(title.Trim(), ResolveName(name), configure);

    public INetForgeMenuGroupBuilder AddItems(Action<NetForgeMenuGroupOptions>? configure = null) =>
        AddSection(null, name, configure);

    public INetForgeMenuGroupBuilder AddItems(string name, Action<NetForgeMenuGroupOptions>? configure = null) =>
        AddSection(null, ResolveName(name), configure);

    public INetForgeMenuBuilder IncludeRegistered()
    {
        foreach (var contributor in state.Contributors)
        {
            if (!state.Contributed.Add(contributor.Name))
            {
                continue;
            }

            string targetName = string.IsNullOrWhiteSpace(contributor.DefaultName)
                ? name
                : contributor.DefaultName.Trim();
            string title = contributor.DefaultMenu?.Trim() ?? string.Empty;
            var group = title.Length == 0
                ? AddSection(null, targetName, null)
                : AddSection(title, targetName, null);
            contributor.Contribute(group, null);
        }

        return this;
    }

    private INetForgeMenuGroupBuilder AddSection(string? title, string name, Action<NetForgeMenuGroupOptions>? configure)
    {
        var existing = state.Sections.FirstOrDefault(section =>
            string.Equals(section.Name, name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(section.Title, title, StringComparison.Ordinal));

        if (existing is null)
        {
            existing = new PendingSection
            {
                Name = name,
                Title = title,
            };
            state.Sections.Add(existing);
        }

        configure?.Invoke(existing.Options);
        return new NetForgeMenuGroupBuilder(state, existing.Items, existing);
    }

    private string ResolveName(string? name) =>
        string.IsNullOrWhiteSpace(name) ? this.name : name.Trim();
}

internal sealed class NetForgeMenuGroupBuilder : INetForgeMenuGroupBuilder
{
    private readonly MenuBuildState state;
    private readonly List<NetForgeMenuItemOptions> items;
    private readonly PendingSection? section;

    public NetForgeMenuGroupBuilder(MenuBuildState state, List<NetForgeMenuItemOptions> items, PendingSection? section = null)
    {
        this.state = state;
        this.items = items;
        this.section = section;
    }

    public IServiceProvider Services => state.Services;

    public INetForgeMenuGroupBuilder AsSidebar() => Place(NetForgeMenuPlacement.Sidebar);

    public INetForgeMenuGroupBuilder AsTopbar() => Place(NetForgeMenuPlacement.Topbar);

    private INetForgeMenuGroupBuilder Place(NetForgeMenuPlacement placement)
    {
        if (section is null)
        {
            throw new InvalidOperationException("AsTopbar and AsSidebar apply to a menu, not to a nested submenu.");
        }

        section.Options.Placement = placement;
        return this;
    }

    public INetForgeMenuGroupBuilder AddCustom(Action<NetForgeMenuItemOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var item = new NetForgeMenuItemOptions();
        configure(item);
        items.Add(item);
        return this;
    }

    public INetForgeMenuGroupBuilder AddSubmenu(
        string title,
        Action<INetForgeMenuGroupBuilder> children,
        Action<NetForgeMenuItemOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(children);

        return AddCustom(item =>
        {
            item.Title = title;
            var childGroup = new NetForgeMenuGroupBuilder(state, item.ChildItems);
            children(childGroup);
            configure?.Invoke(item);
        });
    }

    public INetForgeMenuGroupBuilder AddContributor(string name, Action<NetForgeMenuItemOptions>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!state.TryGetContributor(name, out var contributor) || contributor is null)
        {
            throw new InvalidOperationException(
                $"No menu contributor named '{name}' is registered. The library that owns it should call AddNetForgeMenuContributor from its own service registration.");
        }

        state.Contributed.Add(contributor.Name);
        contributor.Contribute(this, configure);
        return this;
    }

    public INetForgeMenuGroupBuilder AddContributors()
    {
        foreach (var contributor in state.Contributors)
        {
            if (!state.Contributed.Add(contributor.Name))
            {
                continue;
            }

            contributor.Contribute(this, null);
        }

        return this;
    }
}