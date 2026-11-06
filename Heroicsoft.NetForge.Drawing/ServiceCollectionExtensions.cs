namespace Heroicsoft.NetForge.Drawing;

/// <summary>
/// Provides extension methods for service collections.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IImageProcessor"/> as a singleton. The type holds no per-request state.</summary>
    public static IServiceCollection AddNetForgeDrawing(this IServiceCollection services)
    {
        services.AddSingleton<IImageProcessor, ImageProcessor>();
        return services;
    }
}