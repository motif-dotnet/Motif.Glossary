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
public sealed record Source(SourceKind Kind, string Name, string? SketchFileName = null, RepoName? Repo = null)
{
    /// <summary>Where the glossary came from, in a phrase, with its sketch file if any.</summary>
    public string About => Kind switch
    {
        SourceKind.Repo => $"motif/glossary.md from {Name}'s default branch"
            + (SketchFileName is null ? ", with no sketch file" : ", with its sketch file"),
        SourceKind.Example => "The Example: Motif Sketch's glossary and sketch file",
        _ => Name + (SketchFileName is { } sketch ? $", with {sketch}" : ""),
    };
}
