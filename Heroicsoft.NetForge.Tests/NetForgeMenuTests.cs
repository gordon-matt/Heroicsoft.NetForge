using System.Security.Claims;
using System.Text.Encodings.Web;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components;
using Heroicsoft.NetForge.Components.Bootstrap.Navigation;
using Heroicsoft.NetForge.Components.Icons;
using Heroicsoft.NetForge.Components.Layout;
using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Heroicsoft.NetForge.Tests;

public class NetForgeMenuTests
{
    [Fact]
    public void Build_PlacesLibrariesAndKeepsHostOverrides()
    {
        NetForgeMenuModel menu = Catalog(services =>
        {
            services.AddNetForgeMenuContributor(new LogsContributor());
            services.BuildMenu("Admin", menu =>
            {
                menu.AsTopbar();
                menu.AddMenu("Maintenance")
                    .AddContributor("LogVue", item =>
                    {
                        item.Title = "Viewer";
                        item.Icon = "fa fa-heart";
                    })
                    .AddCustom(item =>
                    {
                        item.Title = "Notes";
                        item.Icon = "library";
                        item.Url = "/";
                        item.MatchAll = true;
                        item.Order = -1;
                    });
            });
        }).Get("admin");

        Assert.True(menu.IsDefined);
        Assert.Equal(NetForgeMenuPlacement.Topbar, menu.Placement);
        Assert.Equal("Maintenance", menu.Sections[0].Heading);
        Assert.Equal("Notes", menu.Sections[0].Items[0].Title);
        Assert.Equal("Viewer", menu.Sections[0].Items[1].Title);
        Assert.Equal("/logvue", menu.Sections[0].Items[1].Href);
        Assert.Equal("fa fa-heart", menu.Sections[0].Items[1].Icon);
    }

    [Fact]
    public void Build_AppendsASecondRegistrationAndMergesTheSameSection()
    {
        NetForgeMenuModel menu = Catalog(services =>
        {
            services.BuildMenu(menu => menu.AddMenu("Site").AddCustom(Link("Home", "/home")));
            services.BuildMenu(menu => menu.AddMenu("Site").AddCustom(Link("About", "/about")));
        }).Get(null);

        Assert.Equal(["Home", "About"], menu.Sections.Single().Items.Select(item => item.Title));
    }

    [Fact]
    public void Build_PutsOneGroupInTheTopBarOfTheSameArea()
    {
        var services = new ServiceCollection();
        services.AddNetForgeComponents();
        services.BuildMenu("Admin", menu =>
        {
            menu.AddMenu("Reports").AddCustom(Link("Reports", "/"));
            menu.AddMenu("Maintenance").AsTopbar().AddCustom(Link("Logs", "/logvue"));
        });
        services.Configure<ForgeShellOptions>(shell =>
        {
            shell.UseSharedMenu = true;
            shell.SharedMenuArea = "Admin";
        });

        using ServiceProvider provider = services.BuildServiceProvider();
        ForgeShellOptions options = provider.GetRequiredService<IOptions<ForgeShellOptions>>().Value;
        NetForgeMenuModel? menu = NetForgeMenuShell.Find(provider, options);

        Assert.NotNull(menu);
        Assert.Equal("Admin", menu.Area);
        Assert.Equal(NetForgeMenuPlacement.Sidebar, menu.Placement);
        Assert.Equal("Reports", menu.Sections[0].Heading);
        Assert.Equal("Maintenance", menu.Sections[1].Heading);
        Assert.NotEqual(true, menu.Sections[0].InTopbar);
        Assert.True(menu.Sections[1].InTopbar);
        Assert.True(NetForgeMenuShell.Splits(menu));
        Assert.Equal(
            ForgeNavigationPlacement.Sidebar,
            NetForgeMenuShell.Placement(menu, options.NavigationPlacement, NetForgeMenuPlacement.Sidebar));
        Assert.Equal(ForgeNavigationPlacement.Topbar, NetForgeMenuShell.Placement(menu, options.NavigationPlacement));

        ClaimsPrincipal user = new(new ClaimsIdentity());
        string side = Text(NetForgeMenuHtml.Render(menu, user, "/"));
        string top = Text(NetForgeMenuHtml.Render(menu, user, "/", NetForgeMenuPlacement.Topbar));
        Assert.Contains("Reports", side, StringComparison.Ordinal);
        Assert.DoesNotContain("Logs", side, StringComparison.Ordinal);
        Assert.Contains("Logs", top, StringComparison.Ordinal);
        Assert.DoesNotContain("Reports", top, StringComparison.Ordinal);
    }

    [Fact]
    public void ForSlot_KeepsTheCurrentAppInTheSidebarAndTheOthersOnTop()
    {
        NetForgeMenuModel menu = Catalog(services =>
        {
            services.BuildMenu("Admin", menu =>
            {
                menu.AddMenu("Reports").AddCustom(Link("Reports", "/reports"));
                menu.AddMenu("Maintenance").AddCustom(Link("Logs", "/logvue"));
            });
        }).Get("Admin");

        IReadOnlyList<ForgeNavSection> side = menu.ForSlot(
            NetForgeMenuPlacement.Sidebar, NetForgeMenuPlacement.Sidebar, "/reports", menu.Sections);
        IReadOnlyList<ForgeNavSection> top = menu.ForSlot(
            NetForgeMenuPlacement.Topbar, NetForgeMenuPlacement.Sidebar, "/reports", menu.Sections);
        IReadOnlyList<ForgeNavSection> away = menu.ForSlot(
            NetForgeMenuPlacement.Topbar, NetForgeMenuPlacement.Sidebar, "/logvue", menu.Sections);
        IReadOnlyList<ForgeNavSection> allTop = menu.ForSlot(
            NetForgeMenuPlacement.Topbar, NetForgeMenuPlacement.Topbar, "/logvue", menu.Sections);

        Assert.Equal("Reports", side.Single().Heading);
        Assert.Equal("Maintenance", top.Single().Heading);
        Assert.Equal("Reports", away.Single().Heading);
        Assert.Equal(["Reports", "Maintenance"], allTop.Select(section => section.Heading));
    }

    [Fact]
    public void ForSlot_CollapsesAnotherAppToASingleLink()
    {
        NetForgeMenuModel menu = Catalog(services =>
        {
            services.BuildMenu("Admin", menu =>
            {
                menu.AddMenu("Reports").AddSubmenu("Queryz", children =>
                {
                    children.AddCustom(item =>
                    {
                        item.Title = "Reports";
                        item.Url = "/";
                        item.MatchAll = true;
                    });
                    children.AddCustom(Link("Groups", "/groups"));
                });
                menu.AddMenu("Maintenance").AddCustom(Link("Logs", "/logvue"));
            });
        }).Get("Admin");

        ForgeNavItem outside = menu.ForSlot(
            NetForgeMenuPlacement.Topbar, NetForgeMenuPlacement.Sidebar, "/logvue", menu.Sections)
            .Single(section => section.Heading == "Reports").Items.Single();
        ForgeNavItem inside = menu.ForSlot(
            NetForgeMenuPlacement.Sidebar, NetForgeMenuPlacement.Sidebar, "/groups", menu.Sections)
            .Single().Items.Single();

        Assert.Equal("Queryz", outside.Title);
        Assert.Equal("/", outside.Href);
        Assert.Empty(outside.Children);
        Assert.Equal("Queryz", inside.Title);
        Assert.Equal(["Reports", "Groups"], inside.Children.Select(child => child.Title));
    }

    [Fact]
    public void Build_FilesASectionIntoAnotherArea()
    {
        INetForgeMenuCatalog catalog = Catalog(services =>
        {
            services.BuildMenu(menu =>
            {
                menu.AddMenu("Maintenance", "Admin").AddCustom(Link("Logs", "/logvue"));
                menu.AsSidebar("Admin");
            });
        });

        Assert.False(catalog.Get(null).IsDefined);
        Assert.Equal("Logs", catalog.Get("Admin").Sections.Single().Items.Single().Title);
        Assert.Equal(NetForgeMenuPlacement.Sidebar, catalog.Get("ADMIN").Placement);
    }

    [Fact]
    public void IncludeRegistered_UsesTheContributorDefaultAndSkipsOneAlreadyPlaced()
    {
        INetForgeMenuCatalog catalog = Catalog(services =>
        {
            services.AddNetForgeMenuContributor(new LogsContributor());
            services.AddNetForgeMenuContributor(new PeopleContributor());
            services.BuildMenu(menu =>
            {
                menu.AddMenu("Apps").AddContributor("People");
                menu.IncludeRegistered();
            });
        });

        Assert.Equal("People", catalog.Get(null).Sections.Single().Items.Single().Title);
        Assert.Equal("Logs", catalog.Get("Admin").Sections.Single().Items.Single().Title);
    }

    [Fact]
    public void AddContributors_DropsEveryUnplacedLibraryIntoThatGroup()
    {
        NetForgeMenuModel menu = Catalog(services =>
        {
            services.AddNetForgeMenuContributor(new LogsContributor());
            services.AddNetForgeMenuContributor(new PeopleContributor());
            services.BuildMenu("Office", menu => menu.AddMenu("Tools").AddContributors());
        }).Get("Office");

        Assert.Equal(["Logs", "People"], menu.Sections.Single().Items.Select(item => item.Title));
    }

    [Fact]
    public void Submenu_NestsChildrenAndLetsTheHostRetitleTheParent()
    {
        ForgeNavItem content = Catalog(services =>
        {
            services.BuildMenu(menu => menu.AddMenu("CMS").AddSubmenu("Content", children =>
            {
                children.AddCustom(Link("Pages", "/pages"));
                children.AddCustom(Link("Posts", "/posts"));
            },
            item => item.Title = "Publishing"));
        }).Get(null).Sections.Single().Items.Single();

        Assert.Equal("Publishing", content.Title);
        Assert.Equal("", content.Href);
        Assert.Equal(["Pages", "Posts"], content.Children.Select(item => item.Title));
    }

    [Fact]
    public void Order_SortsAndKeepsRegistrationOrderForTies()
    {
        IReadOnlyList<ForgeNavItem> items = Catalog(services =>
        {
            services.BuildMenu(menu => menu.AddItems().AddCustom(item =>
            {
                item.Title = "A";
                item.Url = "/a";
            }).AddCustom(item =>
            {
                item.Title = "B";
                item.Url = "/b";
                item.Order = 1;
            }).AddCustom(item =>
            {
                item.Title = "C";
                item.Url = "/c";
            }));
        }).Get(null).Sections.Single().Items;

        Assert.Equal(["A", "C", "B"], items.Select(item => item.Title));
    }

    [Fact]
    public void MissingContributor_FailsWhenTheMenuIsFirstBuilt()
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            Catalog(services => services.BuildMenu(menu => menu.AddMenu("Site").AddContributor("Missing"))));

        Assert.Contains("Missing", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateContributorName_FailsWhenTheMenuIsFirstBuilt()
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => Catalog(services =>
        {
            services.AddNetForgeMenuContributor(new LogsContributor());
            services.AddNetForgeMenuContributor(new DuplicateLogsContributor());
            services.BuildMenu(menu => menu.IncludeRegistered());
        }));

        Assert.Contains("LogVue", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ItemWithoutTitleOrDestination_FailsWhenTheMenuIsFirstBuilt()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Catalog(services => services.BuildMenu(menu => menu.AddItems().AddCustom(item => item.Url = "/x"))));
    }

    [Fact]
    public void Filter_HidesRolesVisibilityAndEmptyGroups()
    {
        NetForgeMenuModel menu = Catalog(services =>
        {
            services.BuildMenu(menu =>
            {
                menu.AddMenu("Secret", group => group.Roles = "Administrator")
                    .AddCustom(Link("Keys", "/keys"));
                menu.AddMenu("Open")
                    .AddCustom(item =>
                    {
                        item.Title = "Staff";
                        item.Url = "/staff";
                        item.IsVisible = user => user.IsInRole("Staff");
                    })
                    .AddSubmenu("Gone", children => children.AddCustom(item =>
                    {
                        item.Title = "Hidden";
                        item.Url = "/hidden";
                        item.Roles = "Administrator";
                    }));
            });
        }).Get(null);

        IReadOnlyList<Heroicsoft.NetForge.Abstractions.Navigation.ForgeNavSection> visible =
            NetForgeMenuFilter.Apply(menu.Sections, User("Staff"));

        Assert.Equal("Open", visible.Single().Heading);
        Assert.Equal("Staff", visible.Single().Items.Single().Title);
    }

    [Fact]
    public void Html_RendersTheActiveBranchAndSkipsHiddenLinks()
    {
        NetForgeMenuModel menu = Catalog(services =>
        {
            services.BuildMenu(menu => menu.AddMenu("Work")
                .AddSubmenu("Content", children => children.AddCustom(Link("Pages", "/pages/drafts")))
                .AddCustom(item =>
                {
                    item.Title = "Keys";
                    item.Url = "/keys";
                    item.Roles = "Administrator";
                }));
        }).Get(null);

        string html = Text(NetForgeMenuHtml.Render(menu, User("Staff"), "/pages/drafts"));

        Assert.Contains("href=\"/pages/drafts\"", html, StringComparison.Ordinal);
        Assert.Contains("open", html, StringComparison.Ordinal);
        Assert.Contains("active", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Keys", html, StringComparison.Ordinal);

        string top = Text(NetForgeMenuHtml.Render(menu, User("Staff"), "/pages/drafts", NetForgeMenuPlacement.Topbar));

        Assert.Contains("nf-shared-topnav", top, StringComparison.Ordinal);
        Assert.Contains("Work", top, StringComparison.Ordinal);
    }

    [Fact]
    public void BootstrapHtml_UsesNavLinksAndFontIcons()
    {
        NetForgeMenuModel menu = Catalog(services =>
        {
            services.BuildMenu(menu => menu.AddItems().AddCustom(item =>
            {
                item.Title = "Liked";
                item.Url = "/liked";
                item.Icon = "fa fa-heart";
            }));
        }).Get(null);

        string html = Text(BootstrapNetForgeMenuHtml.Render(menu, new ClaimsPrincipal(new ClaimsIdentity()), "/liked"));

        Assert.Contains("nav-link active", html, StringComparison.Ordinal);
        Assert.Contains("fa fa-heart", html, StringComparison.Ordinal);
        Assert.Contains("aria-current=\"page\"", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("fa fa-heart", true)]
    [InlineData("fa-heart", true)]
    [InlineData("bi bi-trash", false)]
    [InlineData("logs", false)]
    [InlineData("bi-house", false)]
    public void Icons_TreatFontClassesSeparatelyFromIconNames(string icon, bool font) =>
        Assert.Equal(font, NetForgeMenuIcons.IsFontClass(icon));

    [Fact]
    public void ToSvg_DrawsAKnownIcon()
    {
        string svg = IconSet.ToSvg("heart", 18);

        Assert.Contains("<svg", svg, StringComparison.Ordinal);
        Assert.Contains("<path", svg, StringComparison.Ordinal);
        Assert.Contains("aria-hidden=\"true\"", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void Match_BlankAndHashAreNotEveryPage_AndAChildMakesTheBranchCurrent()
    {
        var parent = new ForgeNavItem
        {
            Title = "Content",
            Href = "",
            Children =
            [
                new ForgeNavItem { Title = "Pages", Href = "/pages" },
            ],
        };

        Assert.False(ForgeNavMatch.IsCurrent(parent, "/pages"));
        Assert.False(ForgeNavMatch.IsCurrent(new ForgeNavItem { Title = "Top", Href = "#" }, "/pages"));
        Assert.True(ForgeNavMatch.IsInBranch(parent, "/pages"));
        Assert.True(ForgeNavMatch.IsCurrent(parent.Children[0], "pages/drafts?x=1"));
    }

    private static INetForgeMenuCatalog Catalog(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        services.AddNetForgeComponents();
        configure(services);
        return services.BuildServiceProvider().GetRequiredService<INetForgeMenuCatalog>();
    }

    private static Action<NetForgeMenuItemOptions> Link(string title, string url) => item =>
    {
        item.Title = title;
        item.Url = url;
    };

    private static ClaimsPrincipal User(string role) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], authenticationType: "test"));

    private static string Text(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    private sealed class LogsContributor : INetForgeMenuContributor
    {
        public string Name => "LogVue";

        public string DefaultMenu => "Maintenance";

        public string? DefaultArea => "Admin";

        public void Contribute(INetForgeMenuGroupBuilder group, Action<NetForgeMenuItemOptions>? configure)
        {
            group.AddCustom(item =>
            {
                item.Title = "Logs";
                item.Icon = "logs";
                item.Url = "/logvue";
                configure?.Invoke(item);
            });
        }
    }

    private sealed class PeopleContributor : INetForgeMenuContributor
    {
        public string Name => "People";

        public void Contribute(INetForgeMenuGroupBuilder group, Action<NetForgeMenuItemOptions>? configure)
        {
            group.AddCustom(item =>
            {
                item.Title = "People";
                item.Icon = "people";
                item.Url = "/people";
                configure?.Invoke(item);
            });
        }
    }

    private sealed class DuplicateLogsContributor : INetForgeMenuContributor
    {
        public string Name => "LogVue";

        public void Contribute(INetForgeMenuGroupBuilder group, Action<NetForgeMenuItemOptions>? configure)
        {
        }
    }
}
