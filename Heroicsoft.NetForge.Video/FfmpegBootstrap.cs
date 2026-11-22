using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Heroicsoft.NetForge.Video;

/// <summary>
/// Finds ffmpeg and ffprobe, downloading an official build when the app asked for that and
/// nothing local is complete. Calls <see cref="FFmpeg.SetExecutablesPath"/> so the rest
/// of Xabe (and this library) share one directory.
/// </summary>
public static class FfmpegBootstrap
{
    /// <summary>
    /// Selects a directory that contains ffmpeg, downloads the official build when <paramref name="options"/> allow it,
    /// and points Xabe at that directory.
    /// Returns <see langword="null"/> without changing Xabe's directory when no local tools are complete and a download
    /// is not requested, when the download fails, or when the files still do not satisfy
    /// <see cref="FfmpegBootstrapOptions.RequireFfprobe"/>.
    /// </summary>
    /// <param name="options">Where to look, where to download, and whether ffprobe is required.</param>
    /// <param name="logger">Receives the directory that was selected, or why resolution stopped.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <returns>The location now in use, or <see langword="null"/> when this call did not select one.</returns>
    /// <remarks>
    /// <see cref="Resolve"/> runs first and does not look in <see cref="FfmpegBootstrapOptions.DownloadDirectory"/>.
    /// That directory is probed here, and an official build is downloaded into it only when it is still incomplete.
    /// A download failure is logged and returned as <see langword="null"/>. Creating the download directory is outside that catch.
    /// </remarks>
    /// <exception cref="OperationCanceledException">The download was canceled. A partial download may remain on disk.</exception>
    public static async Task<FfmpegLocation?> EnsureAsync(
        FfmpegBootstrapOptions options,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var resolved = Resolve(options);

        if (resolved is not null)
        {
            Use(resolved);
            logger.LogInformation("FFmpeg tools directory: {Path}", resolved.Directory);
            return resolved;
        }

        if (!options.DownloadIfMissing || string.IsNullOrWhiteSpace(options.DownloadDirectory))
        {
            logger.LogInformation(
                "No local FFmpeg directory was found. Xabe will look for ffmpeg and ffprobe on PATH.");
            return null;
        }

        string downloadDir = Path.GetFullPath(options.DownloadDirectory);
        Directory.CreateDirectory(downloadDir);
        resolved = Probe(downloadDir, options.RequireFfprobe);

        if (resolved is not null)
        {
            Use(resolved);
            logger.LogInformation("FFmpeg tools directory (existing cache): {Path}", resolved.Directory);
            return resolved;
        }

        try
        {
            logger.LogInformation("Downloading FFmpeg (official build) to {Path}", downloadDir);
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, downloadDir, progress: null)
                .WaitAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to download FFmpeg into {Path}", downloadDir);
            return null;
        }

        resolved = Probe(downloadDir, options.RequireFfprobe);

        if (resolved is null)
        {
            logger.LogError("FFmpeg download finished but the binaries were not found under {Path}", downloadDir);
            return null;
        }

        Use(resolved);
        logger.LogInformation("FFmpeg tools directory (downloaded): {Path}", resolved.Directory);
        return resolved;
    }

    /// <summary>
    /// Looks on disk only. Does not download and does not change Xabe's static path.
    /// Returns the first candidate <see cref="Probe"/> accepts.
    /// </summary>
    /// <param name="options">The paths to try, in order, and whether ffprobe is required.</param>
    /// <returns>The first complete location, or <see langword="null"/>.</returns>
    /// <remarks>
    /// The order is <c>/usr/bin</c> when <see cref="FfmpegBootstrapOptions.PreferLinuxSystemDirectory"/> is set on Linux,
    /// then <see cref="FfmpegBootstrapOptions.ConfiguredPath"/>, then the well-known locations when
    /// <see cref="FfmpegBootstrapOptions.SearchWellKnownLocations"/> is set, then the application base directory
    /// and the working directory when <see cref="FfmpegBootstrapOptions.SearchApplicationDirectories"/> is set.
    /// </remarks>
    public static FfmpegLocation? Resolve(FfmpegBootstrapOptions options)
    {
        foreach (string? candidate in Candidates(options))
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var found = Probe(candidate, options.RequireFfprobe);

            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Points Xabe at <paramref name="location"/> by setting its executables directory to <see cref="FfmpegLocation.Directory"/>.
    /// </summary>
    /// <param name="location">Directory to use. The binaries are not checked again, and nothing is downloaded.</param>
    public static void Use(FfmpegLocation location) => FFmpeg.SetExecutablesPath(location.Directory);

    /// <summary>
    /// Checks <paramref name="path"/> for ffmpeg and, when <paramref name="requireFfprobe"/> is <see langword="true"/>, for ffprobe.
    /// Does not download and does not change Xabe's directory.
    /// </summary>
    /// <param name="path">
    /// A tools directory, or an existing file whose parent directory is checked.
    /// <see langword="null"/>, empty, and whitespace return <see langword="null"/>.
    /// </param>
    /// <param name="requireFfprobe">
    /// When <see langword="true"/>, ffprobe must sit beside ffmpeg.
    /// When <see langword="false"/>, ffmpeg alone is enough and <see cref="FfmpegLocation.FfprobePath"/> is that ffmpeg file.
    /// </param>
    /// <returns>
    /// The directory and binary paths, or <see langword="null"/> when the directory is missing, ffmpeg is missing,
    /// or ffprobe is required and missing.
    /// </returns>
    /// <remarks>
    /// Windows looks for <c>ffmpeg.exe</c> and <c>ffprobe.exe</c>. Other operating systems look for <c>ffmpeg</c> and <c>ffprobe</c>.
    /// </remarks>
    public static FfmpegLocation? Probe(string path, bool requireFfprobe)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        string full = Path.GetFullPath(path.Trim());
        string directory = File.Exists(full)
            ? Path.GetDirectoryName(full) ?? full
            : full;

        if (!Directory.Exists(directory))
        {
            return null;
        }

        bool windows = OperatingSystem.IsWindows();
        string ffmpeg = Path.Combine(directory, windows ? "ffmpeg.exe" : "ffmpeg");
        string ffprobe = Path.Combine(directory, windows ? "ffprobe.exe" : "ffprobe");
        bool hasFfmpeg = File.Exists(ffmpeg);
        bool hasFfprobe = File.Exists(ffprobe);

        return !hasFfmpeg || (requireFfprobe && !hasFfprobe) ? null : new FfmpegLocation(directory, ffmpeg, hasFfprobe ? ffprobe : ffmpeg);
    }

    private static IEnumerable<string?> Candidates(FfmpegBootstrapOptions options)
    {
        if (options.PreferLinuxSystemDirectory && OperatingSystem.IsLinux())
        {
            yield return "/usr/bin";
        }

        if (!string.IsNullOrWhiteSpace(options.ConfiguredPath))
        {
            yield return options.ConfiguredPath;
        }

        if (options.SearchWellKnownLocations)
        {
            if (OperatingSystem.IsWindows())
            {
                yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin");
                yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ffmpeg");
            }

            yield return "/usr/bin";
            yield return "/usr/local/bin";
        }

        if (options.SearchApplicationDirectories)
        {
            yield return AppContext.BaseDirectory;
            yield return Directory.GetCurrentDirectory();
        }
    }
}