<img src="https://github.com/gordon-matt/Heroicsoft.NetForge/blob/master/_Misc/Logo-Wide.jpg" alt="Logo" width="100%" />

Shared building blocks for the self-hosted MVC and Blazor apps. Authentication, result helpers, and the Hangfire dashboard filter are used by both. The two Blazor component packages share the same component names and services: use `Heroicsoft.NetForge.Components.Bootstrap` for apps built on Bootstrap and Bootswatch, and `Heroicsoft.NetForge.Components` for apps that use the NetForge `nf-` style (light and dark, no Bootstrap).

## Packages

| Package | What it replaces |
| --- | --- |
| `Heroicsoft.NetForge.Abstractions` | `AuthProvider`, `IUserContextService`, `BootswatchThemes`, `AppVersion`, `DatabaseProviders`, setup status, nav models |
| `Heroicsoft.NetForge.Results` | `ResultExtensions` (`To<T>`, `GetDisplayMessage`) |
| `Heroicsoft.NetForge.Authentication` | Identity / Keycloak / None registration, Keycloak back-channel and role mapping, `/auth/login`, setup redirect |
| `Heroicsoft.NetForge.Components` | Shared services (toasts, dialogs, sidebar refresh, paging maths, shell options) plus the 
f- styled shell, dialogs, toasts, pager, empty states, router, and light/dark toggle. No Bootstrap. |
| `Heroicsoft.NetForge.Components.Bootstrap` | The same set on Bootstrap/Bootswatch, with the Bootswatch theme picker. References Heroicsoft.NetForge.Components. |
| `Heroicsoft.NetForge.Hangfire` | Administrator-only Hangfire dashboard filter |

## New app

```xml
<!-- Pick one: -->
<PackageReference Include="Heroicsoft.NetForge.Components.Bootstrap" Version="1.0.0" />
<!-- <PackageReference Include="Heroicsoft.NetForge.Components" Version="1.0.0" /> -->
<PackageReference Include="Heroicsoft.NetForge.Authentication" Version="1.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="10.0.12" />
```

**Bootstrap apps:** restore Bootswatch, Bootstrap, and Bootstrap Icons into `wwwroot/lib` (see `templates/libman.json`). The theme picker swaps those stylesheets at runtime; the library does not embed them. Import `Heroicsoft.NetForge.Components.Bootstrap` in `_Imports.razor` and call `AddNetForgeBootstrapComponents(ui => ui.ThemeStorageKey = "myapp.theme")`.

**nf apps:** no `libman` needed. Import `Heroicsoft.NetForge.Components` and call `AddNetForgeComponents(ui => { ui.ThemeStorageKey = "myapp.theme"; ui.AccentColor = "#4f46e5"; ui.AccentColorDark = "#818cf8"; })`. Never import both component namespaces in one app: the shared component names would be ambiguous.

```csharp
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddNetForgeBootstrapComponents(ui =>
{
    ui.ThemeStorageKey = "myapp.theme";
});
builder.Services.Configure<ForgeShellOptions>(shell =>
{
    shell.Brand = "My App";
    shell.SearchUrlFormat = "/?q={0}";
    shell.Sections.Add(new ForgeNavSection
    {
        Items =
        [
            new ForgeNavItem { Title = "Home", Href = "/", Icon = "bi-house", MatchAll = true },
        ],
    });
});

var auth = builder.Services.AddNetForgeAuthentication<ApplicationUser, ApplicationRole, ApplicationDbContext>(
    builder.Configuration);

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseNetForgeSetupGate(); // after registering ISetupStatus, if the app has a wizard
app.MapControllers();
if (auth == AuthProvider.Identity)
{
    app.MapRazorPages();
}
```

`Components/App.razor`:

```razor
<ForgeHostPage Title="My App" IconHref="img/Icon.png" AppStylesheet="@Assets["css/site.css"]">
    <ForgeRouter AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)" />
</ForgeHostPage>
```

`Authentication:Provider` is `Identity`, `Keycloak`, or `None`, with the same `Authentication:Keycloak` keys the other apps already use. `None` signs every request in as a single administrator and must not be exposed beyond the machine running it.

Pages inject `ToastService`, `DialogService`, and `SidebarRefresh` instead of copying those types. `ShellLayout` is the default layout: set `ForgeShellOptions.Sections` for a static menu, or register `IShellNavigation` when the sidebar is loaded from data and call `SidebarRefresh.NotifyChanged()` after a write. Set `NavigationPlacement` to `Topbar` when those links belong in the header and the app should not render a sidebar.

Hangfire hosts add `Heroicsoft.NetForge.Hangfire` and pass `new AdministratorDashboardAuthorizationFilter()` in the dashboard options.

MVC hosts keep their own `/auth` controller and `IUserContextService`. Pass `IncludeAuthController = false`, `RegisterUserContext = false`, and `Identity.UseRelaxedPasswordPolicy = false`. Keycloak behind a reverse proxy also sets `RequireHttpsMetadata = false`, correlation cookies to `SameSite=None` / `Secure=Always`, and `RedirectBrowserToPublicAuthority = true`. Set `AccessDeniedPath` to `/Home/AccessDenied` and `RemoteFailurePath` to `/Home/Error`. When `Authentication:Keycloak:PublicOrigin` is set, post-logout redirects to `{PublicOrigin}/signout-callback-oidc`.

Blazor hosts leave those flags at their defaults and pass the app's existing theme storage key (`linkstash.theme`, and the same pattern in the other apps) so a stored theme survives the switch.

## Left in the app

Domain pages, EF contexts, and user-directory services stay in the app. `IUserInfoService` and the Keycloak admin client differ once they start returning each app's own user model, so they were not pulled into the library.
