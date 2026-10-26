using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Heroicsoft.NetForge.Components.Icons;

/// <summary>
/// An inline SVG icon that takes its colour from the surrounding text. Names come from
/// <see cref="IconSet"/>; Bootstrap Icons names such as <c>bi-house-door</c> are accepted and
/// mapped, and an unknown name draws a plain circle.
/// </summary>
public sealed class Icon : ComponentBase
{
    /// <summary>Icon name, for example <c>search</c>. See <see cref="IconSet.Names"/>.</summary>
    [Parameter, EditorRequired]
    public string Name { get; set; } = string.Empty;

    /// <summary>Width and height in CSS pixels.</summary>
    [Parameter]
    public int Size { get; set; } = 18;

    /// <summary>Extra CSS classes, added after <c>nf-icon</c>.</summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Accessible name. Without it the icon is decorative and hidden from assistive technology,
    /// which is right when it sits next to a text label.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        string icon = IconSet.Normalize(Name);

        builder.OpenElement(0, "svg");

        // Keyed by icon, so a changed name rebuilds the drawing instead of diffing two unrelated
        // sets of shapes.
        builder.SetKey(icon);
        builder.AddAttribute(1, "class", string.IsNullOrWhiteSpace(Class) ? "nf-icon" : $"nf-icon {Class}");
        builder.AddAttribute(2, "width", Size);
        builder.AddAttribute(3, "height", Size);
        builder.AddAttribute(4, "viewBox", "0 0 24 24");
        builder.AddAttribute(5, "fill", "none");
        builder.AddAttribute(6, "stroke", "currentColor");
        builder.AddAttribute(7, "stroke-width", "1.8");
        builder.AddAttribute(8, "stroke-linecap", "round");
        builder.AddAttribute(9, "stroke-linejoin", "round");
        builder.AddAttribute(10, "aria-hidden", string.IsNullOrEmpty(Title) ? "true" : null);
        builder.AddAttribute(11, "role", string.IsNullOrEmpty(Title) ? null : "img");
        builder.AddMultipleAttributes(12, AdditionalAttributes);

        if (!string.IsNullOrEmpty(Title))
        {
            builder.OpenElement(13, "title");
            builder.AddContent(14, Title);
            builder.CloseElement();
        }

        foreach (var shape in IconSet.GetShapes(icon))
        {
            builder.OpenElement(15, shape.Tag);
            builder.AddMultipleAttributes(16, shape.Attributes);
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}