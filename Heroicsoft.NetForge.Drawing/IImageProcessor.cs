namespace Heroicsoft.NetForge.Drawing;

/// <summary>
/// Shared raster operations. Apps keep their own storage, caching, and permissions; this type
/// only decodes, transforms, and encodes.
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Reads the dimensions and container of the file at <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The image file to inspect.</param>
    /// <param name="cancellationToken">Token that cancels reading the file.</param>
    /// <returns>The dimensions and format, or <see langword="null"/> when the file is not a decodable image.</returns>
    /// <remarks>This call does not apply the pixel ceiling. A missing file throws <see cref="FileNotFoundException"/>.</remarks>
    Task<RasterInfo?> TryIdentifyAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the dimensions and container of <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The image bytes. A seekable stream is read from the start.</param>
    /// <param name="cancellationToken">Token that cancels reading the stream.</param>
    /// <returns>The dimensions and format, or <see langword="null"/> when the bytes are not a decodable image.</returns>
    /// <remarks>This call does not apply the pixel ceiling.</remarks>
    Task<RasterInfo?> TryIdentifyAsync(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decodes <paramref name="stream"/> to tightly packed RGBA.
    /// </summary>
    /// <param name="stream">The image bytes. A seekable stream is read from the start.</param>
    /// <param name="cancellationToken">Token that cancels reading the stream.</param>
    /// <returns>The decoded frame, or <see langword="null"/> when the bytes cannot be decoded.</returns>
    /// <exception cref="RasterTooLargeException">The image has more pixels than <see cref="ImageFormats.DefaultMaxPixels"/>.</exception>
    Task<DecodedRaster?> TryDecodeAsync(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resizes <paramref name="source"/> into the box described by <paramref name="transform"/> and encodes it.
    /// </summary>
    /// <param name="source">The image bytes. A seekable stream is read from the start.</param>
    /// <param name="transform">The destination box, fit mode, format, and quality.</param>
    /// <param name="cancellationToken">Token that cancels reading the stream.</param>
    /// <returns>The encoded image and the size that was written.</returns>
    /// <exception cref="RasterTooLargeException">The source has more pixels than <see cref="ImageTransform.MaxPixels"/>.</exception>
    /// <exception cref="RasterDecodeException">The source cannot be decoded, resized, or encoded.</exception>
    Task<ProcessedImage> ResizeAsync(Stream source, ImageTransform transform, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resizes the file at <paramref name="path"/> into the box described by <paramref name="transform"/> and encodes it.
    /// </summary>
    /// <param name="path">The image file to resize. The file is not modified.</param>
    /// <param name="transform">The destination box, fit mode, format, and quality.</param>
    /// <param name="cancellationToken">Token that cancels reading the file.</param>
    /// <returns>The encoded image and the size that was written.</returns>
    Task<ProcessedImage> ResizeAsync(string path, ImageTransform transform, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resizes <paramref name="source"/> into the box described by <paramref name="transform"/> and encodes it.
    /// </summary>
    /// <param name="source">The image bytes.</param>
    /// <param name="transform">The destination box, fit mode, format, and quality.</param>
    /// <param name="cancellationToken">Token that cancels the operation.</param>
    /// <returns>The encoded image and the size that was written.</returns>
    Task<ProcessedImage> ResizeAsync(ReadOnlyMemory<byte> source, ImageTransform transform, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rewrites <paramref name="path"/> when either side exceeds the box. Returns <see langword="false"/> when the
    /// file is missing, both limits are non-positive, the file is already small enough, is not a decodable image,
    /// exceeds the pixel ceiling, or the replacement cannot be written.
    /// </summary>
    /// <param name="path">The file to shrink in place.</param>
    /// <param name="options">The maximum width, height, quality, and pixel ceiling.</param>
    /// <param name="cancellationToken">Token that cancels the operation. Cancellation is not reported as <see langword="false"/>.</param>
    /// <returns><see langword="true"/> when the file was replaced with a smaller image.</returns>
    /// <remarks>
    /// A non-positive side is unlimited. The fit keeps the aspect ratio and does not enlarge.
    /// A null <see cref="ImageShrinkOptions.Quality"/> encodes JPEG and WebP at 75. The container stays the source format.
    /// </remarks>
    Task<bool> TryShrinkFileAsync(string path, ImageShrinkOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cuts <paramref name="crop"/> out of <paramref name="source"/> and stretches that rectangle to the crop's destination size.
    /// </summary>
    /// <param name="source">The image bytes. A seekable stream is read from the start.</param>
    /// <param name="crop">The source rectangle and the output size.</param>
    /// <param name="encode">The container and quality of the result.</param>
    /// <param name="cancellationToken">Token that cancels reading the stream.</param>
    /// <returns>The encoded crop.</returns>
    /// <exception cref="RasterTooLargeException">The source has more pixels than <see cref="ImageFormats.DefaultMaxPixels"/>.</exception>
    /// <exception cref="RasterDecodeException">The source cannot be decoded or the rectangle cannot be cut out.</exception>
    Task<ProcessedImage> CropAsync(Stream source, ImageCropBox crop, ImageEncode encode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates clockwise, expanding the canvas so corners are not clipped. A null
    /// <paramref name="encode"/> keeps the source format and encodes JPEG and WebP at 75.
    /// </summary>
    /// <param name="source">The image bytes. A seekable stream is read from the start.</param>
    /// <param name="degrees">Clockwise degrees. Multiples of 90 use a pixel copy; other angles leave the new corners transparent.</param>
    /// <param name="encode">The container and quality. <see langword="null"/>, or <see cref="RasterFormat.Unknown"/>, keeps the source format.</param>
    /// <param name="cancellationToken">Token that cancels reading the stream.</param>
    /// <returns>The encoded rotation.</returns>
    /// <exception cref="RasterTooLargeException">The source has more pixels than <see cref="ImageFormats.DefaultMaxPixels"/>.</exception>
    /// <exception cref="RasterDecodeException">The source cannot be decoded or encoded.</exception>
    Task<ProcessedImage> RotateAsync(Stream source, float degrees, ImageEncode? encode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Encodes <paramref name="source"/> in another container without changing its pixel size.
    /// </summary>
    /// <param name="source">The image bytes. A seekable stream is read from the start.</param>
    /// <param name="encode">The container and quality.</param>
    /// <param name="cancellationToken">Token that cancels reading the stream.</param>
    /// <returns>The encoded image at the source size.</returns>
    /// <exception cref="RasterTooLargeException">The source has more pixels than <see cref="ImageFormats.DefaultMaxPixels"/>.</exception>
    /// <exception cref="RasterDecodeException">The source cannot be decoded or encoded.</exception>
    Task<ProcessedImage> TranscodeAsync(Stream source, ImageEncode encode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Encodes the file at <paramref name="path"/> in another container without changing its pixel size. The file is not modified.
    /// </summary>
    /// <param name="path">The image file to encode.</param>
    /// <param name="encode">The container and quality.</param>
    /// <param name="cancellationToken">Token that cancels reading the file.</param>
    /// <returns>The encoded image at the source size.</returns>
    Task<ProcessedImage> TranscodeAsync(string path, ImageEncode encode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Wraps tightly packed BGRA8 pixels (PDFium's layout) and encodes them. The buffer length
    /// must be <c>width * height * 4</c>.
    /// </summary>
    /// <param name="bgra">The pixel buffer. Bytes past <c>width * height * 4</c> are ignored.</param>
    /// <param name="width">The image width, in pixels.</param>
    /// <param name="height">The image height, in pixels.</param>
    /// <param name="encode">The container and quality.</param>
    /// <param name="cancellationToken">Unused. The encode finishes before the method returns.</param>
    /// <returns>The encoded image.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    /// <exception cref="ArgumentException"><paramref name="bgra"/> is shorter than <c>width * height * 4</c>.</exception>
    /// <exception cref="RasterDecodeException">The pixels cannot be encoded.</exception>
    Task<ProcessedImage> EncodeBgraAsync(
        ReadOnlyMemory<byte> bgra,
        int width,
        int height,
        ImageEncode encode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a 64-bit difference hash of the file at <paramref name="path"/>. The image is stretched to 9×8 grayscale and each bit records
    /// whether a pixel is brighter than the neighbour on its right.
    /// </summary>
    /// <param name="path">The image file to hash.</param>
    /// <param name="cancellationToken">Token that cancels reading the file.</param>
    /// <returns>The 64-bit hash.</returns>
    Task<long> ComputeDifferenceHashAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a 64-bit difference hash of <paramref name="source"/>. The image is stretched to 9×8 grayscale and each bit records
    /// whether a pixel is brighter than the neighbour on its right.
    /// </summary>
    /// <param name="source">The image bytes. A seekable stream is read from the start.</param>
    /// <param name="cancellationToken">Token that cancels reading the stream.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="RasterTooLargeException">The source has more pixels than <see cref="ImageFormats.DefaultMaxPixels"/>.</exception>
    /// <exception cref="RasterDecodeException">The source cannot be decoded.</exception>
    Task<long> ComputeDifferenceHashAsync(Stream source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the bits that differ between two difference hashes.
    /// </summary>
    /// <param name="left">The first hash from <see cref="ComputeDifferenceHashAsync(string, CancellationToken)"/>.</param>
    /// <param name="right">The second hash.</param>
    /// <returns>A value from 0 through 64. Zero means the hashes are equal.</returns>
    int HammingDistance(long left, long right);
}
