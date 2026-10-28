using Xabe.FFmpeg;

namespace Heroicsoft.NetForge.Video;

/// <summary>Names Xabe already accepts, such as <c>Hd720</c>. Kept here so apps can validate a setting without referencing Xabe.</summary>
public static class VideoSizes
{
    public static bool IsKnown(string? name) =>
        !string.IsNullOrWhiteSpace(name) && Enum.TryParse<VideoSize>(name, ignoreCase: true, out _);

    /// <summary>
    /// Pixel box for a name such as <c>Hd720</c> (1280×720). The box is a maximum:
    /// a frame already inside it is not enlarged.
    /// </summary>
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

/// <summary>Display-size checks shared by the transcode and the height downscale.</summary>
public static class VideoFit
{
    /// <summary>
    /// Swaps the coded size when the container rotation is 90 or 270 degrees, which is how
    /// phone video is usually stored.
    /// </summary>
    public static (int Width, int Height) DisplaySize(int width, int height, int? rotationDegrees)
    {
        int turns = ((rotationDegrees ?? 0) % 360 + 360) % 360;
        return turns is 90 or 270 ? (height, width) : (width, height);
    }

    public static bool FitsWithin(int width, int height, int maxWidth, int maxHeight) =>
        width > 0 && height > 0 && width <= maxWidth && height <= maxHeight;

    /// <summary>
    /// An upright re-encode may be a little larger than the source because the container and
    /// keyframes are rewritten. Past this, the original is kept instead.
    /// </summary>
    public const double UprightGrowthFactor = 1.15;

    public static bool WithinUprightBudget(long originalBytes, long encodedBytes) =>
        originalBytes > 0 && encodedBytes > 0 && encodedBytes <= (long)Math.Ceiling(originalBytes * UprightGrowthFactor);

    /// <summary>
    /// ffmpeg transpose chain that bakes the rotation into the pixels. Null when the frame is already upright.
    /// </summary>
    /// <param name="degrees">Rotation from the file. Zero and null mean upright.</param>
    /// <param name="clockwise">
    /// True for the legacy <c>rotate</c> tag, where 90 means turn the picture clockwise (an iPhone portrait).
    /// False for ffprobe's display-matrix angle, which is counter-clockwise and is -90 for that same portrait.
    /// </param>
    public static string? UprightTranspose(int? degrees, bool clockwise)
    {
        int turns = ((degrees ?? 0) % 360 + 360) % 360;

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
/// same directory, so a yt-dlp download that only shipped ffmpeg is not a usable location.
/// </summary>
public sealed record FfmpegLocation(string Directory, string FfmpegPath, string FfprobePath);

/// <summary>
/// How to find or download ffmpeg. Each app fills in the directory it already uses
/// (content root, the yt-dlp tools folder, a TTS cache) and leaves the search order here.
/// </summary>
public sealed class FfmpegBootstrapOptions
{
    /// <summary>A file path or a directory from configuration. A file path uses its parent directory.</summary>
    public string? ConfiguredPath { get; init; }

    /// <summary>Where an official build is downloaded when nothing else is on disk.</summary>
    public string? DownloadDirectory { get; init; }

    public bool SearchWellKnownLocations { get; init; } = true;

    /// <summary>Also look next to the app and in the process working directory.</summary>
    public bool SearchApplicationDirectories { get; init; }

    /// <summary>On Linux, prefer <c>/usr/bin</c> when ffmpeg is installed in the image.</summary>
    public bool PreferLinuxSystemDirectory { get; init; }

    public bool DownloadIfMissing { get; init; } = true;

    /// <summary>Video and thumbnail work needs ffprobe beside ffmpeg. Audio-only encode can relax this.</summary>
    public bool RequireFfprobe { get; init; } = true;
}

/// <param name="IsValid">True when the file passed, or the check was skipped.</param>
/// <param name="WasSkipped">True when ffmpeg was missing and the file was treated as acceptable.</param>
/// <param name="Error">ffmpeg stderr when the file failed the check.</param>
public readonly record struct VideoIntegrityResult(bool IsValid, bool WasSkipped, string? Error)
{
    public static readonly VideoIntegrityResult Valid = new(true, false, null);

    public static readonly VideoIntegrityResult Skipped = new(true, true, null);

    public static VideoIntegrityResult Invalid(string error) => new(false, false, error);
}

/// <summary>
/// H.264 transcode that treats the named picture size (Xabe <c>VideoSize</c>, for example
/// <c>Hd720</c>) as a maximum box. A frame already inside the box is left untouched, except a
/// sideways or upside-down frame, which is turned upright with the bitrate capped so the file
/// does not grow much. A larger frame is scaled down to fit, never up. A shrink is discarded
/// when it is not smaller than the input.
/// </summary>
public sealed record VideoSizeTranscodeRequest
{
    public required string InputPath { get; init; }

    public required string OutputPath { get; init; }

    public string VideoSizeName { get; init; } = "Hd720";

    public int Crf { get; init; } = 23;

    public int AudioBitrate { get; init; } = 128_000;

    /// <summary>
    /// When set, the fit check uses the coded frame and ffmpeg is passed <c>-noautorotate</c>.
    /// Leave this unset so a phone video's display size is what the maximum box is compared with.
    /// </summary>
    public bool NoAutoRotate { get; init; }
}

/// <param name="FileReplaced">
/// True when <see cref="VideoSizeTranscodeRequest.OutputPath"/> holds the file to keep: a smaller
/// shrink, or an upright copy that stayed within <see cref="VideoFit.UprightGrowthFactor"/> of the
/// original. False means the caller should keep the input and ignore the output path.
/// </param>
public readonly record struct VideoTranscodeResult(bool FileReplaced)
{
    public static readonly VideoTranscodeResult Unchanged = new(false);

    public static readonly VideoTranscodeResult Replaced = new(true);
}

/// <summary>
/// Shrinks a file taller than <see cref="MaxHeight"/> and replaces it. Audio is copied.
/// Returns the path to keep, which is the original when the probe says it is already small
/// or when ffmpeg fails.
/// </summary>
public sealed record VideoDownscaleRequest
{
    public required string FfmpegPath { get; init; }

    public required string VideoPath { get; init; }

    public int MaxHeight { get; init; } = 720;

    public int Crf { get; init; } = 23;

    public string Preset { get; init; } = "medium";
}

/// <summary>Headerless s16le PCM chunks concatenated and encoded to AAC in an MP4 container.</summary>
public sealed record PcmAudioEncodeRequest
{
    public required IReadOnlyList<string> PcmChunkFiles { get; init; }

    public required string OutputPath { get; init; }

    public int SampleRateHz { get; init; } = 24_000;

    public int Channels { get; init; } = 1;

    public string AudioBitrate { get; init; } = "64k";
}

public sealed record PcmEncodeResult(string FilePath, long FileSizeBytes, double DurationSeconds);
