using System.Net;
using System.Security.Claims;
using System.Text;
using Heroicsoft.NetForge.Abstractions.Navigation;
using Heroicsoft.NetForge.Components.Navigation;
using Microsoft.AspNetCore.Html;

namespace Heroicsoft.NetForge.Components.Bootstrap.Navigation;

/// <summary>Bootstrap markup for the same menu the nf HTML helper renders.</summary>
public static class BootstrapNetForgeMenuHtml
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
        html.Append("<nav class=\"bf-shared-menu is-side\" aria-label=\"Navigation\">");

        foreach (ForgeNavSection section in sections)
        {
            html.Append("<div class=\"bf-shared-section\">");

            if (!string.IsNullOrWhiteSpace(section.Heading))
            {
                html.Append("<div class=\"bf-sidebar-heading\"><span>")
                    .Append(Encode(section.Heading))
                    .Append("</span></div>");
            }

            html.Append("<ul class=\"nav flex-column bf-nav\">");

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
        html.Append("<nav class=\"bf-shared-menu is-top\" aria-label=\"Navigation\">");
        html.Append("<details class=\"bf-shared-collapse\"><summary class=\"bf-shared-collapse-toggle\" aria-label=\"Menu\">");
        html.Append("<i class=\"bi bi-list\" aria-hidden=\"true\"></i></summary><ul class=\"nav bf-shared-topnav\">");

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
        html.Append("<li class=\"nav-item\">");

        if (item.Children.Count == 0)
        {
            WriteAnchor(html, item, requestPath, "nav-link");
        }
        else
        {
            bool open = ForgeNavMatch.IsInBranch(item, requestPath);
            html.Append(open ? "<details class=\"bf-nav-group\" open>" : "<details class=\"bf-nav-group\">");
            html.Append("<summary class=\"nav-link");
            AppendActive(html, open);
            html.Append("\">");
            WriteIcon(html, item);
            WriteLabel(html, item);
            html.Append("<i class=\"bi bi-chevron-down ms-auto bf-nav-chevron\" aria-hidden=\"true\"></i>");
            html.Append("</summary><ul class=\"nav flex-column bf-nav bf-nav-sub\">");

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
        html.Append(inDropdown ? "<li>" : "<li class=\"nav-item\">");

        if (item.Children.Count == 0)
        {
            WriteAnchor(html, item, requestPath, inDropdown ? "dropdown-item" : "nav-link");
        }
        else
        {
            html.Append("<details class=\"bf-nav-group\">");
            html.Append(inDropdown ? "<summary class=\"dropdown-item" : "<summary class=\"nav-link");
            AppendActive(html, ForgeNavMatch.IsInBranch(item, requestPath));
            html.Append("\">");

            if (!heading)
            {
                WriteIcon(html, item);
            }

            html.Append("<span>").Append(Encode(item.Title)).Append("</span>");
            html.Append("<i class=\"bi bi-chevron-down bf-nav-chevron\" aria-hidden=\"true\"></i></summary>");
            html.Append("<ul class=\"dropdown-menu\">");

            foreach (ForgeNavItem child in item.Children)
            {
                WriteTopItem(html, child, requestPath, inDropdown: true);
            }

            html.Append("</ul></details>");
        }

        html.Append("</li>");
    }

    private static void WriteAnchor(StringBuilder html, ForgeNavItem item, string requestPath, string css)
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

        html.Append('>');
        WriteIcon(html, item);
        html.Append("<span class=\"bf-truncate\">").Append(Encode(item.Title)).Append("</span>");

        if (item.External)
        {
            html.Append("<i class=\"bi bi-box-arrow-up-right ms-auto small\" aria-hidden=\"true\"></i>");
        }
        else if (item.Count is int count)
        {
            html.Append("<span class=\"ms-auto small text-body-secondary\">").Append(Encode(count.ToString("N0"))).Append("</span>");
        }
        else if (!string.IsNullOrEmpty(item.Badge))
        {
            html.Append("<span class=\"badge rounded-pill ms-auto ")
                .Append(Encode(item.BadgeCssClass))
                .Append("\">")
                .Append(Encode(item.Badge))
                .Append("</span>");
        }

        html.Append("</a>");
    }

    private static void WriteLabel(StringBuilder html, ForgeNavItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Href))
        {
            html.Append("<a class=\"bf-truncate\" href=\"").Append(Encode(item.Href)).Append("\" data-enhance=\"false\"");

            if (item.External)
            {
                html.Append(" target=\"_blank\" rel=\"noopener noreferrer\"");
            }

            html.Append('>').Append(Encode(item.Title)).Append("</a>");
            return;
        }

        html.Append("<span class=\"bf-truncate\">").Append(Encode(item.Title)).Append("</span>");
    }

    private static void WriteIcon(StringBuilder html, ForgeNavItem item)
    {
        if (item.TagIcon)
        {
            html.Append("<i class=\"bi bi-circle-fill bf-tag-icon\" aria-hidden=\"true\"></i>");
            return;
        }

        if (string.IsNullOrWhiteSpace(item.Icon))
        {
            return;
        }

        html.Append("<i class=\"").Append(Encode(NetForgeMenuIcons.BootstrapClass(item.Icon))).Append("\" aria-hidden=\"true\"></i>");
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
