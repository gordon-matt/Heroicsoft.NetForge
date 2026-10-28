namespace Heroicsoft.NetForge.Drawing.Tests;

/// <summary>
/// Contract for <see cref="IImageProcessor"/>. These assertions describe the behaviour the apps
/// already rely on (box fit, no upscale, formats, dHash, crop, rotate). A later engine swap has
/// to keep them green.
/// </summary>
public sealed class ImageProcessorTests
{
    private readonly ImageProcessor processor = new();

    [Fact]
    public void Detect_reads_magic_bytes()
    {
        Assert.Equal(RasterFormat.Jpeg, ImageFormats.Detect([0xFF, 0xD8, 0xFF, 0xE0]));
        Assert.Equal(RasterFormat.Png, ImageFormats.Detect([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]));
        Assert.Equal(RasterFormat.Gif, ImageFormats.Detect("GIF89a"u8.ToArray()));
        Assert.Equal(RasterFormat.Bmp, ImageFormats.Detect("BM"u8.ToArray()));
        Assert.Equal(RasterFormat.Unknown, ImageFormats.Detect([0x00, 0x01, 0x02, 0x03]));
    }

    [Theory]
    [InlineData(".jpg", RasterFormat.Jpeg, "image/jpeg")]
    [InlineData(".png", RasterFormat.Png, "image/png")]
    [InlineData(".webp", RasterFormat.Webp, "image/webp")]
    [InlineData(".gif", RasterFormat.Gif, "image/gif")]
    public void Extension_mapping_round_trips(string extension, RasterFormat format, string contentType)
    {
        Assert.Equal(format, ImageFormats.FromExtension(extension));
        Assert.Equal(contentType, ImageFormats.ToContentType(format));
    }

    [Fact]
    public async Task EncodeBgra_then_identify_reports_size_and_format()
    {
        var encoded = await processor.EncodeBgraAsync(SolidBgra(30, 20, 10, 20, 30), 30, 20, new ImageEncode(RasterFormat.Png));

        Assert.Equal(RasterFormat.Png, encoded.Format);
        Assert.Equal(30, encoded.Width);
        Assert.Equal(20, encoded.Height);
        Assert.Equal(RasterFormat.Png, ImageFormats.Detect(encoded.Bytes));

        using var stream = new MemoryStream(encoded.Bytes);
        var info = await processor.TryIdentifyAsync(stream);

        Assert.NotNull(info);
        Assert.Equal(30, info.Width);
        Assert.Equal(20, info.Height);
        Assert.Equal(RasterFormat.Png, info.Format);
    }

    [Fact]
    public async Task TryIdentify_returns_null_for_garbage()
    {
        using var stream = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8]);
        Assert.Null(await processor.TryIdentifyAsync(stream));
        Assert.Null(await processor.TryDecodeAsync(stream));
    }

    [Fact]
    public async Task Resize_max_preserves_aspect_and_scales_to_the_box()
    {
        var source = await processor.EncodeBgraAsync(SolidBgra(100, 50, 200, 10, 10), 100, 50, new ImageEncode(RasterFormat.Png));

        var fitted = await processor.ResizeAsync(source.Bytes, new ImageTransform
        {
            Width = 40,
            Height = 40,
            Mode = ImageScaleMode.Max,
            Format = RasterFormat.Png,
        });

        Assert.Equal(40, fitted.Width);
        Assert.Equal(20, fitted.Height);

        // ImageSharp's Max mode enlarges a smaller source until one side hits the box.
        // Callers that must not upscale (cover thumbnails) check the size before calling.
        var small = await processor.EncodeBgraAsync(SolidBgra(10, 10, 1, 2, 3), 10, 10, new ImageEncode(RasterFormat.Png));
        var enlarged = await processor.ResizeAsync(small.Bytes, new ImageTransform
        {
            Width = 40,
            Height = 40,
            Mode = ImageScaleMode.Max,
            Format = RasterFormat.Png,
        });

        Assert.Equal(40, enlarged.Width);
        Assert.Equal(40, enlarged.Height);
    }

    [Fact]
    public async Task Resize_crop_fills_the_box()
    {
        var source = await processor.EncodeBgraAsync(SolidBgra(100, 50, 20, 20, 20), 100, 50, new ImageEncode(RasterFormat.Png));
        var filled = await processor.ResizeAsync(source.Bytes, new ImageTransform
        {
            Width = 20,
            Height = 20,
            Mode = ImageScaleMode.Crop,
            Format = RasterFormat.Png,
        });

        Assert.Equal(20, filled.Width);
        Assert.Equal(20, filled.Height);
    }

    [Fact]
    public async Task Resize_stretch_uses_the_exact_destination_size()
    {
        var source = await processor.EncodeBgraAsync(SolidBgra(8, 8, 0, 0, 255), 8, 8, new ImageEncode(RasterFormat.Png));

        var stretched = await processor.ResizeAsync(source.Bytes, new ImageTransform
        {
            Width = 10,
            Height = 30,
            Mode = ImageScaleMode.Stretch,
            Format = RasterFormat.Png,
        });

        Assert.Equal(10, stretched.Width);
        Assert.Equal(30, stretched.Height);
    }

    [Fact]
    public async Task Solid_colour_survives_a_downscale()
    {
        var source = await processor.EncodeBgraAsync(SolidBgra(80, 40, 220, 30, 40), 80, 40, new ImageEncode(RasterFormat.Png));
        var fitted = await processor.ResizeAsync(source.Bytes, new ImageTransform
        {
            Width = 40,
            Height = 40,
            Mode = ImageScaleMode.Max,
            Format = RasterFormat.Png,
        });

        using var stream = new MemoryStream(fitted.Bytes);
        var decoded = await processor.TryDecodeAsync(stream);

        Assert.NotNull(decoded);
        var pixel = Pixel(decoded, decoded.Width / 2, decoded.Height / 2);
        Assert.InRange(pixel.R, 210, 230);
        Assert.InRange(pixel.G, 20, 40);
        Assert.InRange(pixel.B, 30, 50);
    }

    [Fact]
    public async Task Jpeg_and_webp_outputs_use_their_container_signatures()
    {
        byte[] pixels = GradientBgra(32, 32);
        var jpeg = await processor.EncodeBgraAsync(pixels, 32, 32, new ImageEncode(RasterFormat.Jpeg, 80));
        var webp = await processor.EncodeBgraAsync(pixels, 32, 32, new ImageEncode(RasterFormat.Webp, 80));

        Assert.Equal(RasterFormat.Jpeg, ImageFormats.Detect(jpeg.Bytes));
        Assert.Equal(RasterFormat.Webp, ImageFormats.Detect(webp.Bytes));
        Assert.Equal(RasterFormat.Jpeg, jpeg.Format);
        Assert.Equal(RasterFormat.Webp, webp.Format);
    }

    [Fact]
    public async Task Lower_jpeg_quality_produces_a_smaller_file()
    {
        byte[] pixels = GradientBgra(64, 64);
        var high = await processor.EncodeBgraAsync(pixels, 64, 64, new ImageEncode(RasterFormat.Jpeg, 95));
        var low = await processor.EncodeBgraAsync(pixels, 64, 64, new ImageEncode(RasterFormat.Jpeg, 10));

        Assert.True(low.Bytes.Length < high.Bytes.Length);
    }

    [Fact]
    public async Task Resize_refuses_a_decompression_bomb()
    {
        var source = await processor.EncodeBgraAsync(SolidBgra(20, 20, 1, 1, 1), 20, 20, new ImageEncode(RasterFormat.Png));

        await Assert.ThrowsAsync<RasterTooLargeException>(() => processor.ResizeAsync(source.Bytes, new ImageTransform
        {
            Width = 10,
            Height = 10,
            MaxPixels = 10,
        }));
    }

    [Fact]
    public async Task TryShrinkFile_rewrites_only_when_the_image_is_larger_than_the_box()
    {
        string path = Path.Combine(Path.GetTempPath(), $"nf-draw-{Guid.NewGuid():N}.png");

        try
        {
            var source = await processor.EncodeBgraAsync(SolidBgra(40, 20, 9, 9, 9), 40, 20, new ImageEncode(RasterFormat.Png));
            await File.WriteAllBytesAsync(path, source.Bytes);

            bool changed = await processor.TryShrinkFileAsync(path, new ImageShrinkOptions
            {
                MaxWidth = 10,
                MaxHeight = 10,
            });

            Assert.True(changed);

            await using var shrunk = File.OpenRead(path);
            var info = await processor.TryIdentifyAsync(shrunk);
            Assert.NotNull(info);
            Assert.True(info.Width <= 10);
            Assert.True(info.Height <= 10);
            Assert.Equal(RasterFormat.Png, info.Format);

            bool second = await processor.TryShrinkFileAsync(path, new ImageShrinkOptions
            {
                MaxWidth = 10,
                MaxHeight = 10,
            });
            Assert.False(second);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task Crop_keeps_the_requested_region()
    {
        byte[] pixels = new byte[8 * 8 * 4];

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (x >= 4)
                {
                    SetPixel(pixels, 8, x, y, 0, 0, 255);
                }
                else
                {
                    SetPixel(pixels, 8, x, y, 255, 0, 0);
                }
            }
        }

        var source = await processor.EncodeBgraAsync(pixels, 8, 8, new ImageEncode(RasterFormat.Png));
        var cropped = await processor.CropAsync(
            new MemoryStream(source.Bytes),
            new ImageCropBox(4, 0, 4, 8, 4, 8),
            new ImageEncode(RasterFormat.Png));

        Assert.Equal(4, cropped.Width);
        Assert.Equal(8, cropped.Height);

        using var stream = new MemoryStream(cropped.Bytes);
        var decoded = await processor.TryDecodeAsync(stream);
        Assert.NotNull(decoded);
        var pixel = Pixel(decoded, 0, 0);
        Assert.True(pixel.B >= 240 && pixel.R <= 15, $"Expected blue, got R={pixel.R} G={pixel.G} B={pixel.B} A={pixel.A}");
    }

    [Fact]
    public async Task Rotate_90_swaps_the_sides()
    {
        byte[] pixels = new byte[4 * 2 * 4];
        Fill(pixels, 4, 2, 0, 255, 0);
        SetPixel(pixels, 4, 0, 0, 255, 0, 0);

        var source = await processor.EncodeBgraAsync(pixels, 4, 2, new ImageEncode(RasterFormat.Png));
        var rotated = await processor.RotateAsync(new MemoryStream(source.Bytes), 90, new ImageEncode(RasterFormat.Png));

        Assert.Equal(2, rotated.Width);
        Assert.Equal(4, rotated.Height);

        using var stream = new MemoryStream(rotated.Bytes);
        var decoded = await processor.TryDecodeAsync(stream);
        Assert.NotNull(decoded);

        // Clockwise 90 in a Y-down bitmap sends (0, 0) to (height - 1, 0) of the new image,
        // which is (1, 0) when the source is 4×2.
        var moved = Pixel(decoded, rotated.Width - 1, 0);
        Assert.InRange(moved.R, 240, 255);
        Assert.InRange(moved.G, 0, 15);
    }

    [Fact]
    public async Task Rotate_180_sends_the_origin_to_the_opposite_corner()
    {
        byte[] pixels = new byte[4 * 2 * 4];
        Fill(pixels, 4, 2, 0, 0, 0);
        SetPixel(pixels, 4, 0, 0, 255, 0, 0);

        var source = await processor.EncodeBgraAsync(pixels, 4, 2, new ImageEncode(RasterFormat.Png));
        var rotated = await processor.RotateAsync(new MemoryStream(source.Bytes), 180, new ImageEncode(RasterFormat.Png));

        Assert.Equal(4, rotated.Width);
        Assert.Equal(2, rotated.Height);

        using var stream = new MemoryStream(rotated.Bytes);
        var decoded = await processor.TryDecodeAsync(stream);
        Assert.NotNull(decoded);
        var moved = Pixel(decoded, 3, 1);
        Assert.InRange(moved.R, 240, 255);
    }

    [Fact]
    public async Task Difference_hash_is_stable_and_separates_distinct_images()
    {
        var uniform = await processor.EncodeBgraAsync(SolidBgra(32, 32, 40, 40, 40), 32, 32, new ImageEncode(RasterFormat.Png));
        long uniformHash = await processor.ComputeDifferenceHashAsync(new MemoryStream(uniform.Bytes));
        long uniformAgain = await processor.ComputeDifferenceHashAsync(new MemoryStream(uniform.Bytes));

        Assert.Equal(0, uniformHash);
        Assert.Equal(uniformHash, uniformAgain);
        Assert.Equal(0, processor.HammingDistance(uniformHash, uniformAgain));

        byte[] crafted = new byte[9 * 8 * 4];

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                byte value = (byte)(255 - (x * 20));
                SetPixel(crafted, 9, x, y, value, value, value);
            }
        }

        var exact = await processor.EncodeBgraAsync(crafted, 9, 8, new ImageEncode(RasterFormat.Png));
        long craftedHash = await processor.ComputeDifferenceHashAsync(new MemoryStream(exact.Bytes));

        Assert.Equal(-1L, craftedHash);
        Assert.Equal(64, processor.HammingDistance(uniformHash, craftedHash));
    }

    [Fact]
    public async Task Transcode_changes_the_container_without_changing_the_size()
    {
        var source = await processor.EncodeBgraAsync(SolidBgra(12, 7, 1, 2, 3), 12, 7, new ImageEncode(RasterFormat.Png));
        var jpeg = await processor.TranscodeAsync(new MemoryStream(source.Bytes), new ImageEncode(RasterFormat.Jpeg, 90));

        Assert.Equal(RasterFormat.Jpeg, jpeg.Format);
        Assert.Equal(12, jpeg.Width);
        Assert.Equal(7, jpeg.Height);
        Assert.Equal(RasterFormat.Jpeg, ImageFormats.Detect(jpeg.Bytes));
    }

    private static byte[] SolidBgra(int width, int height, byte r, byte g, byte b)
    {
        byte[] pixels = new byte[width * height * 4];
        Fill(pixels, width, height, r, g, b);
        return pixels;
    }

    private static byte[] GradientBgra(int width, int height)
    {
        byte[] pixels = new byte[width * height * 4];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SetPixel(pixels, width, x, y, (byte)(x * 255 / Math.Max(1, width - 1)), (byte)(y * 255 / Math.Max(1, height - 1)), 80);
            }
        }

        return pixels;
    }

    private static void Fill(byte[] bgra, int width, int height, byte r, byte g, byte b)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SetPixel(bgra, width, x, y, r, g, b);
            }
        }
    }

    private static void SetPixel(byte[] bgra, int width, int x, int y, byte r, byte g, byte b)
    {
        int offset = ((y * width) + x) * 4;
        bgra[offset] = b;
        bgra[offset + 1] = g;
        bgra[offset + 2] = r;
        bgra[offset + 3] = 255;
    }

    private static (byte R, byte G, byte B, byte A) Pixel(DecodedRaster image, int x, int y)
    {
        int offset = ((y * image.Width) + x) * 4;
        return (image.Rgba[offset], image.Rgba[offset + 1], image.Rgba[offset + 2], image.Rgba[offset + 3]);
    }
}
