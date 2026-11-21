namespace Motif.Glossary;

/// <summary>
/// The terms of one kind. <see cref="Kind"/> is null for terms with no kind
/// out of the seven.
/// </summary>
public sealed record KindGroup(Kind? Kind, IReadOnlyList<TermReview> Terms)
{
    public string Name => Kind?.GroupName() ?? "No kind";
}

/// <summary>
/// The main view: every term at a glance, grouped by kind, with its notes. A
/// term with two kinds is in both groups. The sketch file is optional; without
/// one, sketch ids stay plain ids and there are no sketch notes.
/// </summary>
public sealed record Review(
    Glossary Glossary,
    SketchFile? SketchFile,
    IReadOnlyList<TermReview> Terms,
    IReadOnlyList<KindGroup> Groups,
    IReadOnlyList<SketchNote> WithoutTerms)
{
    public int ProposedCount => Terms.Count(t => t.Term.IsProposed);

    public int NoteCount =>
        Terms.Sum(t => t.NoteCount) + WithoutTerms.Count;

    public static Review Of(Glossary glossary, SketchFile? sketch = null)
    {
        var terms = glossary.Terms
            .Select((term, i) => Of(
                term, i > 0 ? glossary.Terms[i - 1] : null, glossary, sketch))
            .ToList();

        var groups = Kinds.All
            .Select(kind => new KindGroup(kind,
                terms.Where(t => t.Term.Kinds.Contains(kind)).ToList()))
            .Append(new KindGroup(null,
                terms.Where(t => t.Term.Kinds.Count == 0).ToList()))
            .Where(g => g.Terms.Count > 0)
            .ToList();

        return new Review(glossary, sketch, terms, groups,
            sketch is null ? [] : SketchNotes.WithoutTerms(glossary, sketch));
    }

    private static TermReview Of(
        Term term, Term? previous, Glossary glossary, SketchFile? sketch) =>
        new(
            term,
            FormatNotes.For(term, previous, glossary),
            sketch is null ? [] : SketchNotes.For(term, sketch),
            term.SketchIds
                .Select(id => new SketchRef(id, sketch?.Find(id)))
                .ToList(),
            term.SeeAlso
                .Select(link => new SeeAlsoRef(link, glossary.Resolve(link)))
                .ToList());

    /// <summary>
    /// The groups with only the proposed terms in them, or all of them.
    /// </summary>
    public IReadOnlyList<KindGroup> GroupsFor(bool proposedOnly) =>
        !proposedOnly
            ? Groups
            : Groups
                .Select(g => g with
                {
                    Terms = g.Terms.Where(t => t.Term.IsProposed).ToList(),
                })
                .Where(g => g.Terms.Count > 0)
                .ToList();
}
