using Microsoft.Extensions.Logging.Abstractions;

namespace Heroicsoft.NetForge.Video.Tests;

public sealed class FfmpegBootstrapTests
{
    [Fact]
    public void Resolve_uses_a_configured_directory_that_contains_both_binaries()
    {
        string root = Path.Combine(Path.GetTempPath(), "nf-ffmpeg-" + Guid.NewGuid().ToString("N"));
        string tools = Path.Combine(root, "tools");
        Directory.CreateDirectory(tools);
        string exe = OperatingSystem.IsWindows() ? ".exe" : string.Empty;
        File.WriteAllText(Path.Combine(tools, "ffmpeg" + exe), "not a real binary");
        File.WriteAllText(Path.Combine(tools, "ffprobe" + exe), "not a real binary");

        try
        {
            var found = FfmpegBootstrap.Resolve(new FfmpegBootstrapOptions
            {
                ConfiguredPath = tools,
                SearchWellKnownLocations = false,
                DownloadIfMissing = false,
            });

            Assert.NotNull(found);
            Assert.Equal(Path.GetFullPath(tools), found.Directory);
            Assert.True(File.Exists(found.FfmpegPath));
            Assert.True(File.Exists(found.FfprobePath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Resolve_treats_a_configured_file_path_as_its_directory()
    {
        string root = Path.Combine(Path.GetTempPath(), "nf-ffmpeg-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string exe = OperatingSystem.IsWindows() ? ".exe" : string.Empty;
        string ffmpeg = Path.Combine(root, "ffmpeg" + exe);
        File.WriteAllText(ffmpeg, "x");
        File.WriteAllText(Path.Combine(root, "ffprobe" + exe), "x");

        try
        {
            var found = FfmpegBootstrap.Probe(ffmpeg, requireFfprobe: true);
            Assert.NotNull(found);
            Assert.Equal(Path.GetFullPath(root), found.Directory);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Resolve_returns_null_when_ffprobe_is_required_and_missing()
    {
        string root = Path.Combine(Path.GetTempPath(), "nf-ffmpeg-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string exe = OperatingSystem.IsWindows() ? ".exe" : string.Empty;
        File.WriteAllText(Path.Combine(root, "ffmpeg" + exe), "x");

        try
        {
            Assert.Null(FfmpegBootstrap.Probe(root, requireFfprobe: true));
            Assert.NotNull(FfmpegBootstrap.Probe(root, requireFfprobe: false));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Verify_skips_when_ffmpeg_is_missing()
    {
        var service = new VideoIntegrityService(NullLogger<VideoIntegrityService>.Instance);
        var result = await service.VerifyAsync(null, "anything.mp4");

        Assert.True(result.IsValid);
        Assert.True(result.WasSkipped);
    }
}
