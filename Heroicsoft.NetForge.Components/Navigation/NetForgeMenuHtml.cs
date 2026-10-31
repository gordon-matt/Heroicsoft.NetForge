using System.Net;
using System.Security.Claims;
using System.Text;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Icons;
using Heroicsoft.NetForge.Components.Layout;
using Microsoft.AspNetCore.Html;

namespace Heroicsoft.NetForge.Components.Navigation;

/// <summary>Renders a menu as HTML so MVC can draw the same tree as the Blazor component.</summary>
public static class NetForgeMenuHtml
{
    public static IHtmlContent Render(
        NetForgeMenuModel menu,
        ClaimsPrincipal user,
        string requestPath,
        NetForgeMenuPlacement? placement = null,
        NetForgeMenuPlacement? mainPlacement = null)
    {
        if (!menu.IsDefined)
        {
            return HtmlString.Empty;
        }

        NetForgeMenuPlacement chosen = placement ?? mainPlacement ?? menu.Placement;
        IReadOnlyList<ForgeNavSection> visible = NetForgeMenuFilter.Apply(menu.Sections, user);
        IReadOnlyList<ForgeNavSection> sections = mainPlacement is null
            ? menu.SectionsFor(placement, visible)
            : menu.ForSlot(chosen, mainPlacement.Value, requestPath, visible);

        if (sections.Count == 0)
        {
            return HtmlString.Empty;
        }
        var html = new StringBuilder();

        if (chosen == NetForgeMenuPlacement.Topbar)
        {
            WriteTop(html, sections, requestPath);
        }
        else
        {
            WriteSide(html, sections, requestPath);
        }

        return new HtmlString(html.ToString());
    }

    private static void WriteSide(StringBuilder html, IReadOnlyList<ForgeNavSection> sections, string requestPath)
    {
        html.Append("<nav class=\"nf-shared-menu is-side\" aria-label=\"Navigation\">");

        foreach (ForgeNavSection section in sections)
        {
            html.Append("<div class=\"nf-shared-section\">");
            WriteHeading(html, section.Heading, "nf-sidebar-heading");
            html.Append("<ul class=\"nf-nav\">");

            foreach (ForgeNavItem item in section.Items)
            {
                WriteSideItem(html, item, requestPath);
            }

            html.Append("</ul></div>");
        }

        html.Append("</nav>");
    }

    private static void WriteTop(StringBuilder html, IReadOnlyList<ForgeNavSection> sections, string requestPath)
    {
        html.Append("<nav class=\"nf-shared-menu is-top\" aria-label=\"Navigation\">");
        html.Append("<details class=\"nf-shared-collapse\"><summary class=\"nf-shared-collapse-toggle\" aria-label=\"Menu\">");
        html.Append(IconSet.ToSvg("menu", 18));
        html.Append("</summary><ul class=\"nf-shared-topnav\">");

        foreach (ForgeNavSection section in sections)
        {
            if (string.IsNullOrWhiteSpace(section.Heading))
            {
                foreach (ForgeNavItem item in section.Items)
                {
                    WriteTopItem(html, item, requestPath, inDropdown: false);
                }
            }
            else
            {
                var heading = new ForgeNavItem
                {
                    Title = section.Heading,
                    Href = string.Empty,
                    Icon = string.Empty,
                    Children = section.Items,
                };
                WriteTopItem(html, heading, requestPath, inDropdown: false, heading: true);
            }
        }

        html.Append("</ul></details></nav>");
    }

    private static void WriteSideItem(StringBuilder html, ForgeNavItem item, string requestPath)
    {
        html.Append("<li class=\"nf-nav-item\">");

        if (item.Children.Count == 0)
        {
            WriteAnchor(html, item, requestPath, "nf-nav-link", dropdown: false);
        }
        else
        {
            bool open = ForgeNavMatch.IsInBranch(item, requestPath);
            html.Append(open ? "<details class=\"nf-nav-group\" open>" : "<details class=\"nf-nav-group\">");
            html.Append("<summary class=\"nf-nav-link");
            AppendActive(html, ForgeNavMatch.IsInBranch(item, requestPath));
            html.Append("\">");
            WriteIcon(html, item, 18, null);
            WriteLabel(html, item, requestPath, allowLink: true);
            html.Append(IconSet.ToSvg("chevron-down", 14, "nf-nav-chevron"));
            html.Append("</summary><ul class=\"nf-nav nf-nav-sub\">");

            foreach (ForgeNavItem child in item.Children)
            {
                WriteSideItem(html, child, requestPath);
            }

            html.Append("</ul></details>");
        }

        html.Append("</li>");
    }

    private static void WriteTopItem(StringBuilder html, ForgeNavItem item, string requestPath, bool inDropdown, bool heading = false)
    {
        bool branch = item.Children.Count > 0;

        if (!inDropdown)
        {
            html.Append("<li class=\"nf-nav-item\">");
        }

        if (!branch)
        {
            WriteAnchor(html, item, requestPath, inDropdown ? "nf-menu-item" : "nf-nav-link", inDropdown);
        }
        else
        {
            html.Append("<details class=\"nf-nav-group\">");
            html.Append(inDropdown ? "<summary class=\"nf-menu-item" : "<summary class=\"nf-nav-link");
            AppendActive(html, ForgeNavMatch.IsInBranch(item, requestPath));
            html.Append("\">");

            if (!heading)
            {
                WriteIcon(html, item, 18, null);
            }

            html.Append("<span class=\"nf-nav-label\">").Append(Encode(item.Title)).Append("</span>");
            html.Append(IconSet.ToSvg("chevron-down", 14, "nf-nav-chevron"));
            html.Append("</summary>");
            html.Append(inDropdown ? "<div class=\"nf-menu-list\">" : "<div class=\"nf-menu-list is-start\" role=\"menu\">");

            foreach (ForgeNavItem child in item.Children)
            {
                WriteTopItem(html, child, requestPath, inDropdown: true);
            }

            html.Append("</div></details>");
        }

        if (!inDropdown)
        {
            html.Append("</li>");
        }
    }

    private static void WriteAnchor(StringBuilder html, ForgeNavItem item, string requestPath, string css, bool dropdown)
    {
        bool current = ForgeNavMatch.IsCurrent(item, requestPath);
        html.Append("<a class=\"").Append(css);
        AppendActive(html, current);

        if (!string.IsNullOrWhiteSpace(item.CssClass))
        {
            html.Append(' ').Append(Encode(item.CssClass));
        }

        html.Append("\" href=\"").Append(Encode(item.Href)).Append("\" data-enhance=\"false\"");

        if (!string.IsNullOrWhiteSpace(item.Tooltip))
        {
            html.Append(" title=\"").Append(Encode(item.Tooltip)).Append('"');
        }

        if (current)
        {
            html.Append(" aria-current=\"page\"");
        }

        if (item.External)
        {
            html.Append(" target=\"_blank\" rel=\"noopener noreferrer\"");
        }

        if (dropdown)
        {
            html.Append(" role=\"menuitem\"");
        }

        html.Append('>');
        WriteIcon(html, item, dropdown ? 16 : 18, null);
        html.Append("<span class=\"nf-nav-label\">").Append(Encode(item.Title)).Append("</span>");
        WriteTrailing(html, item);
        html.Append("</a>");
    }

    private static void WriteLabel(StringBuilder html, ForgeNavItem item, string requestPath, bool allowLink)
    {
        if (allowLink && !string.IsNullOrWhiteSpace(item.Href))
        {
            bool current = ForgeNavMatch.IsCurrent(item, requestPath);
            html.Append("<a class=\"nf-nav-label\" href=\"").Append(Encode(item.Href)).Append("\" data-enhance=\"false\"");

            if (current)
            {
                html.Append(" aria-current=\"page\"");
            }

            if (item.External)
            {
                html.Append(" target=\"_blank\" rel=\"noopener noreferrer\"");
            }

            html.Append('>').Append(Encode(item.Title)).Append("</a>");
            return;
        }

        html.Append("<span class=\"nf-nav-label\">").Append(Encode(item.Title)).Append("</span>");
    }

    private static void WriteTrailing(StringBuilder html, ForgeNavItem item)
    {
        if (item.External)
        {
            html.Append(IconSet.ToSvg("external", 14, "nf-nav-external"));
            return;
        }

        if (item.Count is int count)
        {
            html.Append("<span class=\"nf-nav-count\">").Append(Encode(count.ToString("N0"))).Append("</span>");
            return;
        }

        if (!string.IsNullOrEmpty(item.Badge))
        {
            html.Append("<span class=\"nf-badge nf-nav-badge ")
                .Append(Encode(ForgeBadge.ToneClass(item.BadgeCssClass)))
                .Append("\">")
                .Append(Encode(item.Badge))
                .Append("</span>");
        }
    }

    private static void WriteIcon(StringBuilder html, ForgeNavItem item, int size, string? cssClass)
    {
        if (item.TagIcon)
        {
            html.Append("<span class=\"nf-tag-icon\" aria-hidden=\"true\"></span>");
            return;
        }

        if (string.IsNullOrWhiteSpace(item.Icon))
        {
            return;
        }

        if (NetForgeMenuIcons.IsFontClass(item.Icon))
        {
            html.Append("<i class=\"").Append(Encode(item.Icon)).Append("\" aria-hidden=\"true\"></i>");
            return;
        }

        html.Append(IconSet.ToSvg(item.Icon, size, cssClass));
    }

    private static void WriteHeading(StringBuilder html, string? title, string css)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        html.Append("<p class=\"").Append(css).Append("\"><span>")
            .Append(Encode(title))
            .Append("</span></p>");
    }

    private static void AppendActive(StringBuilder html, bool active)
    {
        if (active)
        {
            html.Append(" active");
        }
    }

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
