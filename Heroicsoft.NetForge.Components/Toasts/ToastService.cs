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

    /// <summary>Raised when a toast is added or dismissed, so a host can redraw the list. Also raised when <see cref="Dismiss"/> is called with an unknown id.</summary>
    public event Action? Changed;

    /// <summary>Raised for each new toast, for hosts that render and time out toasts themselves.</summary>
    public event Action<Toast>? Shown;

    /// <summary>Toasts still on screen, oldest first.</summary>
    public IReadOnlyCollection<Toast> Toasts =>
        [.. toasts.Values.OrderBy(toast => toast.CreatedAt)];

    /// <summary>Shows a success toast. It leaves on its own after 6 seconds.</summary>
    /// <param name="message">Text of the toast.</param>
    public void Success(string message) => Show(message, "success");

    /// <summary>Shows an error toast that stays for 12 seconds.</summary>
    /// <param name="message">Text of the toast.</param>
    public void Error(string message) => Show(message, "danger", TimeSpan.FromSeconds(12));

    /// <summary>Shows a warning toast that stays for 9 seconds.</summary>
    /// <param name="message">Text of the toast.</param>
    public void Warning(string message) => Show(message, "warning", TimeSpan.FromSeconds(9));

    /// <summary>Shows an info toast. It leaves on its own after 6 seconds.</summary>
    /// <param name="message">Text of the toast.</param>
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
    /// anything else renders neutral). A <see langword="null"/> duration keeps the toast for 6 seconds.
    /// </summary>
    /// <param name="message">Text of the toast.</param>
    /// <param name="tone"><c>success</c>, <c>danger</c>, <c>warning</c>, or <c>info</c>. Anything else renders neutral. Defaults to <c>info</c>.</param>
    /// <param name="duration">How long the toast stays. <see langword="null"/>, the default, is 6 seconds.</param>
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

    /// <summary>Removes the toast and cancels its timer. Raises <see cref="Changed"/> even when <paramref name="id"/> is unknown.</summary>
    /// <param name="id">Identifier from <see cref="Toast.Id"/>.</param>
    public void Dismiss(Guid id)
    {
        toasts.TryRemove(id, out _);

        if (timers.TryRemove(id, out var timer))
        {
            timer.Dispose();
        }

        Changed?.Invoke();
    }

    /// <summary>Stops every toast timer and clears the list. Does not raise <see cref="Changed"/>.</summary>
    public void Dispose()
    {
        foreach (var timer in timers.Values)
        {
            timer.Dispose();
        }

        timers.Clear();
        toasts.Clear();
    }

    /// <summary>One toast currently held by <see cref="ToastService"/>.</summary>
    /// <param name="Id">Value passed to <see cref="Dismiss"/>.</param>
    /// <param name="Message">Text shown in the toast.</param>
    /// <param name="Level">Tone: <c>success</c>, <c>danger</c>, <c>warning</c>, <c>info</c>, or another string that renders neutral.</param>
    /// <param name="CreatedAt">UTC time the toast was shown. <see cref="Toasts"/> is ordered by this.</param>
    public sealed record Toast(Guid Id, string Message, string Level, DateTime CreatedAt)
    {
        /// <summary>Alias of <see cref="Level"/>, the name the app-local toast services used.</summary>
        public string Tone => Level;
    }
}