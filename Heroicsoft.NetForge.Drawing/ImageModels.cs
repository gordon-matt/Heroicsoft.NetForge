namespace Heroicsoft.NetForge.Drawing;

/// <summary>Dimensions and container format, without pixel data.</summary>
public sealed record RasterInfo(int Width, int Height, RasterFormat Format);

/// <summary>A decoded frame in tightly packed RGBA (one byte each, row-major, no padding).</summary>
public sealed record DecodedRaster(int Width, int Height, RasterFormat Format, byte[] Rgba);

/// <summary>Bytes produced by a resize, crop, rotate, or encode, plus the size actually written.</summary>
public sealed record ProcessedImage(byte[] Bytes, int Width, int Height, RasterFormat Format);

/// <summary>
/// How a source is fitted into a destination box. <see cref="Max"/> is the mode the thumbnail
/// and upload-shrink paths already use: aspect ratio is kept, and a smaller image is enlarged
/// until one side hits the box. Callers that must not enlarge check the size first.
/// </summary>
public enum ImageScaleMode
{
    Max = 0,
    Stretch = 1,
    Crop = 2,
}

/// <summary>Resize plus an optional re-encode. A null <see cref="Format"/> keeps the source container.</summary>
public sealed record ImageTransform
{
    public int Width { get; init; }

    public int Height { get; init; }

    public ImageScaleMode Mode { get; init; } = ImageScaleMode.Max;

    public RasterFormat? Format { get; init; }

    public int Quality { get; init; } = 80;

    /// <summary>
    /// Drops EXIF, IPTC, and XMP on the way out. Thumbnails turn this on so a shared preview
    /// cannot leak GPS coordinates that were stored on the original.
    /// </summary>
    public bool StripMetadata { get; init; }

    public long MaxPixels { get; init; } = ImageFormats.DefaultMaxPixels;
}

public sealed record ImageEncode(RasterFormat Format, int Quality = 80, bool StripMetadata = true);

/// <summary>
/// Crops a rectangle out of the source, then stretches that rectangle to
/// <see cref="DestinationWidth"/> by <see cref="DestinationHeight"/>. elFinder's editor scales
/// the crop to the requested output size rather than returning the raw crop.
/// </summary>
public sealed record ImageCropBox(
    int X,
    int Y,
    int Width,
    int Height,
    int DestinationWidth,
    int DestinationHeight);

/// <summary>
/// In-place shrink used when an upload is larger than the configured box. A non-positive side
/// means that side is unlimited. Decoding stops above <see cref="MaxPixels"/> and the file is
/// left unchanged — a small file with enormous dimensions is the decompression-bomb case.
/// </summary>
public sealed record ImageShrinkOptions
{
    public int MaxWidth { get; init; }

    public int MaxHeight { get; init; }

    /// <summary>Encoder quality for JPEG and WebP. Null keeps the encoder default (75).</summary>
    public int? Quality { get; init; }

    public long MaxPixels { get; init; } = ImageFormats.DefaultMaxPixels;
}

/// <summary>The file's pixel count is above the configured decode ceiling.</summary>
public sealed class RasterTooLargeException(string message) : Exception(message);

/// <summary>The bytes are not a raster this processor can decode.</summary>
public sealed class RasterDecodeException(string message, Exception? innerException = null)
    : Exception(message, innerException);
