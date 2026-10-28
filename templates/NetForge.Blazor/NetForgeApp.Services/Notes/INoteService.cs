namespace NetForgeApp.Services.Notes;

public interface INoteService
{
    Task<Result<IReadOnlyList<NoteDto>>> ListAsync(CancellationToken cancellationToken = default);

    Task<Result<NoteDto>> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<NoteDto>> SaveAsync(SaveNoteRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
