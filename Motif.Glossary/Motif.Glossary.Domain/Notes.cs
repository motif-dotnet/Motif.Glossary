namespace Motif.Glossary;

public enum FormatNoteKind
{
    NoKind,
    UnknownKind,
    TooManyKinds,
    UnresolvedSeeAlso,
    AvoidWordIsTerm,
    OutOfOrder,
}

public enum SketchNoteKind
{
    /// <summary>A term's sketch id that isn't in the sketch file any more.</summary>
    IdNotInSketch,

    /// <summary>An element or port in the sketch file that no term has the id of.</summary>
    NoTerm,
}

/// <summary>A gentle note on a term that doesn't follow the glossary format. Notes report; they never block.</summary>
/// <param name="Subject">What the note is about: the kind, link, Avoid word or heading it names.</param>
public sealed record FormatNote(FormatNoteKind Kind, string Subject, string Text);

/// <summary>A gentle note from comparing the glossary with its sketch file.</summary>
public sealed record SketchNote(SketchNoteKind Kind, string Subject, string Text);

public static class FormatNotes
{
    /// <summary>The format notes for every term, keyed by term. Terms with no notes are left out.</summary>
    public static IReadOnlyDictionary<Term, IReadOnlyList<FormatNote>> For(Glossary glossary)
    {
        var notes = new Dictionary<Term, IReadOnlyList<FormatNote>>(ReferenceEqualityComparer.Instance);
        Term? previous = null;
        foreach (var term in glossary.Terms)
        {
            var list = For(term, glossary, previous).ToList();
            if (list.Count > 0) notes[term] = list;
            previous = term;
        }
        return notes;
    }

    private static IEnumerable<FormatNote> For(Term term, Glossary glossary, Term? previous)
    {
        if (term.KindTexts.Count == 0)
            yield return new(FormatNoteKind.NoKind, "", "No Kind yet. Every term has one of the seven kinds.");

        foreach (var text in term.KindTexts.Where(k => !Kinds.TryRead(k, out _)))
        {
            yield return new(FormatNoteKind.UnknownKind, text,
                $"“{text}” isn't one of the seven kinds: system, actor, external system, storage, port, adapter or domain concept.");
        }

        if (term.KindTexts.Count > 2)
            yield return new(FormatNoteKind.TooManyKinds, "", $"{term.KindTexts.Count} kinds. A term has two at most.");

        foreach (var link in term.SeeAlso.Where(l => Resolve(l, glossary) is null))
            yield return new(FormatNoteKind.UnresolvedSeeAlso, link.Text, $"See also “{link.Text}” doesn't match a term here.");

        foreach (var word in term.Avoid.Where(w => !w.IsQualified))
        {
            if (glossary.Find(word.Word) is { } other && !ReferenceEquals(other, term))
                yield return new(FormatNoteKind.AvoidWordIsTerm, word.Word, $"“{word.Word}” is listed under Avoid, but it's also a term here.");
        }

        if (previous is not null && string.Compare(previous.Name, term.Name, StringComparison.OrdinalIgnoreCase) > 0)
            yield return new(FormatNoteKind.OutOfOrder, previous.Name, $"Out of alphabetical order: it comes after “{previous.Name}”.");
    }

    /// <summary>The term a See also link points to, by its anchor or, failing that, by name.</summary>
    public static Term? Resolve(SeeAlsoLink link, Glossary glossary) =>
        (link.Anchor is { Length: > 0 } anchor ? glossary.FindBySlug(anchor) : null) ?? glossary.Find(link.Text);
}

public static class SketchNotes
{
    /// <summary>A term's sketch ids that aren't in the sketch file.</summary>
    public static IReadOnlyList<SketchNote> For(Term term, SketchFile sketch) =>
        term.SketchIds.Where(id => sketch.Find(id) is null)
            .Select(id => new SketchNote(SketchNoteKind.IdNotInSketch, id, $"Sketch id {id} isn't in the sketch file any more."))
            .ToList();

    /// <summary>The elements and ports in the sketch file that no term has the id of.</summary>
    public static IReadOnlyList<SketchNote> WithoutTerms(Glossary glossary, SketchFile sketch)
    {
        var ids = glossary.Terms.SelectMany(t => t.SketchIds).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return sketch.Items.Where(i => i.ExpectsTerm && !ids.Contains(i.Id))
            .Select(i => new SketchNote(SketchNoteKind.NoTerm, i.Id, $"{i.Name} ({i.Type}) has no term."))
            .ToList();
    }
}
