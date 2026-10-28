using System.Diagnostics;
using System.Globalization;
using System.Text;
using Xabe.FFmpeg;

namespace Heroicsoft.NetForge.Video;

public sealed class VideoFrameService(ILogger<VideoFrameService> logger) : IVideoFrameService
{
    public async Task<bool> TryCaptureJpegAsync(
        string videoPath,
        string outputJpegPath,
        TimeSpan? at = null,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(videoPath))
        {
            return false;
        }

        string? directory = Path.GetDirectoryName(outputJpegPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(
                videoPath,
                outputJpegPath,
                at ?? TimeSpan.FromSeconds(3));
            conversion.SetOverwriteOutput(true);
            await conversion.Start(cancellationToken);
            return File.Exists(outputJpegPath);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to capture a frame from {Path}", videoPath);
            return false;
        }
    }
}

public sealed class VideoTranscoder(ILogger<VideoTranscoder> logger) : IVideoTranscoder
{
    /// <summary>
    /// Quality target for a turn-upright pass. The source bitrate is still a ceiling, so this
    /// spends bits on the rotated picture without opening the file up the way an uncapped CRF would.
    /// </summary>
    private const int UprightCrf = 18;

    public async Task<VideoTranscodeResult> TranscodeToSizeAsync(VideoSizeTranscodeRequest request, CancellationToken cancellationToken = default)
    {
        if (!VideoSizes.TryGetPixelSize(request.VideoSizeName, out int maxWidth, out int maxHeight)
            && !VideoSizes.TryGetPixelSize("Hd720", out maxWidth, out maxHeight))
        {
            throw new InvalidOperationException($"'{request.VideoSizeName}' is not a known video size.");
        }

        var mediaInfo = await FFmpeg.GetMediaInfo(request.InputPath, cancellationToken);
        var videoStream = mediaInfo.VideoStreams.FirstOrDefault();

        if (videoStream is null)
        {
            logger.LogWarning("No video stream found in {Path}; keeping the original.", request.InputPath);
            return VideoTranscodeResult.Unchanged;
        }

        (int? degrees, string? transpose) = request.NoAutoRotate
            ? (null, null)
            : await ReadOrientationAsync(request.InputPath, videoStream.Rotation, cancellationToken);

        (int width, int height) = request.NoAutoRotate
            ? (videoStream.Width, videoStream.Height)
            : VideoFit.DisplaySize(videoStream.Width, videoStream.Height, degrees);

        if (width <= 0 || height <= 0)
        {
            logger.LogWarning("Could not read the dimensions of {Path}; keeping the original.", request.InputPath);
            return VideoTranscodeResult.Unchanged;
        }

        bool shrink = !VideoFit.FitsWithin(width, height, maxWidth, maxHeight);

        if (!shrink && transpose is null)
        {
            logger.LogInformation(
                "Video {Path} is {Width}x{Height}, within the {MaxWidth}x{MaxHeight} maximum; keeping the original.",
                request.InputPath,
                width,
                height,
                maxWidth,
                maxHeight);
            return VideoTranscodeResult.Unchanged;
        }

        string? ffmpeg = ResolveFfmpegBinary();

        if (ffmpeg is null)
        {
            throw new InvalidOperationException("ffmpeg was not found. Call FfmpegBootstrap.EnsureAsync before transcoding.");
        }

        string? directory = Path.GetDirectoryName(request.OutputPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        long inputLength = new FileInfo(request.InputPath).Length;
        var filters = new List<string>();

        if (transpose is not null)
        {
            filters.Add(transpose);
        }

        if (shrink)
        {
            filters.Add(
                $"scale={maxWidth.ToString(CultureInfo.InvariantCulture)}:{maxHeight.ToString(CultureInfo.InvariantCulture)}:force_original_aspect_ratio=decrease:force_divisible_by=2");
        }

        string filter = string.Join(',', filters);

        if (shrink)
        {
            logger.LogInformation(
                "Shrinking video {Input} from {Width}x{Height} to fit inside {MaxWidth}x{MaxHeight}",
                request.InputPath,
                width,
                height,
                maxWidth,
                maxHeight);

            bool wrote = await TryEncodeAsync(
                ffmpeg,
                request,
                filter,
                request.Crf,
                maxVideoBitrate: null,
                copyAudio: false,
                cancellationToken);

            if (!wrote)
            {
                TryDelete(request.OutputPath);
                throw new InvalidOperationException($"ffmpeg could not shrink {request.InputPath}.");
            }

            long outputLength = new FileInfo(request.OutputPath).Length;

            if (outputLength >= inputLength)
            {
                logger.LogInformation(
                    "Shrink of {Path} produced {OutputBytes} bytes, which is not smaller than the original {InputBytes}; keeping the original.",
                    request.InputPath,
                    outputLength,
                    inputLength);
                TryDelete(request.OutputPath);
                return VideoTranscodeResult.Unchanged;
            }

            return VideoTranscodeResult.Replaced;
        }

        long maxRate = videoStream.Bitrate;

        if (maxRate <= 0 && videoStream.Duration.TotalSeconds > 0.5)
        {
            maxRate = (long)(inputLength * 8 / videoStream.Duration.TotalSeconds);
        }

        logger.LogInformation(
            "Turning {Path} upright ({Width}x{Height}) with the bitrate capped at {Bitrate}",
            request.InputPath,
            width,
            height,
            maxRate);

        if (!await TryEncodeUprightAsync(ffmpeg, request, filter, maxRate, UprightCrf, inputLength, cancellationToken)
            && request.Crf > UprightCrf)
        {
            logger.LogInformation(
                "Upright encode of {Path} at CRF {Crf} was too large; retrying at CRF {RetryCrf}.",
                request.InputPath,
                UprightCrf,
                request.Crf);
            await TryEncodeUprightAsync(ffmpeg, request, filter, maxRate, request.Crf, inputLength, cancellationToken);
        }

        if (!File.Exists(request.OutputPath))
        {
            logger.LogInformation(
                "Left {Path} unchanged. Turning it upright would have made the file more than {Percent}% larger.",
                request.InputPath,
                (int)Math.Round((VideoFit.UprightGrowthFactor - 1) * 100));
            return VideoTranscodeResult.Unchanged;
        }

        return VideoTranscodeResult.Replaced;
    }

    private async Task<bool> TryEncodeUprightAsync(
        string ffmpeg,
        VideoSizeTranscodeRequest request,
        string filter,
        long maxVideoBitrate,
        int crf,
        long inputLength,
        CancellationToken cancellationToken)
    {
        TryDelete(request.OutputPath);
        bool wrote = await TryEncodeAsync(ffmpeg, request, filter, crf, maxVideoBitrate, copyAudio: true, cancellationToken);

        if (!wrote)
        {
            TryDelete(request.OutputPath);
            wrote = await TryEncodeAsync(ffmpeg, request, filter, crf, maxVideoBitrate, copyAudio: false, cancellationToken);
        }

        if (!wrote || !File.Exists(request.OutputPath))
        {
            TryDelete(request.OutputPath);
            return false;
        }

        long outputLength = new FileInfo(request.OutputPath).Length;

        if (VideoFit.WithinUprightBudget(inputLength, outputLength))
        {
            return true;
        }

        logger.LogInformation(
            "Upright encode of {Path} was {OutputBytes} bytes against an original of {InputBytes}; discarding it.",
            request.InputPath,
            outputLength,
            inputLength);
        TryDelete(request.OutputPath);
        return false;
    }

    private async Task<bool> TryEncodeAsync(
        string ffmpeg,
        VideoSizeTranscodeRequest request,
        string filter,
        int crf,
        long? maxVideoBitrate,
        bool copyAudio,
        CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo
        {
            FileName = ffmpeg,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        start.ArgumentList.Add("-y");
        start.ArgumentList.Add("-nostdin");
        start.ArgumentList.Add("-noautorotate");
        start.ArgumentList.Add("-i");
        start.ArgumentList.Add(request.InputPath);
        start.ArgumentList.Add("-map");
        start.ArgumentList.Add("0:v:0");
        start.ArgumentList.Add("-map");
        start.ArgumentList.Add("0:a:0?");
        start.ArgumentList.Add("-vf");
        start.ArgumentList.Add(filter);
        start.ArgumentList.Add("-c:v");
        start.ArgumentList.Add("libx264");
        start.ArgumentList.Add("-preset");
        start.ArgumentList.Add("medium");
        start.ArgumentList.Add("-crf");
        start.ArgumentList.Add(crf.ToString(CultureInfo.InvariantCulture));

        if (maxVideoBitrate is > 0)
        {
            string bits = maxVideoBitrate.Value.ToString(CultureInfo.InvariantCulture);
            start.ArgumentList.Add("-maxrate");
            start.ArgumentList.Add(bits);
            start.ArgumentList.Add("-bufsize");
            start.ArgumentList.Add((maxVideoBitrate.Value * 2).ToString(CultureInfo.InvariantCulture));
        }

        start.ArgumentList.Add("-pix_fmt");
        start.ArgumentList.Add("yuv420p");

        if (copyAudio)
        {
            start.ArgumentList.Add("-c:a");
            start.ArgumentList.Add("copy");
        }
        else
        {
            start.ArgumentList.Add("-c:a");
            start.ArgumentList.Add("aac");
            start.ArgumentList.Add("-b:a");
            start.ArgumentList.Add(request.AudioBitrate.ToString(CultureInfo.InvariantCulture));
        }

        start.ArgumentList.Add("-movflags");
        start.ArgumentList.Add("+faststart");
        start.ArgumentList.Add("-f");
        start.ArgumentList.Add("mp4");
        start.ArgumentList.Add(request.OutputPath);

        (int exitCode, string error) = await RunAsync(start, cancellationToken, captureStdOut: false);
        bool wrote = exitCode == 0 && File.Exists(request.OutputPath) && new FileInfo(request.OutputPath).Length > 0;

        if (!wrote)
        {
            logger.LogWarning(
                "ffmpeg exited with code {ExitCode} for {Path}. {Error}",
                exitCode,
                request.InputPath,
                error);
            TryDelete(request.OutputPath);
        }

        return wrote;
    }

    private async Task<(int? Degrees, string? Transpose)> ReadOrientationAsync(
        string path,
        int? rotateTag,
        CancellationToken cancellationToken)
    {
        int? matrix = await ProbeStreamValueAsync(path, "stream_side_data=rotation", cancellationToken);

        if (matrix is not null)
        {
            return (matrix, VideoFit.UprightTranspose(matrix, clockwise: false));
        }

        if (rotateTag is not null)
        {
            return (rotateTag, VideoFit.UprightTranspose(rotateTag, clockwise: true));
        }

        return (null, null);
    }

    private static async Task<int?> ProbeStreamValueAsync(string videoPath, string entries, CancellationToken cancellationToken)
    {
        string? ffprobe = ResolveFfprobeBinary();

        if (ffprobe is null)
        {
            return null;
        }

        var start = new ProcessStartInfo
        {
            FileName = ffprobe,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        start.ArgumentList.Add("-v");
        start.ArgumentList.Add("error");
        start.ArgumentList.Add("-select_streams");
        start.ArgumentList.Add("v:0");
        start.ArgumentList.Add("-show_entries");
        start.ArgumentList.Add(entries);
        start.ArgumentList.Add("-of");
        start.ArgumentList.Add("default=noprint_wrappers=1:nokey=1");
        start.ArgumentList.Add(videoPath);

        (int exitCode, string output) = await RunAsync(start, cancellationToken, captureStdOut: true);

        if (exitCode != 0)
        {
            return null;
        }

        foreach (string line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return value;
            }
        }

        return null;
    }

    public async Task<string> DownscaleToMaxHeightAsync(VideoDownscaleRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FfmpegPath)
            || !File.Exists(request.FfmpegPath)
            || !File.Exists(request.VideoPath))
        {
            return request.VideoPath;
        }

        try
        {
            string ffprobe = Path.Combine(
                Path.GetDirectoryName(request.FfmpegPath) ?? string.Empty,
                Path.GetFileName(request.FfmpegPath).Replace("ffmpeg", "ffprobe", StringComparison.OrdinalIgnoreCase));

            int? height = await ProbeHeightAsync(ffprobe, request.VideoPath, cancellationToken);

            if (height is null || height <= request.MaxHeight)
            {
                return request.VideoPath;
            }

            logger.LogInformation(
                "Downscaling {File} from {Height}p to {MaxHeight}p",
                request.VideoPath,
                height,
                request.MaxHeight);

            string tempOutput = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid():N}{Path.GetExtension(request.VideoPath)}");

            var start = new ProcessStartInfo
            {
                FileName = request.FfmpegPath,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            start.ArgumentList.Add("-y");
            start.ArgumentList.Add("-nostdin");
            start.ArgumentList.Add("-i");
            start.ArgumentList.Add(request.VideoPath);
            start.ArgumentList.Add("-map");
            start.ArgumentList.Add("0:v:0");
            start.ArgumentList.Add("-map");
            start.ArgumentList.Add("0:a:0?");
            start.ArgumentList.Add("-vf");
            start.ArgumentList.Add($"scale=-2:{request.MaxHeight.ToString(CultureInfo.InvariantCulture)}");
            start.ArgumentList.Add("-c:v");
            start.ArgumentList.Add("libx264");
            start.ArgumentList.Add("-preset");
            start.ArgumentList.Add(request.Preset);
            start.ArgumentList.Add("-crf");
            start.ArgumentList.Add(request.Crf.ToString(CultureInfo.InvariantCulture));
            start.ArgumentList.Add("-c:a");
            start.ArgumentList.Add("copy");
            start.ArgumentList.Add("-movflags");
            start.ArgumentList.Add("+faststart");
            start.ArgumentList.Add("-f");
            start.ArgumentList.Add("mp4");
            start.ArgumentList.Add(tempOutput);

            (int exitCode, string error) = await RunAsync(start, cancellationToken, captureStdOut: false);

            if (exitCode != 0 || !File.Exists(tempOutput) || new FileInfo(tempOutput).Length == 0)
            {
                logger.LogWarning(
                    "Downscale of {File} failed (ffmpeg exit code {ExitCode}); keeping the original. {Error}",
                    request.VideoPath,
                    exitCode,
                    error);
                TryDelete(tempOutput);
                return request.VideoPath;
            }

            long originalLength = new FileInfo(request.VideoPath).Length;
            long outputLength = new FileInfo(tempOutput).Length;

            if (outputLength >= originalLength)
            {
                logger.LogInformation(
                    "Downscale of {File} produced {OutputBytes} bytes, which is not smaller than the original {InputBytes}; keeping the original.",
                    request.VideoPath,
                    outputLength,
                    originalLength);
                TryDelete(tempOutput);
                return request.VideoPath;
            }

            File.Delete(request.VideoPath);
            File.Move(tempOutput, request.VideoPath);
            return request.VideoPath;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Error downscaling {File}; keeping the original.", request.VideoPath);
            return request.VideoPath;
        }
    }

    private static async Task<int?> ProbeHeightAsync(string ffprobePath, string videoFilePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(ffprobePath))
        {
            return null;
        }

        var start = new ProcessStartInfo
        {
            FileName = ffprobePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        start.ArgumentList.Add("-v");
        start.ArgumentList.Add("error");
        start.ArgumentList.Add("-select_streams");
        start.ArgumentList.Add("v:0");
        start.ArgumentList.Add("-show_entries");
        start.ArgumentList.Add("stream=height");
        start.ArgumentList.Add("-of");
        start.ArgumentList.Add("csv=p=0");
        start.ArgumentList.Add(videoFilePath);

        (int exitCode, string output) = await RunAsync(start, cancellationToken, captureStdOut: true);

        if (exitCode != 0)
        {
            return null;
        }

        string firstLine = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? string.Empty;

        return int.TryParse(firstLine, NumberStyles.Integer, CultureInfo.InvariantCulture, out int height)
            ? height
            : null;
    }

    internal static async Task<(int ExitCode, string Output)> RunAsync(
        ProcessStartInfo startInfo,
        CancellationToken cancellationToken,
        bool captureStdOut)
    {
        using var process = new Process { StartInfo = startInfo };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null && stdout.Length < 4000)
            {
                stdout.AppendLine(e.Data);
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null && stderr.Length < 4000)
            {
                stderr.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync(cancellationToken);
        return (process.ExitCode, (captureStdOut ? stdout : stderr).ToString().Trim());
    }

    private static string? ResolveFfmpegBinary()
    {
        if (string.IsNullOrWhiteSpace(FFmpeg.ExecutablesPath))
        {
            return null;
        }

        string fileName = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
        string path = Path.Combine(FFmpeg.ExecutablesPath, fileName);
        return File.Exists(path) ? path : null;
    }

    private static string? ResolveFfprobeBinary()
    {
        string? ffmpeg = ResolveFfmpegBinary();

        if (ffmpeg is null)
        {
            return null;
        }

        string probe = Path.Combine(
            Path.GetDirectoryName(ffmpeg) ?? string.Empty,
            Path.GetFileName(ffmpeg).Replace("ffmpeg", "ffprobe", StringComparison.OrdinalIgnoreCase));
        return File.Exists(probe) ? probe : null;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            // The original file is still in place. A leftover temp is harmless.
        }
    }
}

public sealed class VideoIntegrityService(ILogger<VideoIntegrityService> logger) : IVideoIntegrityService
{
    public async Task<VideoIntegrityResult> VerifyAsync(
        string? ffmpegPath,
        string videoFilePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ffmpegPath) || !File.Exists(ffmpegPath))
        {
            logger.LogWarning(
                "ffmpeg not found at {Path}; skipping the integrity check for {File}",
                ffmpegPath,
                videoFilePath);
            return VideoIntegrityResult.Skipped;
        }

        var start = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // Stream-copy with -xerror reads every NAL unit and aborts on the first fatal error.
        // That catches container corruption a metadata-only ffprobe pass does not see, without
        // decoding pixels.
        foreach (string arg in new[] { "-v", "error", "-xerror", "-nostdin", "-i", videoFilePath, "-map", "0:v:0", "-c", "copy", "-f", "null", "-" })
        {
            start.ArgumentList.Add(arg);
        }

        try
        {
            (int exitCode, string error) = await VideoTranscoder.RunAsync(start, cancellationToken, captureStdOut: false);

            if (exitCode == 0)
            {
                return VideoIntegrityResult.Valid;
            }

            logger.LogWarning(
                "Integrity check failed for {File} (ffmpeg exit code {ExitCode}): {Error}",
                videoFilePath,
                exitCode,
                error);

            return VideoIntegrityResult.Invalid(string.IsNullOrEmpty(error)
                ? $"ffmpeg exited with code {exitCode}"
                : error);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not run the integrity check for {File}; skipping.", videoFilePath);
            return VideoIntegrityResult.Skipped;
        }
    }
}

public sealed class PcmAudioEncoder(ILogger<PcmAudioEncoder> logger) : IPcmAudioEncoder
{
    public async Task<PcmEncodeResult> EncodeAsync(PcmAudioEncodeRequest request, CancellationToken cancellationToken = default)
    {
        if (request.PcmChunkFiles.Count == 0)
        {
            throw new InvalidOperationException("Cannot encode audio with zero PCM chunks.");
        }

        string workingDir = Path.GetDirectoryName(request.OutputPath)
            ?? throw new InvalidOperationException("The output path has no directory.");
        Directory.CreateDirectory(workingDir);
        string mergedPcmPath = Path.Combine(workingDir, "_merged.pcm");

        await ConcatenateAsync(request.PcmChunkFiles, mergedPcmPath, cancellationToken);

        try
        {
            var conversion = FFmpeg.Conversions.New()
                .AddParameter(
                    $"-f s16le -ar {request.SampleRateHz.ToString(CultureInfo.InvariantCulture)} -ac {request.Channels.ToString(CultureInfo.InvariantCulture)}",
                    ParameterPosition.PreInput)
                .AddParameter($"-i \"{mergedPcmPath}\"", ParameterPosition.PreInput)
                .AddParameter($"-c:a aac -b:a {request.AudioBitrate} -movflags +faststart")
                .SetOverwriteOutput(true)
                .SetOutput(request.OutputPath);

            var info = await conversion.Start(cancellationToken);
            var file = new FileInfo(request.OutputPath);

            return new PcmEncodeResult(request.OutputPath, file.Length, info?.Duration.TotalSeconds ?? 0d);
        }
        finally
        {
            try
            {
                if (File.Exists(mergedPcmPath))
                {
                    File.Delete(mergedPcmPath);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Failed to delete intermediate PCM file {Path}", mergedPcmPath);
            }
        }
    }

    private static async Task ConcatenateAsync(
        IReadOnlyList<string> chunks,
        string outputPath,
        CancellationToken cancellationToken)
    {
        await using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

        foreach (string chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var input = new FileStream(chunk, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            await input.CopyToAsync(output, 81920, cancellationToken);
        }
    }
}
