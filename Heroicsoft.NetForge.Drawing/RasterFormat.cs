namespace Heroicsoft.NetForge.Drawing;

/// <summary>Raster formats the processor can identify and encode.</summary>
public enum RasterFormat
{
    /// <summary>The container was not recognized. Encoding this value writes a PNG.</summary>
    Unknown = 0,

    /// <summary>A JPEG image.</summary>
    Jpeg,

    /// <summary>A PNG image.</summary>
    Png,

    /// <summary>A GIF image.</summary>
    Gif,

    /// <summary>A WebP image.</summary>
    Webp,

    /// <summary>A BMP image.</summary>
    Bmp,

    /// <summary>A TIFF image. Identifying a TIFF works. Encoding this value writes PNG bytes and still reports TIFF.</summary>
    Tiff,
}

/// <summary>
/// Header and extension mapping that does not decode pixels. Callers that only need a file
/// extension or a content type should not spin up a full decode.
/// </summary>
public static class ImageFormats
{
    /// <summary>The decode ceiling used when a call does not pass its own. One hundred million pixels.</summary>
    public const long DefaultMaxPixels = 100_000_000;

    /// <summary>
    /// Maps a file extension to a raster format. The leading dot is optional and case is ignored.
    /// </summary>
    /// <param name="extension">An extension such as <c>.jpg</c> or <c>png</c>. <see langword="null"/> is unknown.</param>
    /// <returns>The matching format, or <see cref="RasterFormat.Unknown"/>.</returns>
    public static RasterFormat FromExtension(string? extension)
    {
        string normalized = (extension ?? string.Empty).Trim().ToLowerInvariant();

        return normalized switch
        {
            ".jpg" or ".jpeg" or "jpg" or "jpeg" => RasterFormat.Jpeg,
            ".png" or "png" => RasterFormat.Png,
            ".gif" or "gif" => RasterFormat.Gif,
            ".webp" or "webp" => RasterFormat.Webp,
            ".bmp" or "bmp" => RasterFormat.Bmp,
            ".tif" or ".tiff" or "tif" or "tiff" => RasterFormat.Tiff,
            _ => RasterFormat.Unknown,
        };
    }

    /// <summary>
    /// Returns the file extension for <paramref name="format"/>, including the leading dot.
    /// </summary>
    /// <param name="format">The raster format.</param>
    /// <returns><c>.jpg</c>, <c>.png</c>, <c>.gif</c>, <c>.webp</c>, <c>.bmp</c>, <c>.tif</c>, or <c>.img</c> for <see cref="RasterFormat.Unknown"/>.</returns>
    public static string ToExtension(RasterFormat format) => format switch
    {
        RasterFormat.Jpeg => ".jpg",
        RasterFormat.Png => ".png",
        RasterFormat.Gif => ".gif",
        RasterFormat.Webp => ".webp",
        RasterFormat.Bmp => ".bmp",
        RasterFormat.Tiff => ".tif",
        _ => ".img",
    };

    /// <summary>
    /// Returns the MIME type for <paramref name="format"/>.
    /// </summary>
    /// <param name="format">The raster format.</param>
    /// <returns>An image media type, or <c>application/octet-stream</c> for <see cref="RasterFormat.Unknown"/>.</returns>
    public static string ToContentType(RasterFormat format) => format switch
    {
        RasterFormat.Jpeg => "image/jpeg",
        RasterFormat.Png => "image/png",
        RasterFormat.Gif => "image/gif",
        RasterFormat.Webp => "image/webp",
        RasterFormat.Bmp => "image/bmp",
        RasterFormat.Tiff => "image/tiff",
        _ => "application/octet-stream",
    };

    /// <summary>
    /// Reads the leading magic bytes only. A matching header is not proof the rest of the file
    /// decodes; use <see cref="IImageProcessor.TryIdentifyAsync(Stream, CancellationToken)"/> for that.
    /// </summary>
    public static RasterFormat Detect(ReadOnlySpan<byte> header) => header.Length >= 3
            && header[0] == 0xFF
            && header[1] == 0xD8
            && header[2] == 0xFF
            ? RasterFormat.Jpeg
            : header.Length >= 8
            && header[0] == 0x89
            && header[1] == 0x50
            && header[2] == 0x4E
            && header[3] == 0x47
            && header[4] == 0x0D
            && header[5] == 0x0A
            && header[6] == 0x1A
            && header[7] == 0x0A
            ? RasterFormat.Png
            : header.Length >= 6
            && header[0] == (byte)'G'
            && header[1] == (byte)'I'
            && header[2] == (byte)'F'
            && header[3] == (byte)'8'
            && (header[4] == (byte)'7' || header[4] == (byte)'9')
            && header[5] == (byte)'a'
            ? RasterFormat.Gif
            : header.Length >= 12
            && header[0] == (byte)'R'
            && header[1] == (byte)'I'
            && header[2] == (byte)'F'
            && header[3] == (byte)'F'
            && header[8] == (byte)'W'
            && header[9] == (byte)'E'
            && header[10] == (byte)'B'
            && header[11] == (byte)'P'
            ? RasterFormat.Webp
            : header.Length >= 2 && header[0] == (byte)'B' && header[1] == (byte)'M'
            ? RasterFormat.Bmp
            : header.Length >= 4
            && ((header[0] == (byte)'I' && header[1] == (byte)'I' && header[2] == 0x2A && header[3] == 0x00)
                || (header[0] == (byte)'M' && header[1] == (byte)'M' && header[2] == 0x00 && header[3] == 0x2A))
            ? RasterFormat.Tiff
            : RasterFormat.Unknown;
}