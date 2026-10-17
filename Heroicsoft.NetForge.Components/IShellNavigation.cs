using Heroicsoft.NetForge.Abstractions.Navigation;

namespace Heroicsoft.NetForge.Components.Shell;

/// <summary>
/// Supplies sidebar sections that change after startup. Register it scoped when counts and
/// lists are loaded from the database, and call <see cref="Services.SidebarRefresh.NotifyChanged"/>
/// after a write so the shell layout loads <see cref="GetSectionsAsync"/> again.
/// </summary>
public interface IShellNavigation
{
    /// <summary>The shell awaits this on startup and after every sidebar refresh.</summary>
    Task<IReadOnlyList<ForgeNavSection>> GetSectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Same shape as <see cref="ForgeShellOptions.SearchUrlFormat"/>. Null keeps the options value.</summary>
    string? SearchUrlFormat => null;

    /// <summary>
    /// Where a search goes. A null <paramref name="term"/> means the box was cleared.
    /// Return null to use <see cref="SearchUrlFormat"/> instead.
    /// </summary>
    string? FormatSearchUrl(string? term, Uri currentUri) => null;
}
