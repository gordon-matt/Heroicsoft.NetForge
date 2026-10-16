namespace Heroicsoft.NetForge.Abstractions.Setup;

/// <summary>
/// Whether the first-run wizard has been finished. The setup gate calls this on every request
/// until it returns true, so the implementation should cache the completed state.
/// </summary>
public interface ISetupStatus
{
    Task<bool> IsCompleteAsync(CancellationToken cancellationToken = default);
}