using Xabe.FFmpeg;

namespace Heroicsoft.NetForge.Video;

/// <summary>Names Xabe already accepts, such as <c>Hd720</c>. Kept here so apps can validate a setting without referencing Xabe.</summary>
public static class VideoSizes
{
    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="name"/> parses as a <see cref="VideoSize"/> name, ignoring case.
    /// </summary>
    /// <param name="name">A size name such as <c>Hd720</c>. <see langword="null"/>, empty, and whitespace return <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when the name is defined on <see cref="VideoSize"/>.</returns>
    public static bool IsKnown(string? name) =>
        !string.IsNullOrWhiteSpace(name) && Enum.TryParse<VideoSize>(name, ignoreCase: true, out _);

    /// <summary>
    /// Resolves the pixel box for a name such as <c>Hd720</c> (1280×720). The box is a maximum:
    /// a frame already inside it is not enlarged.
    /// </summary>
    /// <param name="name">A <see cref="VideoSize"/> name. Matching ignores case. <see langword="null"/> and unknown names return <see langword="false"/>.</param>
    /// <param name="width">Box width in pixels, or 0 when the method returns <see langword="false"/>.</param>
    /// <param name="height">Box height in pixels, or 0 when the method returns <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when <paramref name="name"/> maps to a positive width and height.</returns>
    public static bool TryGetPixelSize(string? name, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (!Enum.TryParse<VideoSize>(name, ignoreCase: true, out var size))
        {
            return false;
        }

        // Xabe keeps the pixel strings on an internal helper. These match VideoSize.ToFFmpegFormat in 6.1.1.
        (width, height) = size switch
        {
            VideoSize.Ntsc => (720, 480),
            VideoSize.Pal => (720, 576),
            VideoSize.Qntsc => (352, 240),
            VideoSize.Qpal => (352, 288),
            VideoSize.Sntsc => (640, 480),
            VideoSize.Spal => (768, 576),
            VideoSize.Film => (352, 240),
            VideoSize.NtscFilm => (352, 240),
            VideoSize.Sqcif => (128, 96),
            VideoSize.Qcif => (176, 144),
            VideoSize.Cif => (352, 288),
            VideoSize._4Cif => (704, 576),
            VideoSize._16cif => (1408, 1152),
            VideoSize.Qqvga => (160, 120),
            VideoSize.Qvga => (320, 240),
            VideoSize.Vga => (640, 480),
            VideoSize.Svga => (800, 600),
            VideoSize.Xga => (1024, 768),
            VideoSize.Uxga => (1600, 1200),
            VideoSize.Qxga => (2048, 1536),
            VideoSize.Sxga => (1280, 1024),
            VideoSize.Qsxga => (2560, 2048),
            VideoSize.Hsxga => (5120, 4096),
            VideoSize.Wvga => (852, 480),
            VideoSize.Wxga => (1366, 768),
            VideoSize.Wsxga => (1600, 1024),
            VideoSize.Wuxga => (1920, 1200),
            VideoSize.Woxga => (2560, 1600),
            VideoSize.Wqsxga => (3200, 2048),
            VideoSize.Wquxga => (3840, 2400),
            VideoSize.Whsxga => (6400, 4096),
            VideoSize.Whuxga => (7680, 4800),
            VideoSize.Cga => (320, 200),
            VideoSize.Ega => (640, 350),
            VideoSize.Hd480 => (852, 480),
            VideoSize.Hd720 => (1280, 720),
            VideoSize.Hd1080 => (1920, 1080),
            VideoSize._2K => (2048, 1080),
            VideoSize._2Kflat => (1998, 1080),
            VideoSize._2Kscope => (2048, 858),
            VideoSize._4K => (4096, 2160),
            VideoSize._4Kflat => (3996, 2160),
            VideoSize._4Kscope => (4096, 1716),
            VideoSize.Nhd => (640, 360),
            VideoSize.Hqvga => (240, 160),
            VideoSize.Wqvga => (400, 240),
            VideoSize.Fwqvga => (432, 240),
            VideoSize.Hvga => (480, 320),
            VideoSize.Qhd => (960, 540),
            VideoSize._2Kdci => (2048, 1080),
            VideoSize._4Kdci => (4096, 2160),
            VideoSize.Uhd2160 => (3840, 2160),
            VideoSize.Uhd4320 => (7680, 4320),
            _ => (0, 0)
        };

        return width > 0 && height > 0;
    }
}

/// <summary>Display size, rotation filters, and the upright file-size budget used by the size transcode.</summary>
public static class VideoFit
{
    /// <summary>
    /// Swaps the coded size when the container rotation is 90 or 270 degrees, which is how
    /// phone video is usually stored. The angle is normalized into 0–359, so -90 swaps as 270 does.
    /// 0, 180, and <see langword="null"/> keep the coded axes.
    /// </summary>
    /// <param name="width">Coded frame width in pixels.</param>
    /// <param name="height">Coded frame height in pixels.</param>
    /// <param name="rotationDegrees">Container rotation in degrees. <see langword="null"/> is treated as 0.</param>
    /// <returns>The display width and height. The axes are swapped only for a quarter turn.</returns>
    public static (int Width, int Height) DisplaySize(int width, int height, int? rotationDegrees)
    {
        int turns = (((rotationDegrees ?? 0) % 360) + 360) % 360;
        return turns is 90 or 270 ? (height, width) : (width, height);
    }

    /// <summary>
    /// Returns <see langword="true"/> when both dimensions are positive and neither exceeds its maximum.
    /// </summary>
    /// <param name="width">Frame width in pixels. Zero or negative returns <see langword="false"/>.</param>
    /// <param name="height">Frame height in pixels. Zero or negative returns <see langword="false"/>.</param>
    /// <param name="maxWidth">Maximum width in pixels, inclusive.</param>
    /// <param name="maxHeight">Maximum height in pixels, inclusive.</param>
    /// <returns>
    /// <see langword="true"/> when <paramref name="width"/> and <paramref name="height"/> are greater than zero
    /// and each is less than or equal to its maximum.
    /// </returns>
    public static bool FitsWithin(int width, int height, int maxWidth, int maxHeight) =>
        width > 0 && height > 0 && width <= maxWidth && height <= maxHeight;

    /// <summary>
    /// An upright re-encode may be a little larger than the source because the container and
    /// keyframes are rewritten. Past this, the original is kept instead.
    /// </summary>
    public const double UprightGrowthFactor = 1.15;

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="encodedBytes"/> is positive and no larger than
    /// <paramref name="originalBytes"/> multiplied by <see cref="UprightGrowthFactor"/>, rounded up.
    /// </summary>
    /// <param name="originalBytes">Size of the source file, in bytes. Zero or negative returns <see langword="false"/>.</param>
    /// <param name="encodedBytes">Size of the upright encode, in bytes. Zero or negative returns <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when the encoded file stays inside the upright growth budget.</returns>
    public static bool WithinUprightBudget(long originalBytes, long encodedBytes) =>
        originalBytes > 0 && encodedBytes > 0 && encodedBytes <= (long)Math.Ceiling(originalBytes * UprightGrowthFactor);

    /// <summary>
    /// Builds the ffmpeg transpose chain that bakes the rotation into the pixels.
    /// Returns <see langword="null"/> when the frame is already upright.
    /// </summary>
    /// <param name="degrees">Rotation from the file. Zero and <see langword="null"/> mean upright.</param>
    /// <param name="clockwise">
    /// <see langword="true"/> for the legacy <c>rotate</c> tag, where 90 means turn the picture clockwise (an iPhone portrait).
    /// <see langword="false"/> for ffprobe's display-matrix angle, which is counter-clockwise and is -90 for that same portrait.
    /// </param>
    /// <returns>
    /// <c>transpose=1</c> for a clockwise quarter turn, <c>transpose=2</c> for a counter-clockwise quarter turn,
    /// <c>transpose=1,transpose=1</c> for 180 degrees, and <see langword="null"/> for any other normalized angle.
    /// </returns>
    /// <remarks>The angle is normalized into 0–359 before the quarter-turn check, so a negative display-matrix angle is accepted.</remarks>
    public static string? UprightTranspose(int? degrees, bool clockwise)
    {
        int turns = (((degrees ?? 0) % 360) + 360) % 360;

        if (turns is not (90 or 180 or 270))
        {
            return null;
        }

        if (!clockwise && turns == 90)
        {
            turns = 270;
        }
        else if (!clockwise && turns == 270)
        {
            turns = 90;
        }

        return turns switch
        {
            90 => "transpose=1",
            180 => "transpose=1,transpose=1",
            270 => "transpose=2",
            _ => null
        };
    }
}

/// <summary>
/// Where ffmpeg and ffprobe were found. Xabe refuses to run unless both binaries sit in the
/// same directory, so a yt-dlp download that only shipped ffmpeg is not a usable location
/// when <see cref="FfmpegBootstrapOptions.RequireFfprobe"/> is left on.
/// </summary>
/// <param name="Directory">Directory that contains the binaries. <see cref="FfmpegBootstrap.Probe"/> stores a full path.</param>
/// <param name="FfmpegPath">Path of the ffmpeg binary.</param>
/// <param name="FfprobePath">
/// Path of ffprobe. When ffprobe was not required and was not present, this is the ffmpeg binary instead.
/// </param>
public sealed record FfmpegLocation(string Directory, string FfmpegPath, string FfprobePath);

/// <summary>
/// How to find or download ffmpeg. Each app fills in the directory it already uses
/// (content root, the yt-dlp tools folder, a TTS cache) and leaves the search order here.
/// </summary>
public sealed class FfmpegBootstrapOptions
{
    /// <summary>
    /// A file path or a directory from configuration. A file path uses its parent directory.
    /// <see langword="null"/> or whitespace is skipped.
    /// </summary>
    public string? ConfiguredPath { get; init; }

    /// <summary>
    /// Where an official build is downloaded when nothing else is on disk.
    /// <see langword="null"/> or whitespace skips the download even when <see cref="DownloadIfMissing"/> is <see langword="true"/>.
    /// </summary>
    public string? DownloadDirectory { get; init; }

    /// <summary>
    /// When <see langword="true"/>, also searches <c>ffmpeg\bin</c> under Program Files and <c>ffmpeg</c> under the local
    /// application data folder on Windows, then <c>/usr/bin</c> and <c>/usr/local/bin</c> on every operating system.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    public bool SearchWellKnownLocations { get; init; } = true;

    /// <summary>Also look next to the app and in the process working directory. Defaults to <see langword="false"/>.</summary>
    public bool SearchApplicationDirectories { get; init; }

    /// <summary>On Linux, prefer <c>/usr/bin</c> when ffmpeg is installed in the image. Defaults to <see langword="false"/>.</summary>
    public bool PreferLinuxSystemDirectory { get; init; }

    /// <summary>
    /// When <see langword="true"/>, <see cref="FfmpegBootstrap.EnsureAsync"/> downloads an official build into
    /// <see cref="DownloadDirectory"/> if no local directory is complete. Defaults to <see langword="true"/>.
    /// </summary>
    public bool DownloadIfMissing { get; init; } = true;

    /// <summary>
    /// Video and thumbnail work needs ffprobe beside ffmpeg. Audio-only encode can relax this.
    /// Defaults to <see langword="true"/>. When <see langword="false"/>, a directory that contains only ffmpeg is accepted.
    /// </summary>
    public bool RequireFfprobe { get; init; } = true;
}

/// <summary>Outcome of a stream-copy integrity check.</summary>
/// <param name="IsValid">
/// <see langword="true"/> when the file passed, or the check was skipped and the file was treated as acceptable.
/// </param>
/// <param name="WasSkipped">
/// <see langword="true"/> when ffmpeg was missing or the check threw before reporting an exit code, and the file was treated as acceptable.
/// </param>
/// <param name="Error">
/// ffmpeg error text when the file failed the check, or <see langword="null"/> when the file passed or the check was skipped.
/// </param>
public readonly record struct VideoIntegrityResult(bool IsValid, bool WasSkipped, string? Error)
{
    /// <summary>
    /// The file passed. <see cref="IsValid"/> is <see langword="true"/>, <see cref="WasSkipped"/> is <see langword="false"/>, and <see cref="Error"/> is <see langword="null"/>.
    /// </summary>
    public static readonly VideoIntegrityResult Valid = new(true, false, null);

    /// <summary>
    /// The check did not run and the file is treated as acceptable. <see cref="IsValid"/> and <see cref="WasSkipped"/> are <see langword="true"/>, and <see cref="Error"/> is <see langword="null"/>.
    /// </summary>
    public static readonly VideoIntegrityResult Skipped = new(true, true, null);

    /// <summary>
    /// Creates a failed result. <see cref="IsValid"/> and <see cref="WasSkipped"/> are <see langword="false"/>.
    /// </summary>
    /// <param name="error">Text stored in <see cref="Error"/>.</param>
    /// <returns>A result that rejects the file.</returns>
    public static VideoIntegrityResult Invalid(string error) => new(false, false, error);
}

/// <summary>
/// H.264 transcode that treats the named picture size (Xabe <see cref="VideoSize"/>, for example
/// <c>Hd720</c>) as a maximum box. A frame already inside the box is left untouched, except a
/// sideways or upside-down frame, which is turned upright with the bitrate capped so the file
/// does not grow much. A larger frame is scaled down to fit, never up. A shrink is discarded
/// when it is not smaller than the input.
/// </summary>
public sealed record VideoSizeTranscodeRequest
{
    /// <summary>Source file passed to ffmpeg. This file is not deleted.</summary>
    public required string InputPath { get; init; }

    /// <summary>
    /// Destination of an encode that is kept. An existing file here is overwritten when ffmpeg runs.
    /// Ignore this path when the result is <see cref="VideoTranscodeResult.Unchanged"/>: either nothing was written, or a discarded encode was deleted.
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// <see cref="VideoSize"/> name of the maximum box. Defaults to <c>Hd720</c>. Matching ignores case.
    /// An unknown name is treated as <c>Hd720</c>.
    /// </summary>
    public string VideoSizeName { get; init; } = "Hd720";

    /// <summary>
    /// libx264 CRF for a shrink. Defaults to 23.
    /// An upright pass encodes at CRF 18 first and uses this value for a second attempt only when it is greater than 18.
    /// </summary>
    public int Crf { get; init; } = 23;

    /// <summary>
    /// AAC bitrate in bits per second when audio is re-encoded. Defaults to 128000.
    /// An upright pass copies audio and uses this value only when that copy fails. A shrink always re-encodes audio.
    /// </summary>
    public int AudioBitrate { get; init; } = 128_000;

    /// <summary>
    /// When set, the fit check uses the coded frame and no transpose filter is added.
    /// Leave this unset so a phone video's display size is what the maximum box is compared with.
    /// Encodes pass <c>-noautorotate</c> either way, so ffmpeg does not rotate the picture on its own.
    /// </summary>
    public bool NoAutoRotate { get; init; }
}

/// <summary>
/// Whether <see cref="IVideoTranscoder.TranscodeToSizeAsync"/> left the file to keep at the output path.
/// </summary>
/// <param name="FileReplaced">
/// <see langword="true"/> when <see cref="VideoSizeTranscodeRequest.OutputPath"/> holds the file to keep: a smaller
/// shrink, or an upright copy that stayed within <see cref="VideoFit.UprightGrowthFactor"/> of the
/// original. <see langword="false"/> means the caller should keep the input and ignore the output path.
/// </param>
public readonly record struct VideoTranscodeResult(bool FileReplaced)
{
    /// <summary>
    /// <see cref="FileReplaced"/> is <see langword="false"/>. Keep the input and ignore the output path.
    /// </summary>
    public static readonly VideoTranscodeResult Unchanged = new(false);

    /// <summary>
    /// <see cref="FileReplaced"/> is <see langword="true"/>. The output path is the file to keep, and the input is left in place.
    /// </summary>
    public static readonly VideoTranscodeResult Replaced = new(true);
}

/// <summary>
/// Shrinks a file whose coded height is greater than <see cref="MaxHeight"/> and replaces that file when the encode is smaller.
/// Audio is copied. The original bytes are kept when the file is already short enough, when ffmpeg or ffprobe cannot be used,
/// or when the encode fails or is not smaller. The method returns <see cref="VideoPath"/> either way.
/// </summary>
public sealed record VideoDownscaleRequest
{
    /// <summary>
    /// Path to the ffmpeg binary. ffprobe is the same directory and file name with <c>ffmpeg</c> replaced by <c>ffprobe</c>.
    /// A blank path or a missing file skips the downscale.
    /// </summary>
    public required string FfmpegPath { get; init; }

    /// <summary>
    /// File to measure and, when a smaller encode is produced, replace in place. A missing file is returned unchanged.
    /// </summary>
    public required string VideoPath { get; init; }

    /// <summary>
    /// Coded height, in pixels, at or below which the file is left unchanged. A taller file is scaled to this height. Defaults to 720.
    /// </summary>
    public int MaxHeight { get; init; } = 720;

    /// <summary>libx264 CRF for the downscale. Defaults to 23.</summary>
    public int Crf { get; init; } = 23;

    /// <summary>libx264 preset passed through to ffmpeg. Defaults to <c>medium</c>.</summary>
    public string Preset { get; init; } = "medium";
}

/// <summary>Headerless s16le PCM chunks concatenated in order and encoded as AAC.</summary>
public sealed record PcmAudioEncodeRequest
{
    /// <summary>
    /// PCM files concatenated in list order. The files are not deleted. An empty list throws <see cref="InvalidOperationException"/>.
    /// </summary>
    public required IReadOnlyList<string> PcmChunkFiles { get; init; }

    /// <summary>
    /// Destination file, overwritten once encoding starts. The path must include a directory.
    /// A temporary <c>_merged.pcm</c> file is written beside it and deleted after the encode. A failure while reading the chunks leaves that file behind.
    /// </summary>
    public required string OutputPath { get; init; }

    /// <summary>
    /// Sample rate of the PCM input, in hertz. Defaults to 24000. The samples are not resampled to another rate.
    /// </summary>
    public int SampleRateHz { get; init; } = 24_000;

    /// <summary>Channel count of the PCM input. Defaults to 1. The samples are not downmixed.</summary>
    public int Channels { get; init; } = 1;

    /// <summary>AAC bitrate passed through to ffmpeg, such as <c>64k</c>. Defaults to <c>64k</c>.</summary>
    public string AudioBitrate { get; init; } = "64k";
}

/// <summary>The AAC file written by <see cref="IPcmAudioEncoder.EncodeAsync"/>.</summary>
/// <param name="FilePath">The output path that was written.</param>
/// <param name="FileSizeBytes">Length of that file, in bytes.</param>
/// <param name="DurationSeconds">Duration reported by the conversion, in seconds, or 0 when the conversion returns no result.</param>
public sealed record PcmEncodeResult(string FilePath, long FileSizeBytes, double DurationSeconds);