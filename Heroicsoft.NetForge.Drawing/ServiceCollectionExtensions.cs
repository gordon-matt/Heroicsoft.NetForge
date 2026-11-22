namespace Heroicsoft.NetForge.Drawing;

/// <summary>Registers the NetForge image processor.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IImageProcessor"/> as a singleton. The type holds no per-request state.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddNetForgeDrawing(this IServiceCollection services)
    {
        services.AddSingleton<IImageProcessor, ImageProcessor>();
        return services;
    }
}