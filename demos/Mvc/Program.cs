using Heroicsoft.NetForge.Components;
using NetForge.Demo.LibraryOne;
using NetForge.Demo.LibraryTwo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddLibraryOne();
builder.Services.AddLibraryTwo();
builder.Services.AddNetForgeComponents()
    .BuildMenu("Admin", menu =>
    {
        menu.AddMenu("Samples")
            .AddLibraryOne()
            .AddLibraryTwo();
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
