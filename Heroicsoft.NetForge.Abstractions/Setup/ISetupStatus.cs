namespace Heroicsoft.NetForge.Abstractions.Setup;

/// <summary>
/// Whether the first-run wizard has been finished. The setup gate calls this on every request
/// until it returns <see langword="true"/>, so the implementation should cache the completed state.
/// </summary>
public interface ISetupStatus
{
    /// <summary>Returns <see langword="true"/> when the first-run wizard has finished.</summary>
    /// <param name="cancellationToken">Cancels the completion check.</param>
    /// <returns><see langword="true"/> when setup is finished; otherwise <see langword="false"/>.</returns>
    Task<bool> IsCompleteAsync(CancellationToken cancellationToken = default);
}