namespace NetForgeApp.Services.Notes;

public sealed class NoteService(
    ILogger<NoteService> logger,
    IUserContextService userContext,
    IRepository<Note> notes,
    TimeProvider time) : INoteService
{
    public async Task<Result<IReadOnlyList<NoteDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (ResolveOwner() is not string ownerUserId)
        {
            return Result<IReadOnlyList<NoteDto>>.Forbidden();
        }

        var page = await notes.FindAsync(new SearchOptions<Note>
        {
            Query = note => note.OwnerUserId == ownerUserId,
            OrderBy = query => query.OrderByDescending(note => note.UpdatedAt).ThenByDescending(note => note.Id),
            CancellationToken = cancellationToken,
        });

        return Result<IReadOnlyList<NoteDto>>.Success(page.Select(ToDto).ToList());
    }

    public async Task<Result<NoteDto>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        if (ResolveOwner() is not string ownerUserId)
        {
            return Result<NoteDto>.Forbidden();
        }

        var found = await FindOwnedAsync(id, ownerUserId, cancellationToken);
        return found.IsSuccessStatus()
            ? Result<NoteDto>.Success(ToDto(found.Value))
            : found.To<NoteDto>();
    }

    public async Task<Result<NoteDto>> SaveAsync(SaveNoteRequest request, CancellationToken cancellationToken = default)
    {
        if (ResolveOwner() is not string ownerUserId)
        {
            return Result<NoteDto>.Forbidden();
        }

        var validated = Validate(request);
        if (!validated.IsSuccess)
        {
            return validated.To<NoteDto>();
        }

        var (title, body) = validated.Value;
        var now = time.GetUtcNow().UtcDateTime;

        if (request.Id is null or 0)
        {
            var saved = await notes.InsertAsync(new Note
            {
                Title = title,
                Body = body,
                OwnerUserId = ownerUserId,
                CreatedAt = now,
                UpdatedAt = now,
            }, ContextOptions.ForCancellationToken(cancellationToken));

            logger.LogInformation("User {UserId} created note {NoteId}", ownerUserId, saved.Id);
            return Result<NoteDto>.Success(ToDto(saved));
        }

        var found = await FindOwnedAsync(request.Id.Value, ownerUserId, cancellationToken);
        if (!found.IsSuccessStatus())
        {
            return found.To<NoteDto>();
        }

        var note = found.Value;
        note.Title = title;
        note.Body = body;
        note.UpdatedAt = now;

        var updated = await notes.UpdateAsync(note, ContextOptions.ForCancellationToken(cancellationToken));
        logger.LogInformation("User {UserId} updated note {NoteId}", ownerUserId, updated.Id);
        return Result<NoteDto>.Success(ToDto(updated));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (ResolveOwner() is not string ownerUserId)
        {
            return Result.Forbidden();
        }

        var found = await FindOwnedAsync(id, ownerUserId, cancellationToken);
        if (!found.IsSuccessStatus())
        {
            return found.ToVoid();
        }

        await notes.DeleteAsync(found.Value, ContextOptions.ForCancellationToken(cancellationToken));
        logger.LogInformation("User {UserId} deleted note {NoteId}", ownerUserId, id);
        return Result.Success();
    }

    private async Task<Result<Note>> FindOwnedAsync(int id, string ownerUserId, CancellationToken cancellationToken)
    {
        var note = await notes.FindOneAsync(new SearchOptions<Note>
        {
            Query = item => item.Id == id && item.OwnerUserId == ownerUserId,
            CancellationToken = cancellationToken,
        });

        return note is null
            ? Result<Note>.NotFound($"Note {id} was not found.")
            : Result<Note>.Success(note);
    }

    private static Result<(string Title, string Body)> Validate(SaveNoteRequest request)
    {
        if (request is null)
        {
            return Result<(string, string)>.Invalid(new ValidationError(nameof(request), "A note is required."));
        }

        var errors = new List<ValidationError>();
        string title = request.Title?.Trim() ?? string.Empty;
        string body = request.Body ?? string.Empty;

        if (title.Length == 0)
        {
            errors.Add(new ValidationError(nameof(request.Title), "Give the note a title."));
        }
        else if (title.Length > Constants.MaxLengths.Title)
        {
            errors.Add(new ValidationError(
                nameof(request.Title),
                $"Titles can be at most {Constants.MaxLengths.Title} characters."));
        }

        if (errors.Count > 0)
        {
            return Result<(string, string)>.Invalid(errors);
        }

        return Result<(string, string)>.Success((title, body));
    }

    private static NoteDto ToDto(Note note) =>
        new(note.Id, note.Title, note.Body, note.CreatedAt, note.UpdatedAt);

    private string? ResolveOwner() => userContext.GetCurrentUserId();
}
