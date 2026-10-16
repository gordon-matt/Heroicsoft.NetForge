using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Heroicsoft.NetForge.Components;

/// <summary>
/// Supplies sidebar sections that change after startup. Register it scoped when counts and
/// lists are loaded from the database, and call <see cref="Services.SidebarRefresh.NotifyChanged"/>
/// after a write so <c>ShellLayout</c> re-reads <see cref="Sections"/>.
/// </summary>
public interface IShellNavigation
{
    IReadOnlyList<ForgeNavSection> Sections { get; }

    /// <summary>Same shape as <see cref="ForgeShellOptions.SearchUrlFormat"/>. Null keeps the options value.</summary>
    string? SearchUrlFormat => null;
}