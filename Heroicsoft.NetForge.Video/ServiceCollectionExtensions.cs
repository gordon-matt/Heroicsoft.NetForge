namespace Heroicsoft.NetForge.Video;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the video helpers. ffmpeg itself is located later with
    /// <see cref="FfmpegBootstrap.EnsureAsync"/>, because each app has its own directory.
    /// </summary>
    public static IServiceCollection AddNetForgeVideo(this IServiceCollection services)
    {
        services.AddSingleton<IVideoFrameService, VideoFrameService>();
        services.AddSingleton<IVideoTranscoder, VideoTranscoder>();
        services.AddSingleton<IVideoIntegrityService, VideoIntegrityService>();
        services.AddSingleton<IPcmAudioEncoder, PcmAudioEncoder>();
        return services;
    }
}
