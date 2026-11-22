namespace Heroicsoft.NetForge.Components.Toasts;

/// <summary>
/// Optional sounds played when a toast is shown, modelled on egalink-toasty's <c>enableSounds</c> and
/// <c>sounds</c> options. Off by default: hosts opt in with <c>ConfigureNetForgeToastSounds</c>.
/// Shared by the nf and Bootstrap toasters.
/// </summary>
public sealed class ForgeToastSoundOptions
{
    /// <summary>Static-asset base for the bundled sounds (<c>{tone}/1.mp3</c> to <c>3.mp3</c>).</summary>
    public const string SoundsBasePath = "_content/Heroicsoft.NetForge.Components/sounds";

    /// <summary>Bundled info sound used by <see cref="Info"/> unless the host replaces it.</summary>
    public static readonly string InfoSound1 = $"{SoundsBasePath}/info/1.mp3";

    /// <summary>Alternative bundled info sound. Assign it to <see cref="Info"/> to use it.</summary>
    public static readonly string InfoSound2 = $"{SoundsBasePath}/info/2.mp3";

    /// <summary>Alternative bundled info sound. Assign it to <see cref="Info"/> to use it.</summary>
    public static readonly string InfoSound3 = $"{SoundsBasePath}/info/3.mp3";

    /// <summary>Bundled success sound used by <see cref="Success"/> unless the host replaces it.</summary>
    public static readonly string SuccessSound1 = $"{SoundsBasePath}/success/1.mp3";

    /// <summary>Alternative bundled success sound. Assign it to <see cref="Success"/> to use it.</summary>
    public static readonly string SuccessSound2 = $"{SoundsBasePath}/success/2.mp3";

    /// <summary>Alternative bundled success sound. Assign it to <see cref="Success"/> to use it.</summary>
    public static readonly string SuccessSound3 = $"{SoundsBasePath}/success/3.mp3";

    /// <summary>Bundled warning sound used by <see cref="Warning"/> unless the host replaces it.</summary>
    public static readonly string WarningSound1 = $"{SoundsBasePath}/warning/1.mp3";

    /// <summary>Alternative bundled warning sound. Assign it to <see cref="Warning"/> to use it.</summary>
    public static readonly string WarningSound2 = $"{SoundsBasePath}/warning/2.mp3";

    /// <summary>Alternative bundled warning sound. Assign it to <see cref="Warning"/> to use it.</summary>
    public static readonly string WarningSound3 = $"{SoundsBasePath}/warning/3.mp3";

    /// <summary>Bundled error sound used by <see cref="Error"/> unless the host replaces it.</summary>
    public static readonly string ErrorSound1 = $"{SoundsBasePath}/error/1.mp3";

    /// <summary>Alternative bundled error sound. Assign it to <see cref="Error"/> to use it.</summary>
    public static readonly string ErrorSound2 = $"{SoundsBasePath}/error/2.mp3";

    /// <summary>Alternative bundled error sound. Assign it to <see cref="Error"/> to use it.</summary>
    public static readonly string ErrorSound3 = $"{SoundsBasePath}/error/3.mp3";

    /// <summary>Plays a sound when a toast is shown. False unless the host turns it on.</summary>
    public bool EnableSounds { get; set; }

    /// <summary>Sound for <c>info</c> (and unknown) toasts. Null or empty means silent for this tone.</summary>
    public string? Info { get; set; } = InfoSound1;

    /// <summary>Sound for <c>success</c> toasts.</summary>
    public string? Success { get; set; } = SuccessSound1;

    /// <summary>Sound for <c>warning</c> toasts.</summary>
    public string? Warning { get; set; } = WarningSound1;

    /// <summary>Sound for <c>danger</c> (error) toasts.</summary>
    public string? Error { get; set; } = ErrorSound1;

    /// <summary>The sound for a toast level, or <see langword="null"/> when sounds are off or none is set for it.</summary>
    public string? SoundFor(string level)
    {
        if (!EnableSounds)
        {
            return null;
        }

        string? path = level switch
        {
            "success" => Success,
            "warning" => Warning,
            "danger" => Error,
            _ => Info,
        };

        return string.IsNullOrWhiteSpace(path) ? null : path;
    }
}