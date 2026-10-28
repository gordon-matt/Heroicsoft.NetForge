namespace Heroicsoft.NetForge.Video.Tests;

public sealed class VideoFitTests
{
    [Fact]
    public void Hd720_is_a_1280_by_720_box()
    {
        Assert.True(VideoSizes.TryGetPixelSize("Hd720", out int width, out int height));
        Assert.Equal(1280, width);
        Assert.Equal(720, height);
    }

    [Fact]
    public void A_frame_inside_the_box_fits_and_a_larger_frame_does_not()
    {
        Assert.True(VideoFit.FitsWithin(640, 360, 1280, 720));
        Assert.True(VideoFit.FitsWithin(1280, 720, 1280, 720));
        Assert.False(VideoFit.FitsWithin(1920, 1080, 1280, 720));
        Assert.False(VideoFit.FitsWithin(720, 1280, 1280, 720));
    }

    [Theory]
    [InlineData(1920, 1080, 90, 1080, 1920)]
    [InlineData(1920, 1080, -90, 1080, 1920)]
    [InlineData(1920, 1080, 270, 1080, 1920)]
    [InlineData(1920, 1080, 180, 1920, 1080)]
    [InlineData(1920, 1080, 0, 1920, 1080)]
    public void Display_size_swaps_axes_for_a_quarter_turn(int width, int height, int rotation, int expectedWidth, int expectedHeight)
    {
        (int displayWidth, int displayHeight) = VideoFit.DisplaySize(width, height, rotation);

        Assert.Equal(expectedWidth, displayWidth);
        Assert.Equal(expectedHeight, displayHeight);
    }

    [Theory]
    [InlineData(-90, false, "transpose=1")]
    [InlineData(90, false, "transpose=2")]
    [InlineData(90, true, "transpose=1")]
    [InlineData(270, true, "transpose=2")]
    [InlineData(180, true, "transpose=1,transpose=1")]
    [InlineData(180, false, "transpose=1,transpose=1")]
    public void Upright_transpose_matches_the_rotation_convention(int degrees, bool clockwise, string expected)
    {
        Assert.Equal(expected, VideoFit.UprightTranspose(degrees, clockwise));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(null, false)]
    public void An_upright_frame_has_no_transpose(int? degrees, bool clockwise)
    {
        Assert.Null(VideoFit.UprightTranspose(degrees, clockwise));
    }

    [Fact]
    public void An_upright_file_may_grow_by_fifteen_percent_and_no_more()
    {
        Assert.True(VideoFit.WithinUprightBudget(1000, 1150));
        Assert.False(VideoFit.WithinUprightBudget(1000, 1151));
    }
}
