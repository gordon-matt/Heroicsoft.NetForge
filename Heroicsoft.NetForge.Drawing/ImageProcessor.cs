using System.Numerics;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace Heroicsoft.NetForge.Drawing;

/// <summary>
/// SkiaSharp processor. Downscales with the Mitchell cubic resampler: Skia's default filter is
/// harsher than ImageSharp's Lanczos and makes thumbnails look soft or crunchy. Colour management
/// stops at sRGB; these apps make previews, not prints, so an ICC-aware library is not warranted.
/// Encoded output does not carry EXIF. Kinnect reads GPS with MetadataExtractor before this runs.
/// </summary>
public sealed class ImageProcessor : IImageProcessor
{
    /// <summary>Mitchell is the cubic kernel that stays closest to the previous Lanczos thumbnails.</summary>
    private static readonly SKSamplingOptions Mitchell = new(SKCubicResampler.Mitchell);

    private static readonly SKSamplingOptions Nearest = new(SKFilterMode.Nearest, SKMipmapMode.None);

    /// <summary>
    /// Attempts to identify.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the raster info.</returns>
    public async Task<RasterInfo?> TryIdentifyAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        return await TryIdentifyAsync(stream, cancellationToken);
    }

    /// <summary>
    /// Attempts to identify.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the raster info.</returns>
    public async Task<RasterInfo?> TryIdentifyAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        byte[] bytes = await ReadBytesAsync(stream, cancellationToken);
        return TryIdentify(bytes);
    }

    /// <summary>
    /// Attempts to decode.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the decoded raster.</returns>
    public async Task<DecodedRaster?> TryDecodeAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        try
        {
            byte[] bytes = await ReadBytesAsync(stream, cancellationToken);
            using var bitmap = Decode(bytes, ImageFormats.DefaultMaxPixels, target: null);
            return ToRaster(bitmap, DetectFormat(bytes));
        }
        catch (RasterDecodeException)
        {
            return null;
        }
    }

    /// <summary>
    /// Resizes the value.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="transform">The transform.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed image.</returns>
    public Task<ProcessedImage> ResizeAsync(string path, ImageTransform transform, CancellationToken cancellationToken = default) => WithFileAsync(path, stream => ResizeAsync(stream, transform, cancellationToken), cancellationToken);

    /// <summary>
    /// Resizes the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="transform">The transform.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed image.</returns>
    public async Task<ProcessedImage> ResizeAsync(ReadOnlyMemory<byte> source, ImageTransform transform, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream(source.ToArray(), writable: false);
        return await ResizeAsync(stream, transform, cancellationToken);
    }

    /// <summary>
    /// Resizes the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="transform">The transform.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed image.</returns>
    public async Task<ProcessedImage> ResizeAsync(Stream source, ImageTransform transform, CancellationToken cancellationToken = default)
    {
        byte[] bytes = await ReadBytesAsync(source, cancellationToken);
        using var bitmap = Decode(bytes, transform.MaxPixels, TargetBox(transform));
        using var transformed = Scale(bitmap, transform);
        var format = transform.Format is { } requested && requested != RasterFormat.Unknown
            ? requested
            : DetectFormat(bytes);
        return Encode(transformed, format, transform.Quality);
    }

    /// <summary>
    /// Attempts to shrink file.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="options">The options.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true when the condition is met; otherwise, false.</returns>
    public async Task<bool> TryShrinkFileAsync(string path, ImageShrinkOptions options, CancellationToken cancellationToken = default)
    {
        if ((options.MaxWidth <= 0 && options.MaxHeight <= 0) || !File.Exists(path))
        {
            return false;
        }

        try
        {
            RasterInfo? info;
            await using (var probe = File.OpenRead(path))
            {
                info = await TryIdentifyAsync(probe, cancellationToken);
            }

            if (info is null || (long)info.Width * info.Height > options.MaxPixels)
            {
                return false;
            }

            bool tooWide = options.MaxWidth > 0 && info.Width > options.MaxWidth;
            bool tooTall = options.MaxHeight > 0 && info.Height > options.MaxHeight;

            if (!tooWide && !tooTall)
            {
                return false;
            }

            var processed = await ResizeAsync(path, new ImageTransform
            {
                Width = options.MaxWidth,
                Height = options.MaxHeight,
                Mode = ImageScaleMode.Max,
                Format = info.Format,
                Quality = options.Quality ?? 75,
                StripMetadata = false,
                MaxPixels = options.MaxPixels,
            }, cancellationToken);

            string temporary = path + ".resize";
            await File.WriteAllBytesAsync(temporary, processed.Bytes, cancellationToken);
            File.Move(temporary, path, overwrite: true);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the crop.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="crop">The crop.</param>
    /// <param name="encode">The encode.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed image.</returns>
    public async Task<ProcessedImage> CropAsync(
        Stream source,
        ImageCropBox crop,
        ImageEncode encode,
        CancellationToken cancellationToken = default)
    {
        byte[] bytes = await ReadBytesAsync(source, cancellationToken);
        using var bitmap = Decode(bytes, ImageFormats.DefaultMaxPixels, target: null);
        using var cropped = CropBitmap(bitmap, crop.X, crop.Y, crop.Width, crop.Height);

        using var output = cropped.Width == crop.DestinationWidth && cropped.Height == crop.DestinationHeight
            ? cropped.Copy()
            : ResizeBitmap(cropped, crop.DestinationWidth, crop.DestinationHeight, Mitchell);

        return Encode(output, encode.Format, encode.Quality);
    }

    /// <summary>
    /// Rotates the value.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="degrees">The degrees.</param>
    /// <param name="encode">The encode.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed image.</returns>
    public async Task<ProcessedImage> RotateAsync(
        Stream source,
        float degrees,
        ImageEncode? encode,
        CancellationToken cancellationToken = default)
    {
        byte[] bytes = await ReadBytesAsync(source, cancellationToken);
        using var bitmap = Decode(bytes, ImageFormats.DefaultMaxPixels, target: null);
        using var rotated = RotateBitmap(bitmap, degrees);
        var format = encode?.Format is { } requested && requested != RasterFormat.Unknown
            ? requested
            : DetectFormat(bytes);
        return Encode(rotated, format, encode?.Quality ?? 75);
    }

    /// <summary>
    /// Gets the transcode.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="encode">The encode.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed image.</returns>
    public Task<ProcessedImage> TranscodeAsync(string path, ImageEncode encode, CancellationToken cancellationToken = default) => WithFileAsync(path, stream => TranscodeAsync(stream, encode, cancellationToken), cancellationToken);

    /// <summary>
    /// Gets the transcode.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="encode">The encode.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed image.</returns>
    public async Task<ProcessedImage> TranscodeAsync(Stream source, ImageEncode encode, CancellationToken cancellationToken = default)
    {
        byte[] bytes = await ReadBytesAsync(source, cancellationToken);
        using var bitmap = Decode(bytes, ImageFormats.DefaultMaxPixels, target: null);
        return Encode(bitmap, encode.Format, encode.Quality);
    }

    /// <summary>
    /// Encodes the bgra.
    /// </summary>
    /// <param name="bgra">The bgra.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="encode">The encode.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed image.</returns>
    public Task<ProcessedImage> EncodeBgraAsync(
        ReadOnlyMemory<byte> bgra,
        int width,
        int height,
        ImageEncode encode,
        CancellationToken cancellationToken = default)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width and height must be positive.");
        }

        if (bgra.Length < width * height * 4)
        {
            throw new ArgumentException("The BGRA buffer is shorter than width * height * 4.", nameof(bgra));
        }

        byte[] pixels = bgra.ToArray();
        var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

        try
        {
            using var wrapped = new SKBitmap();
            wrapped.InstallPixels(info, handle.AddrOfPinnedObject(), width * 4);
            using var copy = wrapped.Copy();
            return Task.FromResult(Encode(copy, encode.Format, encode.Quality));
        }
        finally
        {
            handle.Free();
        }
    }

    /// <summary>
    /// Computes the difference hash.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the long.</returns>
    public Task<long> ComputeDifferenceHashAsync(string path, CancellationToken cancellationToken = default) => WithFileAsync(path, stream => ComputeDifferenceHashAsync(stream, cancellationToken), cancellationToken);

    /// <summary>
    /// Computes the difference hash.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the long.</returns>
    public async Task<long> ComputeDifferenceHashAsync(Stream source, CancellationToken cancellationToken = default)
    {
        const int hashWidth = 9;
        const int hashHeight = 8;

        byte[] bytes = await ReadBytesAsync(source, cancellationToken);
        using var bitmap = Decode(bytes, ImageFormats.DefaultMaxPixels, target: null);
        ulong hash = 0;
        int bit = 0;

        if (bitmap.Width == hashWidth && bitmap.Height == hashHeight)
        {
            for (int y = 0; y < hashHeight; y++)
            {
                for (int x = 0; x < hashWidth - 1; x++)
                {
                    if (Luminance(bitmap.GetPixel(x, y)) > Luminance(bitmap.GetPixel(x + 1, y)))
                    {
                        hash |= 1UL << bit;
                    }

                    bit++;
                }
            }

            return unchecked((long)hash);
        }

        using var gray = ResizeBitmap(
            bitmap,
            hashWidth,
            hashHeight,
            Mitchell,
            new SKImageInfo(hashWidth, hashHeight, SKColorType.Gray8, SKAlphaType.Opaque));

        int rowBytes = gray.RowBytes;
        IntPtr pixels = gray.GetPixels();

        for (int y = 0; y < hashHeight; y++)
        {
            for (int x = 0; x < hashWidth - 1; x++)
            {
                byte left = Marshal.ReadByte(pixels, (y * rowBytes) + x);
                byte right = Marshal.ReadByte(pixels, (y * rowBytes) + x + 1);

                if (left > right)
                {
                    hash |= 1UL << bit;
                }

                bit++;
            }
        }

        return unchecked((long)hash);
    }

    /// <summary>
    /// Gets the hamming distance.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The hamming distance.</returns>
    public int HammingDistance(long left, long right) =>
        BitOperations.PopCount(unchecked((ulong)(left ^ right)));

    private static RasterInfo? TryIdentify(byte[] bytes)
    {
        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data);

        return codec is null ? null : new RasterInfo(codec.Info.Width, codec.Info.Height, MapFormat(codec.EncodedFormat));
    }

    private static SKBitmap Decode(byte[] bytes, long maxPixels, SKSizeI? target)
    {
        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data) ?? throw new RasterDecodeException("The image could not be decoded.");
        var info = codec.Info;

        if ((long)info.Width * info.Height > maxPixels)
        {
            throw new RasterTooLargeException($"{info.Width}x{info.Height} exceeds the decode limit of {maxPixels} pixels.");
        }

        int width = info.Width;
        int height = info.Height;

        if (target is { } box && (info.Width > box.Width || info.Height > box.Height))
        {
            float scale = Math.Min((float)box.Width / info.Width, (float)box.Height / info.Height);

            if (scale is > 0 and < 1)
            {
                var scaled = codec.GetScaledDimensions(scale);

                if (scaled.Width > 0 && scaled.Height > 0)
                {
                    width = scaled.Width;
                    height = scaled.Height;
                }
            }
        }

        var decodeInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        var bitmap = new SKBitmap(decodeInfo);
        var result = codec.GetPixels(decodeInfo, bitmap.GetPixels());

        if (result is not (SKCodecResult.Success or SKCodecResult.IncompleteInput))
        {
            bitmap.Dispose();
            throw new RasterDecodeException($"The image could not be decoded ({result}).");
        }

        return bitmap;
    }

    private static SKBitmap Scale(SKBitmap bitmap, ImageTransform transform)
    {
        int boundWidth = transform.Width > 0 ? transform.Width : bitmap.Width;
        int boundHeight = transform.Height > 0 ? transform.Height : bitmap.Height;

        if (transform.Mode == ImageScaleMode.Stretch)
        {
            return ResizeBitmap(bitmap, boundWidth, boundHeight, Mitchell);
        }

        double scale = transform.Mode == ImageScaleMode.Crop
            ? Math.Max((double)boundWidth / bitmap.Width, (double)boundHeight / bitmap.Height)
            : Math.Min((double)boundWidth / bitmap.Width, (double)boundHeight / bitmap.Height);

        int fittedWidth = Math.Max(1, (int)Math.Round(bitmap.Width * scale));
        int fittedHeight = Math.Max(1, (int)Math.Round(bitmap.Height * scale));
        var sampling = fittedWidth == bitmap.Width && fittedHeight == bitmap.Height ? Nearest : Mitchell;
        var fitted = ResizeBitmap(bitmap, fittedWidth, fittedHeight, sampling);

        if (transform.Mode != ImageScaleMode.Crop
            || (fitted.Width == boundWidth && fitted.Height == boundHeight))
        {
            return fitted;
        }

        int x = Math.Max(0, (fitted.Width - boundWidth) / 2);
        int y = Math.Max(0, (fitted.Height - boundHeight) / 2);

        try
        {
            return CropBitmap(fitted, x, y, Math.Min(boundWidth, fitted.Width), Math.Min(boundHeight, fitted.Height));
        }
        finally
        {
            fitted.Dispose();
        }
    }

    private static SKBitmap ResizeBitmap(SKBitmap bitmap, int width, int height, SKSamplingOptions sampling) =>
        ResizeBitmap(bitmap, width, height, sampling, new SKImageInfo(width, height, bitmap.ColorType, bitmap.AlphaType));

    private static SKBitmap ResizeBitmap(SKBitmap bitmap, int width, int height, SKSamplingOptions sampling, SKImageInfo destination)
    {
        if (bitmap.Width == width && bitmap.Height == height && bitmap.ColorType == destination.ColorType)
        {
            return bitmap.Copy();
        }

        var resized = bitmap.Resize(destination, sampling);

        return resized is null ? throw new RasterDecodeException($"Could not resize the image to {width}x{height}.") : resized;
    }

    private static SKBitmap CropBitmap(SKBitmap bitmap, int x, int y, int width, int height)
    {
        var subset = new SKBitmap(width, height, bitmap.ColorType, bitmap.AlphaType);

        if (!bitmap.ExtractSubset(subset, new SKRectI(x, y, x + width, y + height)))
        {
            subset.Dispose();
            throw new RasterDecodeException($"Could not crop {width}x{height} at ({x}, {y}).");
        }

        // ExtractSubset can alias the source. Copy so the caller can dispose the original.
        var copy = subset.Copy();
        subset.Dispose();
        return copy;
    }

    private static SKBitmap RotateBitmap(SKBitmap bitmap, float degrees)
    {
        float normalized = degrees % 360f;

        if (normalized < 0)
        {
            normalized += 360f;
        }

        if (Nearly(normalized, 0f) || Nearly(normalized, 360f))
        {
            return bitmap.Copy();
        }

        if (Nearly(normalized, 90f) || Nearly(normalized, 180f) || Nearly(normalized, 270f))
        {
            return RotateOrthogonal(bitmap, (int)Math.Round(normalized / 90f));
        }

        double radians = normalized * Math.PI / 180d;
        double sine = Math.Abs(Math.Sin(radians));
        double cosine = Math.Abs(Math.Cos(radians));
        int width = Math.Max(1, (int)Math.Ceiling((bitmap.Width * cosine) + (bitmap.Height * sine)));
        int height = Math.Max(1, (int)Math.Ceiling((bitmap.Width * sine) + (bitmap.Height * cosine)));
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);

        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        canvas.Translate(width / 2f, height / 2f);
        canvas.RotateDegrees(normalized);
        canvas.Translate(-bitmap.Width / 2f, -bitmap.Height / 2f);
        canvas.DrawBitmap(bitmap, 0, 0);
        canvas.Flush();

        using var snapshot = surface.Snapshot();
        return SKBitmap.FromImage(snapshot);
    }

    /// <summary>Right-angle turns use a pixel copy so a 90° rotate lands on the same corner ImageSharp did.</summary>
    private static SKBitmap RotateOrthogonal(SKBitmap source, int quarterTurns)
    {
        int turns = quarterTurns % 4;
        bool swap = turns is 1 or 3;
        int width = swap ? source.Height : source.Width;
        int height = swap ? source.Width : source.Height;
        var destination = new SKBitmap(new SKImageInfo(width, height, source.ColorType, source.AlphaType));

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var color = source.GetPixel(x, y);
                (int nx, int ny) = turns switch
                {
                    1 => (source.Height - 1 - y, x),
                    2 => (source.Width - 1 - x, source.Height - 1 - y),
                    _ => (y, source.Width - 1 - x),
                };
                destination.SetPixel(nx, ny, color);
            }
        }

        return destination;
    }

    private static ProcessedImage Encode(SKBitmap bitmap, RasterFormat format, int quality)
    {
        var output = format == RasterFormat.Unknown ? RasterFormat.Png : format;
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(ToEncodedFormat(output), Math.Clamp(quality, 1, 100))
            ?? throw new RasterDecodeException($"Could not encode {output}.");

        return new ProcessedImage(data.ToArray(), bitmap.Width, bitmap.Height, output);
    }

    private static DecodedRaster ToRaster(SKBitmap bitmap, RasterFormat format)
    {
        byte[] rgba = new byte[bitmap.Width * bitmap.Height * 4];

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                int offset = ((y * bitmap.Width) + x) * 4;
                rgba[offset] = color.Red;
                rgba[offset + 1] = color.Green;
                rgba[offset + 2] = color.Blue;
                rgba[offset + 3] = color.Alpha;
            }
        }

        return new DecodedRaster(bitmap.Width, bitmap.Height, format, rgba);
    }

    private static RasterFormat DetectFormat(byte[] bytes) => TryIdentify(bytes)?.Format ?? ImageFormats.Detect(bytes);

    private static RasterFormat MapFormat(SKEncodedImageFormat format) => format switch
    {
        SKEncodedImageFormat.Jpeg => RasterFormat.Jpeg,
        SKEncodedImageFormat.Png => RasterFormat.Png,
        SKEncodedImageFormat.Gif => RasterFormat.Gif,
        SKEncodedImageFormat.Webp => RasterFormat.Webp,
        SKEncodedImageFormat.Bmp => RasterFormat.Bmp,
        _ => RasterFormat.Unknown,
    };

    private static SKEncodedImageFormat ToEncodedFormat(RasterFormat format) => format switch
    {
        RasterFormat.Jpeg => SKEncodedImageFormat.Jpeg,
        RasterFormat.Webp => SKEncodedImageFormat.Webp,
        RasterFormat.Gif => SKEncodedImageFormat.Gif,
        RasterFormat.Bmp => SKEncodedImageFormat.Bmp,
        _ => SKEncodedImageFormat.Png,
    };

    private static SKSizeI? TargetBox(ImageTransform transform) => transform.Mode == ImageScaleMode.Stretch || transform.Width <= 0 || transform.Height <= 0
            ? null
            : new SKSizeI(transform.Width, transform.Height);

    private static int Luminance(SKColor color) =>
        (int)Math.Round((0.2126 * color.Red) + (0.7152 * color.Green) + (0.0722 * color.Blue));

    private static bool Nearly(float value, float target) => Math.Abs(value - target) < 0.01f;

    private static async Task<byte[]> ReadBytesAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }

    private static async Task<T> WithFileAsync<T>(string path, Func<Stream, Task<T>> action, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        return await action(stream);
    }
}