using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Heroicsoft.NetForge.Components.Editors;

/// <summary>
/// A Markdown box with a formatting toolbar and a preview. The preview is whatever the caller
/// renders from the current text, so each product can use its own pipeline.
/// </summary>
public partial class MarkdownEditor
{
    private const string ModulePath = "./_content/Heroicsoft.NetForge.Components/js/markdown-editor.js";

    private ElementReference textarea;
    private bool previewing;
    private Task<IJSObjectReference>? module;

    /// <summary>The Markdown source.</summary>
    [Parameter]
    public string Value { get; set; } = string.Empty;

    /// <summary>Raised when the source changes.</summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>Rows on the text area.</summary>
    [Parameter]
    public int Rows { get; set; } = 14;

    /// <summary>Shown in the empty text area.</summary>
    [Parameter]
    public string Placeholder { get; set; } = "Write in Markdown: ## for headings, **bold**, _italic_, and blank lines between paragraphs.";

    /// <summary>Accessible name for the text area.</summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Markdown";

    /// <summary>Extra toolbar buttons, for example a host's image picker.</summary>
    [Parameter]
    public RenderFragment? ToolbarExtra { get; set; }

    /// <summary>How the current source should look. The argument is <see cref="Value"/>.</summary>
    [Parameter]
    public RenderFragment<string>? Preview { get; set; }

    [Inject]
    private IJSRuntime Js { get; set; } = default!;

    /// <summary>Inserts text at the caret. A file picker calls this after the author chooses an image.</summary>
    /// <param name="text">The text to insert.</param>
    public async Task InsertTextAsync(string text)
    {
        if (previewing || string.IsNullOrEmpty(text))
        {
            return;
        }

        string value = await (await ModuleAsync()).InvokeAsync<string>("insertText", textarea, text);
        await SetValueAsync(value);
    }

    private Task ChangedAsync(ChangeEventArgs args) => SetValueAsync(args.Value?.ToString() ?? string.Empty);

    private async Task SetValueAsync(string value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    private async Task WrapAsync(string before, string after, string placeholder) =>
        await SetValueAsync(await (await ModuleAsync()).InvokeAsync<string>("wrapSelection", textarea, before, after, placeholder));

    private async Task PrefixAsync(string prefix) =>
        await SetValueAsync(await (await ModuleAsync()).InvokeAsync<string>("prefixLines", textarea, prefix));

    private Task<IJSObjectReference> ModuleAsync() =>
        module ??= Js.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask();
}
