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

/// <summary>
/// A gentle note on a term that doesn't follow the glossary format. Notes
/// report; they never block.
/// </summary>
/// <param name="Subject">
/// What the note is about: the kind, link, Avoid word or heading it names.
/// </param>
public sealed record FormatNote(
    FormatNoteKind Kind, string Subject, string Text);

public static class FormatNotes
{
    /// <summary>
    /// The format notes on a term, given the term before it in the glossary,
    /// if any, for the alphabetical order.
    /// </summary>
    public static IReadOnlyList<FormatNote> For(
        Term term, Term? previous, Glossary glossary) =>
        Find(term, previous, glossary).ToList();

    private static IEnumerable<FormatNote> Find(
        Term term, Term? previous, Glossary glossary)
    {
        if (term.KindTexts.Count == 0)
        {
            yield return new(FormatNoteKind.NoKind, "",
                "No Kind yet. Every term has one of the seven kinds.");
        }

        foreach (var text in term.KindTexts.Where(k => !Kinds.TryRead(k, out _)))
        {
            yield return new(FormatNoteKind.UnknownKind, text,
                $"“{text}” isn't one of the seven kinds: system, actor, "
                + "external system, storage, port, adapter or domain concept.");
        }

        if (term.KindTexts.Count > 2)
        {
            yield return new(FormatNoteKind.TooManyKinds, "",
                $"{term.KindTexts.Count} kinds. A term has two at most.");
        }

        foreach (var link in term.SeeAlso.Where(l => glossary.Resolve(l) is null))
        {
            yield return new(FormatNoteKind.UnresolvedSeeAlso, link.Text,
                $"See also “{link.Text}” doesn't match a term here.");
        }

        foreach (var word in term.Avoid.Where(w => !w.IsQualified))
        {
            if (glossary.Find(word.Word) is { } other
                && !ReferenceEquals(other, term))
            {
                yield return new(FormatNoteKind.AvoidWordIsTerm, word.Word,
                    $"“{word.Word}” is listed under Avoid, but it's also a "
                    + "term here.");
            }
        }

        if (previous is not null && string.Compare(
                previous.Name, term.Name, StringComparison.OrdinalIgnoreCase) > 0)
        {
            yield return new(FormatNoteKind.OutOfOrder, previous.Name,
                "Out of alphabetical order: it comes after "
                + $"“{previous.Name}”.");
        }
    }
}
