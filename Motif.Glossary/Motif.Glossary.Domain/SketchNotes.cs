namespace Motif.Glossary;

public enum SketchNoteKind
{
    /// <summary>
    /// A term's sketch id that isn't in the sketch file any more.
    /// </summary>
    IdNotInSketch,

    /// <summary>
    /// An element or port in the sketch file that no term has the id of.
    /// </summary>
    NoTerm,
}

/// <summary>
/// A gentle note from comparing the glossary with its sketch file.
/// </summary>
public sealed record SketchNote(
    SketchNoteKind Kind, string Subject, string Text);

public static class SketchNotes
{
    /// <summary>
    /// A term's sketch ids that aren't in the sketch file.
    /// </summary>
    public static IReadOnlyList<SketchNote> For(Term term, SketchFile sketch) =>
        term.SketchIds
            .Where(id => sketch.Find(id) is null)
            .Select(id => new SketchNote(SketchNoteKind.IdNotInSketch, id,
                $"Sketch id {id} isn't in the sketch file any more."))
            .ToList();

    /// <summary>
    /// The elements and ports in the sketch file that no term has the id of.
    /// </summary>
    public static IReadOnlyList<SketchNote> WithoutTerms(
        Glossary glossary, SketchFile sketch)
    {
        var ids = glossary.Terms
            .SelectMany(t => t.SketchIds)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return sketch.Items
            .Where(item => item.ExpectsTerm && !ids.Contains(item.Id))
            .Select(item => new SketchNote(SketchNoteKind.NoTerm, item.Id,
                $"{item.Name} ({item.Type}) has no term."))
            .ToList();
    }
}
