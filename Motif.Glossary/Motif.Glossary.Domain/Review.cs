namespace Motif.Glossary;

/// <summary>A term's sketch id, with the element or port it points to when there's a sketch file.</summary>
public sealed record SketchRef(string Id, SketchItem? Item);

/// <summary>A See also link, with the term it points to if it resolves.</summary>
public sealed record SeeAlsoRef(SeeAlsoLink Link, Term? Term);

/// <summary>One term as the review shows it: the term, its notes, and its links resolved.</summary>
public sealed record TermReview(
    Term Term,
    IReadOnlyList<FormatNote> FormatNotes,
    IReadOnlyList<SketchNote> SketchNotes,
    IReadOnlyList<SketchRef> Sketch,
    IReadOnlyList<SeeAlsoRef> SeeAlso)
{
    public int NoteCount => FormatNotes.Count + SketchNotes.Count;
}

/// <summary>The terms of one kind. <see cref="Kind"/> is null for terms with no kind out of the seven.</summary>
public sealed record KindGroup(Kind? Kind, IReadOnlyList<TermReview> Terms)
{
    public string Name => Kind?.GroupName() ?? "No kind";
}

/// <summary>
/// The main view: every term at a glance, grouped by kind, with its notes. A term with two kinds is in both
/// groups. The sketch file is optional; without one, sketch ids stay plain ids and there are no sketch notes.
/// </summary>
public sealed record Review(
    Glossary Glossary,
    SketchFile? SketchFile,
    IReadOnlyList<TermReview> Terms,
    IReadOnlyList<KindGroup> Groups,
    IReadOnlyList<SketchNote> WithoutTerms)
{
    public int ProposedCount => Terms.Count(t => t.Term.IsProposed);
    public int NoteCount => Terms.Sum(t => t.NoteCount) + WithoutTerms.Count;

    public static Review Of(Glossary glossary, SketchFile? sketch = null)
    {
        var formatNotes = FormatNotes.For(glossary);
        var terms = glossary.Terms.Select(t => new TermReview(
            t,
            formatNotes.TryGetValue(t, out var notes) ? notes : [],
            sketch is null ? [] : SketchNotes.For(t, sketch),
            t.SketchIds.Select(id => new SketchRef(id, sketch?.Find(id))).ToList(),
            t.SeeAlso.Select(l => new SeeAlsoRef(l, FormatNotes.Resolve(l, glossary))).ToList())).ToList();

        var groups = Kinds.All
            .Select(k => new KindGroup(k, terms.Where(t => t.Term.Kinds.Contains(k)).ToList()))
            .Append(new KindGroup(null, terms.Where(t => t.Term.Kinds.Count == 0).ToList()))
            .Where(g => g.Terms.Count > 0)
            .ToList();

        return new Review(glossary, sketch, terms, groups, sketch is null ? [] : SketchNotes.WithoutTerms(glossary, sketch));
    }

    /// <summary>The groups with only the proposed terms in them, or all of them.</summary>
    public IReadOnlyList<KindGroup> GroupsFor(bool proposedOnly) => !proposedOnly
        ? Groups
        : Groups.Select(g => g with { Terms = g.Terms.Where(t => t.Term.IsProposed).ToList() })
            .Where(g => g.Terms.Count > 0).ToList();
}
