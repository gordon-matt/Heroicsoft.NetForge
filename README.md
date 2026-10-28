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
| `Heroicsoft.NetForge.Drawing` | Resize, crop, rotate, transcode, and difference-hash for thumbnails and other rasters. SkiaSharp, not ImageSharp |
| `Heroicsoft.NetForge.Video` | FFmpeg bootstrap, frame capture, max-size shrink, height downscale, integrity check, and PCM-to-AAC. Xabe.FFmpeg |

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

`ThemeStorageKey` is the key for the default area (the public site, an empty name). A named area gets that key plus a lower-case suffix, so `myapp.theme` and `myapp.theme.admin` are remembered separately and every library on the same area follows the same choice. List the areas that should differ. Anything not listed, including the Identity UI, keeps the default area's theme. `ForgeHostPage` reads the route or the first path segment (`/Admin/...`). Set `Area="Admin"` when a screen lives at some other path but should still use that area, which is how LogVue or IdentityNexus can share the back-office theme from `/logs` or `/identity`. The value is a Bootswatch name, or for an nf app `light`, `dark`, or `system`. Null uses the app default.

```csharp
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddNetForgeBootstrapComponents(ui =>
{
    ui.ThemeStorageKey = "myapp.theme";
    ui.Areas["Admin"] = "darkly";
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

Pages inject `ToastService`, `DialogService`, and `SidebarRefresh` instead of copying those types. `ShellLayout` is the default layout: set `ForgeShellOptions.Sections` for a static menu, or register `IShellNavigation` when the sidebar is loaded from data. `GetSectionsAsync` is awaited on startup and again after `SidebarRefresh.NotifyChanged()`. A search format of `/?q={0}` fills the box from that query parameter and clears it when the box is submitted empty. `TopbarEndComponent` and `SidebarFooterComponent` add a bell or a pinned footer without replacing the layout. A nav item highlights a query with `ActiveQuery` (a null value means the parameter is absent) and can set `Tooltip`, `Count`, and `TagIcon`. Set `NavigationPlacement` to `Topbar` when those links belong in the header and the app should not render a sidebar.

Hangfire hosts add `Heroicsoft.NetForge.Hangfire` and pass `new AdministratorDashboardAuthorizationFilter()` in the dashboard options.

Image hosts add `Heroicsoft.NetForge.Drawing` and call `AddNetForgeDrawing()`. Downscales use the Mitchell cubic resampler. `ImageScaleMode.Max` keeps the aspect ratio and can enlarge a small source, matching the previous ImageSharp behaviour. Encodes are sRGB and drop EXIF, so read GPS or orientation before processing. Animated GIF and WebP keep only the first frame once they are resized. Difference hashes of real photos will not match hashes computed by ImageSharp, so recompute any stored perceptual hashes after switching. The package references `SkiaSharp.NativeAssets.Linux.NoDependencies`, so a Linux container does not need fontconfig or FreeType. Colour-managed print work is outside this library.

Video hosts add `Heroicsoft.NetForge.Video` and call `AddNetForgeVideo()`. Call `FfmpegBootstrap.EnsureAsync` at startup with the app's download directory and whether ffprobe is required. Xabe needs both `ffmpeg` and `ffprobe` in one directory when ffprobe is required. The native binaries are resolved on Windows and Linux the same way: a configured path, well-known locations, then an official download. A named size such as `Hd720` is a maximum box: a video already inside it is left as-is, unless it is stored sideways, in which case the pixels are turned upright with the bitrate capped at the source and the result kept only when it is at most 15% larger. A larger video is scaled down to fit, and a re-encode that is not smaller than the original is discarded. The height downscale does the same with the file size.

A new Blazor app can start from the template in `templates/NetForge.Blazor`:

```bash
dotnet new install templates/NetForge.Blazor
dotnet new netforge-blazor -n MyApp
```

Visual Studio lists it as **NetForge Blazor App** after that install (Visual Studio 2022 17.4 or later). The generated solution has the host, Core, Data, one project per database provider with an `InitialCreate` migration, Services, and Tests. It uses `Heroicsoft.NetForge.Components` (the `nf-` shell), the Extenso repository, and `Ardalis.Result`. A sample `Note` entity shows the service, controller, page, and test. Lowercase file names, the theme key, the SQLite file, and the seed email follow the project name.

MVC hosts keep their own `/auth` controller and `IUserContextService`. Pass `IncludeAuthController = false`, `RegisterUserContext = false`, and `Identity.UseRelaxedPasswordPolicy = false`. Keycloak behind a reverse proxy also sets `RequireHttpsMetadata = false`, correlation cookies to `SameSite=None` / `Secure=Always`, and `RedirectBrowserToPublicAuthority = true`. Set `AccessDeniedPath` to `/Home/AccessDenied` and `RemoteFailurePath` to `/Home/Error`. When `Authentication:Keycloak:PublicOrigin` is set, post-logout redirects to `{PublicOrigin}/signout-callback-oidc`.

Blazor hosts leave those flags at their defaults and pass the app's existing theme storage key (`linkstash.theme`, and the same pattern in the other apps) so a stored theme survives the switch.

## Left in the app

Domain pages, EF contexts, and user-directory services stay in the app. `IUserInfoService` and the Keycloak admin client differ once they start returning each app's own user model, so they were not pulled into the library.

## Support

If you find this project useful, consider supporting its development.

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_Logo.png"
         alt="Buy Me a Coffee"
         height="40">
  </a>&nbsp;&nbsp;&nbsp;
  <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/PayPal_Logo.png"
         alt="Donate with PayPal"
         height="40">
  </a>
</p>

<p align="center">
  Prefer using your phone? Scan the QR code:
</p>

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_QR.png"
         alt="Buy Me a Coffee QR Code"
         width="180">
  </a>
</p>

<img src="https://komarev.com/ghpvc/?username=gordon-matt&label=GitHub%20Hits%20Since%202025-06-01%3A%20&color=ff0000&style=flat" alt="gordon-matt" />