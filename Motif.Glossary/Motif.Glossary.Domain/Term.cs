namespace Motif.Glossary;

/// <summary>Whether a term is proposed or agreed. A term with no status counts as agreed.</summary>
public enum Status
{
    Agreed,
    Proposed,
}

/// <summary>A word listed under a term's Avoid field, with the note that explains or limits it, if any.</summary>
/// <remarks>"user (that's the port)" and "project, on its own" both have notes; "sample" doesn't.</remarks>
public sealed record AvoidWord(string Word, string? Note = null)
{
    /// <summary>True when the glossary already says why, or when, the word is avoided.</summary>
    public bool IsQualified => Note is not null;
}

/// <summary>One link in a term's See also field: the text shown and the heading anchor it points to.</summary>
public sealed record SeeAlsoLink(string Text, string? Anchor);

/// <summary>One entry in a glossary: a ## heading naming it, a short definition, then its fields.</summary>
public sealed record Term
{
    public required string Name { get; init; }

    /// <summary>The heading's anchor, as GitHub makes it: "Sketch id" becomes "sketch-id".</summary>
    public required string Slug { get; init; }

    /// <summary>The definition's paragraphs, as Markdown.</summary>
    public IReadOnlyList<string> Definition { get; init; } = [];

    /// <summary>The Kind field as written, one entry per comma-separated value.</summary>
    public IReadOnlyList<string> KindTexts { get; init; } = [];

    /// <summary>The kinds out of <see cref="KindTexts"/> that are one of the seven, in the order written.</summary>
    public IReadOnlyList<Kind> Kinds { get; init; } = [];

    public IReadOnlyList<AvoidWord> Avoid { get; init; } = [];

    /// <summary>The In code field, as Markdown.</summary>
    public string? InCode { get; init; }

    public Status Status { get; init; } = Status.Agreed;

    /// <summary>The Status field as written, when there is one.</summary>
    public string? StatusText { get; init; }

    public IReadOnlyList<string> SketchIds { get; init; } = [];

    /// <summary>The Sketch field as written, which can say which id is which: "`c5a10a0a` (element)".</summary>
    public string? SketchText { get; init; }

    public IReadOnlyList<SeeAlsoLink> SeeAlso { get; init; } = [];

    /// <summary>Fields with a label outside the six, kept so nothing people wrote is lost.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> OtherFields { get; init; } = [];

    /// <summary>The heading's line number in the file, from 1.</summary>
    public int Line { get; init; }

    public bool IsProposed => Status == Status.Proposed;
}

/// <summary>A project's agreed words, read from its motif/glossary.md.</summary>
public sealed record Glossary(string? Title, IReadOnlyList<string> Intro, IReadOnlyList<Term> Terms)
{
    public Term? Find(string name) =>
        Terms.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));

    public Term? FindBySlug(string slug) =>
        Terms.FirstOrDefault(t => string.Equals(t.Slug, slug, StringComparison.OrdinalIgnoreCase));
}
