namespace Heroicsoft.NetForge.Drawing;

/// <summary>Raster formats the processor can identify and encode.</summary>
public enum RasterFormat
{
    Unknown = 0,
    Jpeg,
    Png,
    Gif,
    Webp,
    Bmp,
    Tiff,
}

/// <summary>
/// Header and extension mapping that does not decode pixels. Callers that only need a file
/// extension or a content type should not spin up a full decode.
/// </summary>
public static class ImageFormats
{
    public const long DefaultMaxPixels = 100_000_000;

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
    public static RasterFormat Detect(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3
            && header[0] == 0xFF
            && header[1] == 0xD8
            && header[2] == 0xFF)
        {
            return RasterFormat.Jpeg;
        }

        if (header.Length >= 8
            && header[0] == 0x89
            && header[1] == 0x50
            && header[2] == 0x4E
            && header[3] == 0x47
            && header[4] == 0x0D
            && header[5] == 0x0A
            && header[6] == 0x1A
            && header[7] == 0x0A)
        {
            return RasterFormat.Png;
        }

        if (header.Length >= 6
            && header[0] == (byte)'G'
            && header[1] == (byte)'I'
            && header[2] == (byte)'F'
            && header[3] == (byte)'8'
            && (header[4] == (byte)'7' || header[4] == (byte)'9')
            && header[5] == (byte)'a')
        {
            return RasterFormat.Gif;
        }

        if (header.Length >= 12
            && header[0] == (byte)'R'
            && header[1] == (byte)'I'
            && header[2] == (byte)'F'
            && header[3] == (byte)'F'
            && header[8] == (byte)'W'
            && header[9] == (byte)'E'
            && header[10] == (byte)'B'
            && header[11] == (byte)'P')
        {
            return RasterFormat.Webp;
        }

        if (header.Length >= 2 && header[0] == (byte)'B' && header[1] == (byte)'M')
        {
            return RasterFormat.Bmp;
        }

        if (header.Length >= 4
            && ((header[0] == (byte)'I' && header[1] == (byte)'I' && header[2] == 0x2A && header[3] == 0x00)
                || (header[0] == (byte)'M' && header[1] == (byte)'M' && header[2] == 0x00 && header[3] == 0x2A)))
        {
            return RasterFormat.Tiff;
        }

        return RasterFormat.Unknown;
    }
}
