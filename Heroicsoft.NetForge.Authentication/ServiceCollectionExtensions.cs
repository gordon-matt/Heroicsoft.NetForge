using System.Security.Claims;
using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Authentication.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Heroicsoft.NetForge.Authentication;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Wires up whichever authentication provider <c>Authentication:Provider</c> names, and
    /// returns which one it was so <c>Program.cs</c> can decide whether Identity's Razor Pages
    /// need mapping.
    /// </summary>
    /// <remarks>
    /// Blazor hosts should also call <c>AddCascadingAuthenticationState</c>. When that provider
    /// is registered, <see cref="IUserContextService"/> reads the circuit. Otherwise it reads
    /// <see cref="HttpContext"/>, which is what MVC requests have.
    /// </remarks>
    public static AuthProvider AddNetForgeAuthentication<TUser, TRole, TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<NetForgeAuthOptions>? configure = null)
        where TUser : IdentityUser
        where TRole : IdentityRole
        where TContext : DbContext
    {
        var authOptions = new NetForgeAuthOptions();
        configuration.GetSection(NetForgeAuthOptions.SectionName).Bind(authOptions);
        configure?.Invoke(authOptions);
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(authOptions));

        services.AddHttpContextAccessor();

        var controllers = services.AddControllers();
        if (authOptions.IncludeAuthController)
        {
            controllers.AddApplicationPart(typeof(AuthController).Assembly);
        }

        string provider = string.IsNullOrWhiteSpace(authOptions.Provider)
            ? AuthProviderNames.Identity
            : authOptions.Provider.Trim();

        return string.Equals(provider, AuthProviderNames.Identity, StringComparison.OrdinalIgnoreCase)
            ? services.AddIdentityAuthentication<TUser, TRole, TContext>(authOptions)
            : string.Equals(provider, AuthProviderNames.Keycloak, StringComparison.OrdinalIgnoreCase)
            ? services.AddKeycloakAuthentication(authOptions)
            : string.Equals(provider, AuthProviderNames.None, StringComparison.OrdinalIgnoreCase)
            ? services.AddNoAuthentication(authOptions)
            : throw new InvalidOperationException(
            $"Unknown {NetForgeAuthOptions.SectionName}:Provider '{provider}'. " +
            $"Valid values: {string.Join(", ", AuthProviderNames.All)}.");
    }

    /// <summary>
    /// Registers the setup gate. The app must also register <see cref="Abstractions.Setup.ISetupStatus"/>
    /// and call <see cref="UseNetForgeSetupGate"/> after authentication and before endpoints.
    /// </summary>
    public static IServiceCollection AddNetForgeSetupGate(
        this IServiceCollection services,
        Action<SetupGateOptions>? configure = null)
    {
        if (configure is null)
        {
            services.AddOptions<SetupGateOptions>();
        }
        else
        {
            services.Configure(configure);
        }

        return services;
    }

    public static IApplicationBuilder UseNetForgeSetupGate(this IApplicationBuilder app) =>
        app.UseMiddleware<SetupRedirectMiddleware>();

    private static AuthProvider AddIdentityAuthentication<TUser, TRole, TContext>(
        this IServiceCollection services,
        NetForgeAuthOptions authOptions)
        where TUser : IdentityUser
        where TRole : IdentityRole
        where TContext : DbContext
    {
        services.AddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.Identity, null));
        services.AddNetForgeUserContext(authOptions, noneMode: false);

        services.AddIdentity<TUser, TRole>(options =>
            {
                var password = authOptions.Identity;
                if (!password.UseRelaxedPasswordPolicy)
                {
                    return;
                }

                options.Password.RequireDigit = password.RequireDigit;
                options.Password.RequireLowercase = password.RequireLowercase;
                options.Password.RequireUppercase = password.RequireUppercase;
                options.Password.RequireNonAlphanumeric = password.RequireNonAlphanumeric;
                options.Password.RequiredLength = password.RequiredLength;
                options.User.RequireUniqueEmail = password.RequireUniqueEmail;
                options.SignIn.RequireConfirmedAccount = password.RequireConfirmedAccount;
            })
            .AddEntityFrameworkStores<TContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

        return AuthProvider.Identity;
    }

    private static AuthProvider AddNoAuthentication(
        this IServiceCollection services,
        NetForgeAuthOptions authOptions)
    {
        services.AddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.None, null));
        services.AddNetForgeUserContext(authOptions, noneMode: true);

        services.AddAuthentication(NoneAuthenticationHandler.SchemeName)
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, NoneAuthenticationHandler>(
                NoneAuthenticationHandler.SchemeName,
                _ => { });

        return AuthProvider.None;
    }

    private static AuthProvider AddKeycloakAuthentication(
        this IServiceCollection services,
        NetForgeAuthOptions authOptions)
    {
        string? authority = authOptions.Keycloak.Authority;
        if (string.IsNullOrWhiteSpace(authority))
        {
            throw new InvalidOperationException(
                "Authentication:Provider is 'Keycloak' but Authentication:Keycloak:Authority is not set.");
        }

        services.AddSingleton<IAuthProviderService>(
            new AuthProviderService(AuthProvider.Keycloak, authority));
        services.AddNetForgeUserContext(authOptions, noneMode: false);
        services.AddHttpClient();

        string? publicOrigin = authOptions.Keycloak.PublicOrigin;
        string? backchannelBase = authOptions.Keycloak.BackchannelBaseUrl;
        var keycloak = authOptions.Keycloak;

        services.AddAuthentication(options =>
            {
                // The cookie holds the session; OIDC is only used to establish it.
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = keycloak.LoginPath;
                options.AccessDeniedPath = authOptions.AccessDeniedPath;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(Math.Max(1, keycloak.CookieDays));
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Authority = authority;

                if (!string.IsNullOrWhiteSpace(backchannelBase))
                {
                    options.BackchannelHttpHandler = new KeycloakBackchannelHandler(authority, backchannelBase);
                }

                options.ClientId = keycloak.ClientId;
                options.ClientSecret = keycloak.ClientSecret;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role,
                };

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.RequireHttpsMetadata = keycloak.RequireHttpsMetadata;
                options.NonceCookie.SameSite = keycloak.CorrelationCookieSameSite;
                options.CorrelationCookie.SameSite = keycloak.CorrelationCookieSameSite;
                options.NonceCookie.SecurePolicy = keycloak.CorrelationCookieSecurePolicy;
                options.CorrelationCookie.SecurePolicy = keycloak.CorrelationCookieSecurePolicy;

                // Keycloak does not advertise PAR support consistently across versions, and
                // enabling it opportunistically breaks the code flow on older realms.
                options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;

                string failurePath = authOptions.RemoteFailurePath ?? authOptions.AccessDeniedPath;

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProvider = context =>
                    {
                        if (!string.IsNullOrWhiteSpace(publicOrigin))
                        {
                            context.ProtocolMessage.RedirectUri = KeycloakClaims.RewriteOrigin(
                                context.ProtocolMessage.RedirectUri,
                                publicOrigin);
                        }

                        // Discovery through the back-channel can advertise the internal host. The
                        // browser has to be sent to the public authority instead.
                        if (keycloak.RedirectBrowserToPublicAuthority
                            && !string.IsNullOrWhiteSpace(backchannelBase))
                        {
                            string fullUrl = context.ProtocolMessage.CreateAuthenticationRequestUrl();
                            int queryIndex = fullUrl.IndexOf('?', StringComparison.Ordinal);
                            string query = queryIndex >= 0 ? fullUrl[queryIndex..] : string.Empty;
                            context.Response.Redirect(authority.TrimEnd('/') + "/protocol/openid-connect/auth" + query);
                            context.HandleResponse();
                        }

                        return Task.CompletedTask;
                    },

                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        if (!string.IsNullOrWhiteSpace(publicOrigin))
                        {
                            context.ProtocolMessage.PostLogoutRedirectUri =
                                publicOrigin.TrimEnd('/') + "/signout-callback-oidc";
                        }

                        return Task.CompletedTask;
                    },

                    OnTokenValidated = async context =>
                    {
                        // Keycloak puts realm roles in a nested "realm_access.roles" claim, which
                        // ASP.NET does not understand. Without flattening them every
                        // IsInRole("Administrator") check would fail.
                        KeycloakClaims.MapRoles(context.Principal);
                        KeycloakClaims.EnsureNameIdentifier(context.Principal);

                        if (keycloak.OnTokenValidated is not null)
                        {
                            await keycloak.OnTokenValidated(context);
                        }
                    },

                    OnUserInformationReceived = context =>
                    {
                        // Userinfo is merged after token validation and can drop the claims just
                        // copied, so they are re-applied.
                        KeycloakClaims.MapRoles(context.Principal);
                        KeycloakClaims.EnsureNameIdentifier(context.Principal);
                        return Task.CompletedTask;
                    },

                    OnRemoteFailure = context =>
                    {
                        context.Response.Redirect(failurePath);
                        context.HandleResponse();
                        return Task.CompletedTask;
                    },
                };
            });

        return AuthProvider.Keycloak;
    }

    private static void AddNetForgeUserContext(
        this IServiceCollection services,
        NetForgeAuthOptions authOptions,
        bool noneMode)
    {
        if (!authOptions.RegisterUserContext)
        {
            return;
        }

        if (noneMode)
        {
            services.AddSingleton<IUserContextService, NoneUserContextService>();
            return;
        }

        services.AddScoped<IUserContextService>(serviceProvider =>
        {
            var accessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var options = serviceProvider.GetRequiredService<IOptions<NetForgeAuthOptions>>();
            var authenticationState = serviceProvider.GetService<AuthenticationStateProvider>();

            return authenticationState is null
                ? new HttpUserContextService(accessor, options)
                : new BlazorAwareUserContextService(accessor, authenticationState, options);
        });
    }
}