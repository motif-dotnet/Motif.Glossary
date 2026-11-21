namespace Motif.Glossary;

/// <summary>
/// A term's sketch id, with the element or port it points to when there's a
/// sketch file.
/// </summary>
public sealed record SketchRef(string Id, SketchItem? Item);

/// <summary>
/// A See also link, with the term it points to if it resolves.
/// </summary>
public sealed record SeeAlsoRef(SeeAlsoLink Link, Term? Term);

/// <summary>
/// One term as the review shows it: the term, its notes, and its links
/// resolved.
/// </summary>
public sealed record TermReview(
    Term Term,
    IReadOnlyList<FormatNote> FormatNotes,
    IReadOnlyList<SketchNote> SketchNotes,
    IReadOnlyList<SketchRef> Sketch,
    IReadOnlyList<SeeAlsoRef> SeeAlso)
{
    public int NoteCount => FormatNotes.Count + SketchNotes.Count;
}
