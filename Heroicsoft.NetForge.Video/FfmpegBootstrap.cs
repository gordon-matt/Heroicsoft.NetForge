using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Heroicsoft.NetForge.Video;

/// <summary>
/// Finds ffmpeg and ffprobe, downloading an official build when the app asked for that and
/// nothing local is complete. Calls <see cref="FFmpeg.SetExecutablesPath(string)"/> so the rest
/// of Xabe (and this library) share one directory.
/// </summary>
public static class FfmpegBootstrap
{
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

    /// <summary>Looks on disk only. Does not download and does not change Xabe's static path.</summary>
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

    public static void Use(FfmpegLocation location) => FFmpeg.SetExecutablesPath(location.Directory);

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

        if (!hasFfmpeg || (requireFfprobe && !hasFfprobe))
        {
            return null;
        }

        return new FfmpegLocation(directory, ffmpeg, hasFfprobe ? ffprobe : ffmpeg);
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
