namespace Heroicsoft.NetForge.Drawing;

/// <summary>
/// Shared raster operations. Apps keep their own storage, caching, and permissions; this type
/// only decodes, transforms, and encodes.
/// </summary>
public interface IImageProcessor
{
    Task<RasterInfo?> TryIdentifyAsync(string path, CancellationToken cancellationToken = default);

    Task<RasterInfo?> TryIdentifyAsync(Stream stream, CancellationToken cancellationToken = default);

    Task<DecodedRaster?> TryDecodeAsync(Stream stream, CancellationToken cancellationToken = default);

    Task<ProcessedImage> ResizeAsync(Stream source, ImageTransform transform, CancellationToken cancellationToken = default);

    Task<ProcessedImage> ResizeAsync(string path, ImageTransform transform, CancellationToken cancellationToken = default);

    Task<ProcessedImage> ResizeAsync(ReadOnlyMemory<byte> source, ImageTransform transform, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rewrites <paramref name="path"/> when either side exceeds the box. Returns false when the
    /// file is already small enough, is not a decodable image, or exceeds the pixel ceiling.
    /// </summary>
    Task<bool> TryShrinkFileAsync(string path, ImageShrinkOptions options, CancellationToken cancellationToken = default);

    Task<ProcessedImage> CropAsync(Stream source, ImageCropBox crop, ImageEncode encode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates clockwise, expanding the canvas so corners are not clipped. A null
    /// <paramref name="encode"/> keeps the source format.
    /// </summary>
    Task<ProcessedImage> RotateAsync(Stream source, float degrees, ImageEncode? encode, CancellationToken cancellationToken = default);

    Task<ProcessedImage> TranscodeAsync(Stream source, ImageEncode encode, CancellationToken cancellationToken = default);

    Task<ProcessedImage> TranscodeAsync(string path, ImageEncode encode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Wraps tightly packed BGRA8 pixels (PDFium's layout) and encodes them. The buffer length
    /// must be <c>width * height * 4</c>.
    /// </summary>
    Task<ProcessedImage> EncodeBgraAsync(
        ReadOnlyMemory<byte> bgra,
        int width,
        int height,
        ImageEncode encode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 64-bit difference hash. The image is stretched to 9×8 grayscale and each bit records
    /// whether a pixel is brighter than the neighbour on its right.
    /// </summary>
    Task<long> ComputeDifferenceHashAsync(string path, CancellationToken cancellationToken = default);

    Task<long> ComputeDifferenceHashAsync(Stream source, CancellationToken cancellationToken = default);

    int HammingDistance(long left, long right);
}
