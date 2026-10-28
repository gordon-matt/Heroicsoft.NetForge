using Microsoft.AspNetCore.Components;

namespace NetForgeApp.Components.Pages;

public partial class Home
{
    [Inject]
    private INoteService Notes { get; set; } = default!;

    [Inject]
    private ToastService Toasts { get; set; } = default!;

    private bool loading = true;
    private string? loadError;
    private string title = string.Empty;
    private string body = string.Empty;
    private List<NoteDto> notes = [];

    protected override async Task OnInitializedAsync()
    {
        loading = true;
        var listed = await Notes.ListAsync();

        if (!listed.IsSuccess)
        {
            loadError = listed.GetDisplayMessage();
            loading = false;
            return;
        }

        notes = [.. listed.Value];
        loadError = null;
        loading = false;
    }

    private async Task SaveAsync()
    {
        var saved = await Notes.SaveAsync(new SaveNoteRequest(null, title, body));
        if (!saved.IsSuccess)
        {
            Toasts.ShowFailure(saved);
            return;
        }

        notes.Insert(0, saved.Value);
        title = string.Empty;
        body = string.Empty;
    }

    private async Task DeleteAsync(int id)
    {
        var deleted = await Notes.DeleteAsync(id);
        if (!deleted.IsSuccess)
        {
            Toasts.ShowFailure(deleted);
            return;
        }

        notes.RemoveAll(note => note.Id == id);
    }
}
