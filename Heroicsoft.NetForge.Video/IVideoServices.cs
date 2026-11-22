namespace Heroicsoft.NetForge.Video;

/// <summary>
/// Captures one frame from a video file.
/// </summary>
public interface IVideoFrameService
{
    /// <summary>
    /// Writes one JPEG frame. <paramref name="at"/> defaults to three seconds, which is what the
    /// video apps already capture so a black opening frame is less likely.
    /// Returns <see langword="false"/> when <paramref name="videoPath"/> is missing, when the snapshot fails,
    /// or when the output file is not on disk afterward. An existing file at <paramref name="outputJpegPath"/> is overwritten.
    /// </summary>
    /// <param name="videoPath">Source video. A missing file returns <see langword="false"/>.</param>
    /// <param name="outputJpegPath">Frame to write. Its parent directory is created when the path has one and the directory is missing.</param>
    /// <param name="at">Capture position. <see langword="null"/>, the default, uses three seconds. <see cref="TimeSpan.Zero"/> captures the start of the file.</param>
    /// <param name="cancellationToken">Cancels the snapshot.</param>
    /// <returns><see langword="true"/> when <paramref name="outputJpegPath"/> exists after the snapshot finishes.</returns>
    /// <remarks>
    /// Snapshot failures, including a missing ffmpeg, are logged and returned as <see langword="false"/>.
    /// That <see langword="false"/> is returned even when an older file is still at <paramref name="outputJpegPath"/>.
    /// Creating the output directory happens before that catch, so a failure there propagates.
    /// </remarks>
    /// <exception cref="OperationCanceledException">The token was canceled. A partial output file may remain.</exception>
    Task<bool> TryCaptureJpegAsync(
        string videoPath,
        string outputJpegPath,
        TimeSpan? at = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Shrinks video that does not fit a named pixel box, and turns rotated video upright.
/// </summary>
public interface IVideoTranscoder
{
    /// <summary>
    /// Shrinks a video that is larger than the named maximum box. A video already inside the box
    /// is left as-is, unless it is stored sideways or upside down, in which case the pixels are turned upright
    /// and the bitrate is capped at the source. Returns <see cref="VideoTranscodeResult.Unchanged"/>
    /// when there is nothing to do, or the new file is not worth keeping. The input file is not deleted.
    /// <see cref="VideoSizeTranscodeRequest.OutputPath"/> is the file to keep only when the result says it was replaced.
    /// </summary>
    /// <param name="request">Input, output, size name, quality, audio bitrate, and whether to skip auto-rotation.</param>
    /// <param name="cancellationToken">Cancels probing and ffmpeg.</param>
    /// <returns>
    /// <see cref="VideoTranscodeResult.Replaced"/> when the output is a strictly smaller shrink, or an upright
    /// file within <see cref="VideoFit.UprightGrowthFactor"/> of the input. Otherwise
    /// <see cref="VideoTranscodeResult.Unchanged"/>. A discarded encode is deleted; an output that was never written is left alone.
    /// </returns>
    /// <remarks>
    /// An unknown <see cref="VideoSizeTranscodeRequest.VideoSizeName"/> uses the <c>Hd720</c> box.
    /// No video stream, or a non-positive width or height, leaves the file unchanged.
    /// A shrink is libx264 at <see cref="VideoSizeTranscodeRequest.Crf"/>, re-encodes audio to AAC at
    /// <see cref="VideoSizeTranscodeRequest.AudioBitrate"/> bits per second, and is kept only when it is strictly smaller than the input.
    /// An upright pass encodes at CRF 18, caps the video bitrate at the source stream, and copies audio.
    /// When that bitrate is not positive and the duration is longer than half a second, the cap is the file size in bits divided by the duration in seconds.
    /// A bitrate that is not positive on a clip of half a second or less is not capped. If copying audio fails, audio is re-encoded.
    /// The upright file is kept only when it stays within <see cref="VideoFit.UprightGrowthFactor"/>.
    /// <see cref="VideoSizeTranscodeRequest.Crf"/> is tried once more only when it is greater than 18 and the first upright file was discarded.
    /// The encode overwrites the output, forces an MP4 container (H.264, <c>yuv420p</c>, medium preset, faststart),
    /// maps the first video stream and the optional first audio stream, and passes <c>-noautorotate</c>.
    /// A 90, 180, or 270 degree rotation is transposed before a shrink unless <see cref="VideoSizeTranscodeRequest.NoAutoRotate"/> is set.
    /// The display-matrix angle is preferred, and the stream rotate tag is used when ffprobe does not report one.
    /// ffmpeg is the binary in Xabe's executables directory and is required only when an encode is needed.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Neither the requested size nor <c>Hd720</c> resolves, ffmpeg is missing when an encode is required, or the shrink encode fails.
    /// A failed shrink deletes the output. A failed upright encode returns <see cref="VideoTranscodeResult.Unchanged"/> instead of throwing.
    /// </exception>
    /// <exception cref="ArgumentException">The input file does not exist.</exception>
    /// <exception cref="OperationCanceledException">The token was canceled. A partial output file may remain.</exception>
    Task<VideoTranscodeResult> TranscodeToSizeAsync(VideoSizeTranscodeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces <see cref="VideoDownscaleRequest.VideoPath"/> with an H.264 file scaled to
    /// <see cref="VideoDownscaleRequest.MaxHeight"/> when its coded height is greater than that and the encode is strictly smaller.
    /// Audio is copied.
    /// The original bytes are kept when ffmpeg or the video is missing, when the coded height cannot be read,
    /// when that height is already within the maximum, or when the encode fails or is not smaller.
    /// </summary>
    /// <param name="request">ffmpeg location, video path, maximum coded height, CRF, and preset.</param>
    /// <param name="cancellationToken">Cancels ffprobe and ffmpeg.</param>
    /// <returns>
    /// <see cref="VideoDownscaleRequest.VideoPath"/> in every case. The file at that path was replaced only when a smaller encode was produced.
    /// </returns>
    /// <remarks>
    /// Height is the coded stream height, not the display size after rotation. The picture is scaled to
    /// <see cref="VideoDownscaleRequest.MaxHeight"/> pixels tall with an even width. The encode is libx264 at
    /// <see cref="VideoDownscaleRequest.Crf"/> and <see cref="VideoDownscaleRequest.Preset"/>, audio is copied, and the container is MP4,
    /// including when the original extension is not <c>.mp4</c>. ffprobe is the ffmpeg file name with <c>ffmpeg</c> replaced by <c>ffprobe</c>,
    /// in the same directory. Encode failures and exceptions other than cancellation are logged, and the original path is returned.
    /// A missing ffmpeg or video file returns that path without logging.
    /// </remarks>
    /// <exception cref="OperationCanceledException">The token was canceled. The original file is not replaced, and a temporary encode may remain.</exception>
    Task<string> DownscaleToMaxHeightAsync(VideoDownscaleRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Checks that the first video stream can be read without a fatal error.
/// </summary>
public interface IVideoIntegrityService
{
    /// <summary>
    /// Stream-copies the first video stream through ffmpeg with <c>-xerror</c> to a null output.
    /// Does not decode pixels and does not modify <paramref name="videoFilePath"/>.
    /// </summary>
    /// <param name="ffmpegPath">ffmpeg binary. <see langword="null"/>, blank, or a missing file skips the check.</param>
    /// <param name="videoFilePath">File to read. A missing file fails the check.</param>
    /// <param name="cancellationToken">Cancels ffmpeg.</param>
    /// <returns>
    /// <see cref="VideoIntegrityResult.Valid"/> when ffmpeg exits 0,
    /// <see cref="VideoIntegrityResult.Invalid"/> when it exits non-zero,
    /// and <see cref="VideoIntegrityResult.Skipped"/> when ffmpeg is missing or the check throws before it reports an exit code.
    /// A skipped result is still <see cref="VideoIntegrityResult.IsValid"/>.
    /// </returns>
    /// <remarks>
    /// Only stream <c>0:v:0</c> is mapped, so audio and extra video streams are not checked.
    /// <see cref="VideoIntegrityResult.Error"/> is the ffmpeg error text, cut off after about 4000 characters.
    /// When that text is empty, the error is a sentence that includes the exit code.
    /// </remarks>
    /// <exception cref="OperationCanceledException">The token was canceled.</exception>
    Task<VideoIntegrityResult> VerifyAsync(
        string? ffmpegPath,
        string videoFilePath,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Concatenates headerless PCM chunks and encodes them as AAC.
/// </summary>
public interface IPcmAudioEncoder
{
    /// <summary>
    /// Concatenates <see cref="PcmAudioEncodeRequest.PcmChunkFiles"/> in order and encodes them as AAC at
    /// <see cref="PcmAudioEncodeRequest.AudioBitrate"/>, overwriting <see cref="PcmAudioEncodeRequest.OutputPath"/>.
    /// </summary>
    /// <param name="request">Chunk paths, output path, and the PCM layout ffmpeg should assume.</param>
    /// <param name="cancellationToken">Cancels the copy and the encode.</param>
    /// <returns>
    /// The written path, its length in bytes, and the duration in seconds. The duration is 0 when the conversion returns no result.
    /// </returns>
    /// <remarks>
    /// The chunks are headerless signed 16-bit little-endian PCM at <see cref="PcmAudioEncodeRequest.SampleRateHz"/> hertz
    /// and <see cref="PcmAudioEncodeRequest.Channels"/> channels. Those values describe the input. They are not a resample or downmix target.
    /// The chunks are merged to <c>_merged.pcm</c> in the output directory, overwriting that file. That file is deleted after the encode
    /// returns or throws. A failure to delete it is logged and ignored. Copying the chunks is outside that cleanup, so a missing chunk
    /// or cancellation during the copy leaves <c>_merged.pcm</c> behind. The chunk files are left in place.
    /// The output arguments are AAC, the requested bitrate, and faststart. The container is the one ffmpeg selects for the output path.
    /// An existing output is overwritten only after the chunks have been merged. Canceling during the encode can leave a partial output.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// <see cref="PcmAudioEncodeRequest.PcmChunkFiles"/> is empty, or <see cref="PcmAudioEncodeRequest.OutputPath"/> is empty or a root path.
    /// </exception>
    /// <exception cref="ArgumentException"><see cref="PcmAudioEncodeRequest.OutputPath"/> is a file name with no directory, or Xabe has no ffmpeg executables.</exception>
    /// <exception cref="FileNotFoundException">A chunk file does not exist.</exception>
    /// <exception cref="DirectoryNotFoundException">A chunk path's directory does not exist.</exception>
    /// <exception cref="Xabe.FFmpeg.Exceptions.ConversionException">ffmpeg fails the encode.</exception>
    /// <exception cref="OperationCanceledException">
    /// The token was canceled. Canceling the encode still deletes the merged PCM file. Canceling the chunk copy leaves that file behind.
    /// </exception>
    Task<PcmEncodeResult> EncodeAsync(PcmAudioEncodeRequest request, CancellationToken cancellationToken = default);
}
