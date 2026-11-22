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
    /// <summary>
    /// Keeps the aspect ratio and scales so the image fits inside the box, enlarging a smaller image until one side meets the box.
    /// A non-positive side uses the source size for that side.
    /// </summary>
    Max = 0,

    /// <summary>Resizes to the box and does not keep the aspect ratio. A non-positive side uses the source size for that side.</summary>
    Stretch = 1,

    /// <summary>
    /// Scales until the image covers the box, then center-crops to the box.
    /// A non-positive side uses the source size for that side.
    /// </summary>
    Crop = 2,
}

/// <summary>Resize plus an optional re-encode. A null <see cref="Format"/> keeps the source container.</summary>
public sealed record ImageTransform
{
    /// <summary>Destination width in pixels. Zero or a negative value uses the source width.</summary>
    public int Width { get; init; }

    /// <summary>Destination height in pixels. Zero or a negative value uses the source height.</summary>
    public int Height { get; init; }

    /// <summary>How the source is fitted into the box. The default is <see cref="ImageScaleMode.Max"/>.</summary>
    public ImageScaleMode Mode { get; init; } = ImageScaleMode.Max;

    /// <summary>
    /// Container to write. <see langword="null"/> or <see cref="RasterFormat.Unknown"/> keeps the source container.
    /// <see cref="RasterFormat.Tiff"/> writes PNG bytes and still reports TIFF.
    /// </summary>
    public RasterFormat? Format { get; init; }

    /// <summary>Encoder quality for JPEG and WebP. The default is 80, and encoding clamps the value to 1 through 100.</summary>
    public int Quality { get; init; } = 80;

    /// <summary>
    /// Stored for callers. The processor does not read it. Encoded output is built from pixels,
    /// so source EXIF, IPTC, and XMP are omitted whether this is <see langword="true"/> or <see langword="false"/>.
    /// </summary>
    public bool StripMetadata { get; init; }

    /// <summary>
    /// Largest source, in pixels, that <see cref="IImageProcessor.ResizeAsync(Stream, ImageTransform, CancellationToken)"/> will decode.
    /// A larger source throws <see cref="RasterTooLargeException"/>. The default is <see cref="ImageFormats.DefaultMaxPixels"/>.
    /// </summary>
    public long MaxPixels { get; init; } = ImageFormats.DefaultMaxPixels;
}

/// <summary>Container and quality for a crop, rotation, or transcode.</summary>
/// <param name="Format">
/// Container to write. <see cref="RasterFormat.Unknown"/> writes a PNG.
/// <see cref="RasterFormat.Tiff"/> writes PNG bytes and the result still reports TIFF.
/// </param>
/// <param name="Quality">Encoder quality for JPEG and WebP. The default is 80, and encoding clamps the value to 1 through 100.</param>
/// <param name="StripMetadata">
/// Stored for callers and not read. Encoded output is built from pixels, so source metadata is omitted either way.
/// The default is <see langword="true"/>.
/// </param>
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
    /// <summary>Largest width to keep. Zero or a negative value leaves the width unlimited.</summary>
    public int MaxWidth { get; init; }

    /// <summary>Largest height to keep. Zero or a negative value leaves the height unlimited.</summary>
    public int MaxHeight { get; init; }

    /// <summary>Encoder quality for JPEG and WebP. <see langword="null"/> uses 75.</summary>
    public int? Quality { get; init; }

    /// <summary>
    /// Largest file, in pixels, that is still decoded. A larger file is left unchanged.
    /// The default is <see cref="ImageFormats.DefaultMaxPixels"/>.
    /// </summary>
    public long MaxPixels { get; init; } = ImageFormats.DefaultMaxPixels;
}

/// <summary>The file's pixel count is above the configured decode ceiling.</summary>
/// <param name="message">The message that names the image size and the ceiling.</param>
public sealed class RasterTooLargeException(string message) : Exception(message);

/// <summary>The bytes are not a raster this processor can decode, or a resize, crop, or encode failed.</summary>
/// <param name="message">The reason the raster operation failed.</param>
/// <param name="innerException">The exception that caused the failure, when there is one.</param>
public sealed class RasterDecodeException(string message, Exception? innerException = null)
    : Exception(message, innerException);