namespace Motif.Glossary;

/// <summary>
/// A project's agreed words, read from its motif/glossary.md.
/// </summary>
public sealed record Glossary(
    string? Title, IReadOnlyList<string> Intro, IReadOnlyList<Term> Terms)
{
    public const string FileName = "glossary.md";

    public Term? Find(string name) =>
        Terms.FirstOrDefault(t =>
            string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));

    public Term? FindBySlug(string slug) =>
        Terms.FirstOrDefault(t =>
            string.Equals(t.Slug, slug, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The term a See also link points to, by its anchor or, failing that, by
    /// name.
    /// </summary>
    public Term? Resolve(SeeAlsoLink link) =>
        (link.Anchor is { Length: > 0 } anchor ? FindBySlug(anchor) : null)
        ?? Find(link.Text);
}
