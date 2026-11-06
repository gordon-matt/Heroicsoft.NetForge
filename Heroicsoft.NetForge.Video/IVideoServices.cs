namespace Heroicsoft.NetForge.Video;

/// <summary>
/// Defines the contract for the video frame service.
/// </summary>
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

/// <summary>
/// Defines the contract for the video transcoder.
/// </summary>
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

    /// <summary>
    /// Gets the downscale to max height.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the string.</returns>
    Task<string> DownscaleToMaxHeightAsync(VideoDownscaleRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the contract for the video integrity service.
/// </summary>
public interface IVideoIntegrityService
{
    /// <summary>
    /// Verifies the value.
    /// </summary>
    /// <param name="ffmpegPath">The ffmpeg path.</param>
    /// <param name="videoFilePath">The video file path.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the video integrity result.</returns>
    Task<VideoIntegrityResult> VerifyAsync(
        string? ffmpegPath,
        string videoFilePath,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the contract for the pcm audio encoder.
/// </summary>
public interface IPcmAudioEncoder
{
    /// <summary>
    /// Encodes the value.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the pcm encode result.</returns>
    Task<PcmEncodeResult> EncodeAsync(PcmAudioEncodeRequest request, CancellationToken cancellationToken = default);
}