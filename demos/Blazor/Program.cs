using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Components;
using Heroicsoft.NetForge.Components.Layout;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetForge.Demo.Blazor.Components;
using NetForge.Demo.LibraryOne;
using NetForge.Demo.LibraryOne.Components;
using NetForge.Demo.LibraryTwo;
using NetForge.Demo.LibraryTwo.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLibraryOne();
builder.Services.AddLibraryTwo();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.TryAddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.None, null));
builder.Services.AddNetForgeComponents()
    .BuildMenu("Admin", menu =>
    {
        menu
            //.AsTopbar() // Uncomment this to make the menu appear in the topbar instead of the sidebar
            .AddMenu("Samples")
            .AddLibraryOne()
            .AddLibraryTwo();
    });
builder.Services.Configure<ForgeShellOptions>(shell =>
{
    shell.Brand = "Menu demo";
    shell.UseSharedMenu = true;
    shell.SharedMenuArea = "Admin";
    shell.ShowUserMenu = false;
    shell.ShowVersion = false;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/not-found");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(LibraryOnePage).Assembly, typeof(LibraryTwoPage).Assembly);

app.Run();