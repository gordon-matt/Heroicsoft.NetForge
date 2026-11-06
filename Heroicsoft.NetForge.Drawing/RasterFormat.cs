namespace Heroicsoft.NetForge.Drawing;

/// <summary>Raster formats the processor can identify and encode.</summary>
public enum RasterFormat
{
    /// <summary>
    /// The unknown value.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// The jpeg value.
    /// </summary>
    Jpeg,
    /// <summary>
    /// The png value.
    /// </summary>
    Png,
    /// <summary>
    /// The gif value.
    /// </summary>
    Gif,
    /// <summary>
    /// The webp value.
    /// </summary>
    Webp,
    /// <summary>
    /// The bmp value.
    /// </summary>
    Bmp,
    /// <summary>
    /// The tiff value.
    /// </summary>
    Tiff,
}

/// <summary>
/// Header and extension mapping that does not decode pixels. Callers that only need a file
/// extension or a content type should not spin up a full decode.
/// </summary>
public static class ImageFormats
{
    /// <summary>
    /// The default max pixels constant.
    /// </summary>
    public const long DefaultMaxPixels = 100_000_000;

    /// <summary>
    /// Creates a value from the extension.
    /// </summary>
    /// <param name="extension">The extension.</param>
    /// <returns>The extension.</returns>
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
    /// Converts the value to the extension.
    /// </summary>
    /// <param name="format">The format.</param>
    /// <returns>The extension.</returns>
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
    /// Converts the value to the content type.
    /// </summary>
    /// <param name="format">The format.</param>
    /// <returns>The content type.</returns>
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