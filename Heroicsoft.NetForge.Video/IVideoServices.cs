namespace Heroicsoft.NetForge.Video;

public interface IVideoFrameService
{
    /// <summary>
    /// Writes one JPEG frame. <paramref name="at"/> defaults to three seconds, which is what the
    /// video apps already capture so a black opening frame is less likely.
    /// </summary>
    Task<bool> TryCaptureJpegAsync(
        string videoPath,
        string outputJpegPath,
        TimeSpan? at = null,
        CancellationToken cancellationToken = default);
}

public interface IVideoTranscoder
{
    /// <summary>
    /// Shrinks a video that is larger than the named maximum box. A video already inside the box
    /// is left as-is, unless it is stored sideways, in which case the pixels are turned upright
    /// and the bitrate is capped at the source. Returns <see cref="VideoTranscodeResult.Unchanged"/>
    /// when there is nothing to do, or the new file is not worth keeping. The output file exists
    /// only when the result says it was replaced.
    /// </summary>
    Task<VideoTranscodeResult> TranscodeToSizeAsync(VideoSizeTranscodeRequest request, CancellationToken cancellationToken = default);

    Task<string> DownscaleToMaxHeightAsync(VideoDownscaleRequest request, CancellationToken cancellationToken = default);
}

public interface IVideoIntegrityService
{
    Task<VideoIntegrityResult> VerifyAsync(
        string? ffmpegPath,
        string videoFilePath,
        CancellationToken cancellationToken = default);
}

public interface IPcmAudioEncoder
{
    Task<PcmEncodeResult> EncodeAsync(PcmAudioEncodeRequest request, CancellationToken cancellationToken = default);
}
