using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NetForgeApp.Controllers;

[Authorize]
[Route("api/notes")]
public sealed class NotesController(INoteService notes) : Controller
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await notes.ListAsync(cancellationToken);
        return ToHttpResult(result, () => Ok(result.Value));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var result = await notes.GetAsync(id, cancellationToken);
        return ToHttpResult(result, () => Ok(result.Value));
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaveNoteRequest request, CancellationToken cancellationToken)
    {
        var result = await notes.SaveAsync(request, cancellationToken);
        return ToHttpResult(result, () => Ok(result.Value));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await notes.DeleteAsync(id, cancellationToken);
        return ToHttpResult(result, () => NoContent());
    }

    private IActionResult ToHttpResult<T>(Result<T> result, Func<IActionResult> onSuccess) =>
        Map(result.IsSuccess, result.Status, result.ValidationErrors, onSuccess);

    private IActionResult ToHttpResult(Result result, Func<IActionResult> onSuccess) =>
        Map(result.IsSuccess, result.Status, result.ValidationErrors, onSuccess);

    /// <summary>
    /// A missing row is 404. Validation is 400. Every other failure is 403, matching the
    /// sample host's NotFound-or-forbidden split.
    /// </summary>
    private IActionResult Map(
        bool success,
        ResultStatus status,
        IEnumerable<ValidationError>? errors,
        Func<IActionResult> onSuccess)
    {
        if (success)
        {
            return onSuccess();
        }

        return status switch
        {
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Invalid => BadRequest(errors),
            _ => StatusCode(StatusCodes.Status403Forbidden),
        };
    }
}
