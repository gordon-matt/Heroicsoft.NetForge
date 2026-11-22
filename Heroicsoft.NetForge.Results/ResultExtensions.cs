using Ardalis.Result;

namespace Heroicsoft.NetForge.Results;

/// <summary>
/// Converts a failed result between value types while preserving why it failed.
/// </summary>
/// <remarks>
/// Service methods often resolve the current user or a row and return early when that fails.
/// Without this, each one would need its own <see cref="ResultStatus"/> switch, and any status
/// missed in one of them would silently downgrade a 403 or a 404 into a generic 500. Declared
/// on <see cref="IResult"/> so callers only name the destination type: <c>resolved.To&lt;Dto&gt;()</c>.
/// </remarks>
public static class ResultExtensions
{
    /// <summary>
    /// Returns whether <paramref name="result"/> completed without an error status.
    /// </summary>
    /// <param name="result">The result to test.</param>
    /// <returns>
    /// <see langword="true"/> when the status is <see cref="ResultStatus.Ok"/>, <see cref="ResultStatus.Created"/>,
    /// or <see cref="ResultStatus.NoContent"/>.
    /// </returns>
    public static bool IsSuccessStatus(this IResult result) =>
        result.Status is ResultStatus.Ok or ResultStatus.Created or ResultStatus.NoContent;

    /// <summary>
    /// Re-shapes a failed <paramref name="result"/> as a <see cref="Result{TTo}"/>, carrying the
    /// status, error messages and validation errors across. Throws on a success, since there
    /// would be no value to carry.
    /// </summary>
    /// <typeparam name="TTo">The value type of the returned result. No value is produced.</typeparam>
    /// <param name="result">The failed result to copy.</param>
    /// <returns>A failed result of <typeparamref name="TTo"/> with the same status and messages.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="result"/> is a success status.</exception>
    /// <remarks>
    /// <see cref="ResultStatus.Invalid"/> copies validation errors. Every other recognized failure copies
    /// <see cref="IResult.Errors"/>. An unrecognized status becomes <see cref="ResultStatus.Error"/>.
    /// </remarks>
    public static Result<TTo> To<TTo>(this IResult result)
    {
        if (result.IsSuccessStatus())
        {
            throw new InvalidOperationException(
                $"{nameof(To)} propagates failures; a successful result cannot be converted to " +
                $"Result<{typeof(TTo).Name}> without a projection.");
        }

        string[] errors = result.Errors?.ToArray() ?? [];

        return result.Status switch
        {
            ResultStatus.NotFound => Result<TTo>.NotFound(errors),
            ResultStatus.Forbidden => Result<TTo>.Forbidden(errors),
            ResultStatus.Unauthorized => Result<TTo>.Unauthorized(errors),
            ResultStatus.Invalid => Result<TTo>.Invalid([.. result.ValidationErrors ?? []]),
            ResultStatus.Conflict => Result<TTo>.Conflict(errors),
            ResultStatus.Unavailable => Result<TTo>.Unavailable(errors),
            ResultStatus.CriticalError => Result<TTo>.CriticalError(errors),
            _ => Result<TTo>.Error(new ErrorList(errors)),
        };
    }

    /// <summary>
    /// Copies <paramref name="result"/> to a non-generic <see cref="Result"/>.
    /// A success status becomes <see cref="Result.Success()"/>. A failure keeps its status and messages.
    /// </summary>
    /// <param name="result">The result to copy.</param>
    /// <returns>A non-generic result with the same outcome.</returns>
    /// <remarks>
    /// <see cref="ResultStatus.Invalid"/> copies validation errors. Every other recognized failure copies
    /// <see cref="IResult.Errors"/>. An unrecognized status becomes <see cref="ResultStatus.Error"/>.
    /// </remarks>
    public static Result ToVoid(this IResult result)
    {
        if (result.IsSuccessStatus())
        {
            return Result.Success();
        }

        string[] errors = result.Errors?.ToArray() ?? [];

        return result.Status switch
        {
            ResultStatus.NotFound => Result.NotFound(errors),
            ResultStatus.Forbidden => Result.Forbidden(errors),
            ResultStatus.Unauthorized => Result.Unauthorized(errors),
            ResultStatus.Invalid => Result.Invalid([.. result.ValidationErrors ?? []]),
            ResultStatus.Conflict => Result.Conflict(errors),
            ResultStatus.Unavailable => Result.Unavailable(errors),
            ResultStatus.CriticalError => Result.CriticalError(errors),
            _ => Result.Error(new ErrorList(errors)),
        };
    }

    /// <summary>
    /// Returns the message to surface to a user, falling back to a status-appropriate default so the UI
    /// never renders an empty alert. Pass <paramref name="notFound"/> when the generic wording
    /// would be less clear than naming the thing that disappeared.
    /// </summary>
    /// <param name="result">The result whose message is shown.</param>
    /// <param name="notFound">Text used for <see cref="ResultStatus.NotFound"/> when the result has no message of its own.</param>
    /// <returns>The first error, otherwise the first validation message, otherwise a sentence for the status.</returns>
    /// <remarks>
    /// The status sentences are: not found uses <paramref name="notFound"/> or "That item no longer exists.";
    /// forbidden, unauthorized, conflict, invalid, and unavailable each have their own sentence.
    /// Every other status, including success, uses "Something went wrong. Please try again."
    /// </remarks>
    public static string GetDisplayMessage(this IResult result, string? notFound = null)
    {
        string? first = result.Errors?.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(first))
        {
            return first;
        }

        var validation = result.ValidationErrors?.FirstOrDefault();
        return validation is not null && !string.IsNullOrWhiteSpace(validation.ErrorMessage)
            ? validation.ErrorMessage
            : result.Status switch
            {
                ResultStatus.NotFound => notFound ?? "That item no longer exists.",
                ResultStatus.Forbidden => "You do not have permission to do that.",
                ResultStatus.Unauthorized => "Please sign in and try again.",
                ResultStatus.Conflict => "That conflicts with something that already exists.",
                ResultStatus.Invalid => "Some of the values supplied were not valid.",
                ResultStatus.Unavailable => "That service is temporarily unavailable.",
                _ => "Something went wrong. Please try again.",
            };
    }
}