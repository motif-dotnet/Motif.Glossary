namespace Motif.Glossary;

public enum SourceKind
{
    Example,
    Files,
    Pasted,
    Repo,
}

/// <summary>Where the glossary under review came from: the Example, a file, pasted Markdown or a public repo.</summary>
/// <param name="Name">"glossary.md", "owner/name", and so on.</param>
/// <param name="SketchFileName">The sketch file read with it, if any.</param>
public sealed record Source(SourceKind Kind, string Name, string? SketchFileName = null, RepoName? Repo = null);
