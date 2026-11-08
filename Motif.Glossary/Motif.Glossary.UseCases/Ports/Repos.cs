namespace Motif.Glossary;

/// <summary>What a repo's motif/ folder holds on its default branch. Either can be missing.</summary>
/// <param name="Glossary">motif/glossary.md, or null if the repo has none (or isn't public).</param>
/// <param name="SketchFile">motif/sketch.motifsketch.json, or null if there isn't one.</param>
public sealed record RepoFiles(string? Glossary, string? SketchFile);

/// <summary>Reads a public repo's motif/glossary.md and, when it's there, its sketch file, from the default branch.</summary>
public interface Repos
{
    /// <summary>Throws <see cref="HttpRequestException"/> when the repo can't be reached at all.</summary>
    Task<RepoFiles> Read(RepoName repo, CancellationToken cancellationToken = default);
}
