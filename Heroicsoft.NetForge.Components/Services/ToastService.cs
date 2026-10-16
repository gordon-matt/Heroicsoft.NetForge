using System.Collections.Concurrent;
using Ardalis.Result;
using Heroicsoft.NetForge.Results;

namespace Heroicsoft.NetForge.Components.Services;

/// <summary>
/// Transient notifications, rendered once by <c>Toaster</c>. Scoped to the circuit.
/// </summary>
public sealed class ToastService : IDisposable
{
    private readonly ConcurrentDictionary<Guid, Toast> toasts = new();
    private readonly ConcurrentDictionary<Guid, Timer> timers = new();

    public event Action? Changed;

    public IReadOnlyCollection<Toast> Toasts =>
        [.. toasts.Values.OrderBy(toast => toast.CreatedAt)];

    public void Success(string message) => Show(message, "success");

    public void Error(string message) => Show(message, "danger", TimeSpan.FromSeconds(12));

    public void Warning(string message) => Show(message, "warning", TimeSpan.FromSeconds(9));

    public void Info(string message) => Show(message, "info");

    /// <summary>Surfaces a failed result without every caller writing the same status switch.</summary>
    public void ShowFailure(IResult result, string? notFound = null) =>
        Error(result.GetDisplayMessage(notFound));

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

    private void Show(string message, string level, TimeSpan? duration = null)
    {
        var toast = new Toast(Guid.NewGuid(), message, level, DateTime.UtcNow);
        toasts[toast.Id] = toast;

        timers[toast.Id] = new Timer(
            _ => Dismiss(toast.Id),
            null,
            duration ?? TimeSpan.FromSeconds(6),
            Timeout.InfiniteTimeSpan);

        Changed?.Invoke();
    }

    public sealed record Toast(Guid Id, string Message, string Level, DateTime CreatedAt);
}