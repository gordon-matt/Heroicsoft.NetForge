namespace NetForgeApp.Models;

public sealed record NoteDto(
    int Id,
    string Title,
    string Body,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record SaveNoteRequest(
    int? Id,
    string? Title,
    string? Body);
