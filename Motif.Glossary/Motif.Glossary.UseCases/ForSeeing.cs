namespace Motif.Glossary;

/// <summary>
/// The User port's query half: what the Reviewer sees.
/// </summary>
public interface ForSeeing
{
    /// <summary>
    /// The review of the glossary that's open, or null before one is.
    /// </summary>
    Review? Review { get; }

    Source? Source { get; }

    bool ProposedOnly { get; }

    /// <summary>
    /// The review's groups, with only proposed terms when
    /// <see cref="ProposedOnly"/> is on.
    /// </summary>
    IReadOnlyList<KindGroup> Groups { get; }

    /// <summary>
    /// Why the last thing the Reviewer tried didn't open.
    /// <see cref="Problem.None"/> when all is well.
    /// </summary>
    Problem Problem { get; }

    /// <summary>
    /// Why the sketch file that came with the glossary couldn't be read. The
    /// glossary is still reviewed.
    /// </summary>
    Problem SketchFileProblem { get; }

    /// <summary>
    /// True while a repo is being read.
    /// </summary>
    bool Loading { get; }

    /// <summary>
    /// The repo link's fragment (repo=owner/name) when the review is of a
    /// repo, else null.
    /// </summary>
    string? RepoLinkFragment { get; }
}
