// Formatting helpers for the Markdown editor. The component keeps the returned text;
// raising "change" as well would report the edit twice.

function commit(textarea, value, selectionStart, selectionEnd) {
    textarea.value = value;
    textarea.focus();
    textarea.setSelectionRange(selectionStart, selectionEnd);
    return value;
}

export function wrapSelection(textarea, before, after, placeholder) {
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selected = textarea.value.substring(start, end) || placeholder || "";
    const value = textarea.value.substring(0, start) + before + selected + after + textarea.value.substring(end);
    return commit(textarea, value, start + before.length, start + before.length + selected.length);
}

export function prefixLines(textarea, prefix) {
    const text = textarea.value;
    const lineStart = text.lastIndexOf("\n", textarea.selectionStart - 1) + 1;
    const end = textarea.selectionEnd;
    const block = text.substring(lineStart, end);
    const prefixed = block.split("\n").map(line => prefix + line).join("\n");
    const value = text.substring(0, lineStart) + prefixed + text.substring(end);
    return commit(textarea, value, lineStart, lineStart + prefixed.length);
}

export function insertText(textarea, insert) {
    const start = textarea.selectionStart;
    const value = textarea.value.substring(0, start) + insert + textarea.value.substring(textarea.selectionEnd);
    return commit(textarea, value, start + insert.length, start + insert.length);
}
