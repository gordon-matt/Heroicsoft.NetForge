using System.Collections.Concurrent;
using Ardalis.Result;
using Heroicsoft.NetForge.Results;

namespace Heroicsoft.NetForge.Components.Toasts;

/// <summary>
/// Transient notifications, rendered once by <c>Toaster</c>. Scoped to the circuit.
/// </summary>
public sealed class ToastService : IDisposable
{
    private readonly ConcurrentDictionary<Guid, Toast> toasts = new();
    private readonly ConcurrentDictionary<Guid, Timer> timers = new();

    public event Action? Changed;

    /// <summary>Raised for each new toast, for hosts that render and time out toasts themselves.</summary>
    public event Action<Toast>? Shown;

    public IReadOnlyCollection<Toast> Toasts =>
        [.. toasts.Values.OrderBy(toast => toast.CreatedAt)];

    public void Success(string message) => Show(message, "success");

    public void Error(string message) => Show(message, "danger", TimeSpan.FromSeconds(12));

    public void Warning(string message) => Show(message, "warning", TimeSpan.FromSeconds(9));

    public void Info(string message) => Show(message, "info");

    /// <summary>Surfaces a failed result without every caller writing the same status switch.</summary>
    public void ShowFailure(IResult result, string? notFound = null) =>
        Error(result.GetDisplayMessage(notFound));

    /// <summary>
    /// Reports a batch that may have partly succeeded: one line when everything worked, otherwise
    /// a count plus the first failure, which is nearly always representative of the rest.
    /// </summary>
    /// <param name="succeeded">Items that completed.</param>
    /// <param name="failed">Items that did not.</param>
    /// <param name="verb">Past-tense action, for example <c>Deleted</c>.</param>
    /// <param name="firstError">Message from the first failed item, when there is one.</param>
    public void ShowBulk(int succeeded, int failed, string verb, string? firstError = null)
    {
        if (failed == 0)
        {
            Success($"{verb} {succeeded} {(succeeded == 1 ? "item" : "items")}.");
            return;
        }

        if (succeeded == 0)
        {
            Error($"None of the items were {verb.ToLowerInvariant()}. {firstError}".TrimEnd());
            return;
        }

        Warning($"{verb} {succeeded} of {succeeded + failed} items. {failed} failed: {firstError}".TrimEnd());
    }

    /// <summary>
    /// Shows a toast with a custom tone (<c>success</c>, <c>danger</c>, <c>warning</c>, <c>info</c>;
    /// anything else renders neutral).
    /// </summary>
    public void Show(string message, string tone = "info", TimeSpan? duration = null)
    {
        var toast = new Toast(Guid.NewGuid(), message, tone, DateTime.UtcNow);
        toasts[toast.Id] = toast;

        timers[toast.Id] = new Timer(
            _ => Dismiss(toast.Id),
            null,
            duration ?? TimeSpan.FromSeconds(6),
            Timeout.InfiniteTimeSpan);

        Shown?.Invoke(toast);
        Changed?.Invoke();
    }

    public void Dismiss(Guid id)
    {
        toasts.TryRemove(id, out _);

        if (timers.TryRemove(id, out var timer))
        {
            timer.Dispose();
        }

        Changed?.Invoke();
    }

    public void Dispose()
    {
        foreach (var timer in timers.Values)
        {
            timer.Dispose();
        }

        timers.Clear();
        toasts.Clear();
    }

    public sealed record Toast(Guid Id, string Message, string Level, DateTime CreatedAt)
    {
        /// <summary>Alias of <see cref="Level"/>, the name the app-local toast services used.</summary>
        public string Tone => Level;
    }
}