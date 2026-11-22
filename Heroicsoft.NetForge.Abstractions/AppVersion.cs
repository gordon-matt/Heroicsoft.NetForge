using System.Reflection;

namespace Heroicsoft.NetForge.Abstractions;

/// <summary>
/// The running assembly's version, plus the UTC time that assembly was written.
/// Useful on a NAS or Docker host to confirm a deploy actually replaced the bits.
/// </summary>
public static class AppVersion
{
    private static readonly Lazy<string> CurrentLazy = new(Resolve);

    /// <summary>
    /// Version of the entry assembly, or of the assembly that contains <see cref="AppVersion"/> when
    /// there is no entry assembly. Prefers the informational version with the <c>+</c> metadata
    /// removed, then the assembly version in three parts, then <c>0.0.0</c>. When the assembly
    /// file's UTC write time is after the Unix epoch, that time is appended as <c>+yyyyMMdd.HHmm</c>.
    /// The suffix is omitted when the write time is not after the Unix epoch, and an
    /// <see cref="IOException"/> while reading it is ignored.
    /// </summary>
    public static string Current => CurrentLazy.Value;

    private static string Resolve()
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(AppVersion).Assembly;
        string version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
                ?.Split('+', 2)[0]
            ?? assembly.GetName().Version?.ToString(3)
            ?? "0.0.0";

        try
        {
            var built = File.GetLastWriteTimeUtc(assembly.Location);
            if (built > DateTime.UnixEpoch)
            {
                return $"{version}+{built:yyyyMMdd.HHmm}";
            }
        }
        catch (IOException)
        {
        }

        return version;
    }
}