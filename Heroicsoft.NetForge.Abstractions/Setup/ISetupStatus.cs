namespace Heroicsoft.NetForge.Abstractions.Setup;

/// <summary>
/// Whether the first-run wizard has been finished. The setup gate calls this on every request
/// until it returns true, so the implementation should cache the completed state.
/// </summary>
public interface ISetupStatus
{
    /// <summary>
    /// Determines whether this instance is complete.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true when the condition is met; otherwise, false.</returns>
    Task<bool> IsCompleteAsync(CancellationToken cancellationToken = default);
}