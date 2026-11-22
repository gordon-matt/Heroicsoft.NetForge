namespace Heroicsoft.NetForge.Video;

/// <summary>
/// Registers the video services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IVideoFrameService"/>, <see cref="IVideoTranscoder"/>,
    /// <see cref="IVideoIntegrityService"/>, and <see cref="IPcmAudioEncoder"/> as singletons.
    /// ffmpeg itself is located later with <see cref="FfmpegBootstrap.EnsureAsync"/>, because each app has its own directory.
    /// </summary>
    /// <param name="services">The collection that receives the four singletons.</param>
    /// <returns>The same collection, so registration can be chained.</returns>
    public static IServiceCollection AddNetForgeVideo(this IServiceCollection services)
    {
        services.AddSingleton<IVideoFrameService, VideoFrameService>();
        services.AddSingleton<IVideoTranscoder, VideoTranscoder>();
        services.AddSingleton<IVideoIntegrityService, VideoIntegrityService>();
        services.AddSingleton<IPcmAudioEncoder, PcmAudioEncoder>();
        return services;
    }
}