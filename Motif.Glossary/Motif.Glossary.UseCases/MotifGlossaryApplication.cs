namespace Motif.Glossary;

/// <summary>
/// The application: reads a glossary from one of the four sources and keeps
/// its review.
/// </summary>
public sealed class MotifGlossaryApplication(
    ForOpeningRepos repos, ForOpeningFiles files) : ForDoing, ForSeeing
{
    // Counts what the Reviewer has tried, so a repo read that something newer
    // has overtaken is dropped when it comes back.
    private int attempt;

    public Review? Review { get; private set; }
    public Source? Source { get; private set; }
    public bool ProposedOnly { get; private set; }
    public IReadOnlyList<KindGroup> Groups =>
        Review?.GroupsFor(ProposedOnly) ?? [];
    public Problem Problem { get; private set; } = Problem.None;
    public Problem SketchFileProblem { get; private set; } = Problem.None;
    public bool Loading { get; private set; }

    public string? RepoLinkFragment =>
        Source is { Kind: SourceKind.Repo, Repo: { } repo }
            ? RepoLink.Fragment(repo)
            : null;

    public void OpenExample()
    {
        Overtake();
        Show(
            new Source(SourceKind.Example, Example.Name, SketchFile.FileName),
            Example.Glossary(),
            Example.SketchFile());
    }

    public void Paste(string markdown)
    {
        Overtake();
        if (string.IsNullOrWhiteSpace(markdown))
        {
            Problem = Unreviewable(
                "There's nothing to review: paste a glossary's Markdown first.");
            return;
        }
        Show(new Source(SourceKind.Pasted, "Pasted Markdown"), markdown, null);
    }

    public async Task OpenFiles() =>
        OpenLocal(await files.Pick(), dropped: false);

    public async Task OpenDroppedFiles() =>
        OpenLocal(await files.TakeDropped(), dropped: true);

    private void OpenLocal(IReadOnlyList<LocalFile> picked, bool dropped)
    {
        if (picked.Count == 0) return;
        Overtake();
        var glossary = picked.FirstOrDefault(f => f.Name.Equals(
                Glossary.FileName, StringComparison.OrdinalIgnoreCase))
            ?? picked.FirstOrDefault(f => EndsWith(f, ".md", ".markdown", ".txt"));
        var sketch = picked.FirstOrDefault(f => EndsWith(f, ".motifsketch.json"))
            ?? picked.FirstOrDefault(f => EndsWith(f, ".json"));

        if (glossary is null && sketch is not null
            && Review is not null && Source is not null)
        {
            // A sketch file on its own joins the glossary that's already open.
            Show(
                Source with { SketchFileName = sketch.Name },
                Review.Glossary,
                sketch.Text);
            return;
        }
        if (glossary is null)
        {
            var verb = dropped ? "Drop" : "Pick";
            var why = sketch is not null
                ? "A sketch file on its own needs a glossary open first."
                : "None of those was a Markdown file.";
            Problem = Unreviewable(
                $"{verb} a glossary.md, and its sketch file too if you like. "
                + why);
            return;
        }
        Show(
            new Source(SourceKind.Files, glossary.Name, sketch?.Name),
            glossary.Text,
            sketch?.Text);
    }

    public async Task OpenRepo(string repo)
    {
        if (!RepoName.TryParse(repo, out var name))
        {
            Overtake();
            Problem = Unreviewable(
                $"“{repo.Trim()}” isn't a GitHub repo. Give it as owner/name, "
                + "such as motif-dotnet/Motif.Sketch.");
            return;
        }
        await Open(name);
    }

    public async Task<bool> OpenLink(string? fragment)
    {
        if (!RepoLink.TryRead(fragment, out var repo)) return false;
        await Open(repo);
        return true;
    }

    private async Task Open(RepoName repo)
    {
        var mine = Overtake();
        Loading = true;
        Problem = Problem.None;
        try
        {
            var read = await repos.Read(repo);
            if (mine != attempt) return;
            if (read.Glossary is null)
            {
                Problem = Unreviewable(
                    $"{repo} has no motif/glossary.md on its default branch, "
                    + "or it isn't a public repo.");
                return;
            }
            var sketchFileName =
                read.SketchFile is null ? null : SketchFile.FileName;
            Show(
                new Source(SourceKind.Repo, repo.ToString(), sketchFileName, repo),
                read.Glossary,
                read.SketchFile);
        }
        catch (HttpRequestException e)
        {
            if (mine == attempt)
            {
                Problem = Unreviewable(
                    $"GitHub couldn't be reached for {repo} ({e.Message}).");
            }
        }
        finally
        {
            if (mine == attempt) Loading = false;
        }
    }

    public void ShowProposedOnly(bool proposedOnly) =>
        ProposedOnly = proposedOnly;

    public void DismissProblem() => Problem = Problem.None;

    /// <summary>
    /// Starts something new the Reviewer tried, so a repo still being read is
    /// dropped and no longer loading. Returns the new attempt.
    /// </summary>
    private int Overtake()
    {
        Loading = false;
        return ++attempt;
    }

    private void Show(Source source, string markdown, string? sketchJson) =>
        Show(source, GlossaryReader.Read(markdown), sketchJson);

    private void Show(Source source, Glossary glossary, string? sketchJson)
    {
        if (glossary.Terms.Count == 0)
        {
            Problem = Unreviewable(
                $"No terms in {source.Name}. "
                + "A glossary has one ## heading per term.");
            return;
        }

        SketchFile? sketch = null;
        var sketchProblem = Problem.None;
        if (sketchJson is not null
            && !SketchFileReader.TryRead(sketchJson, out sketch, out var why))
        {
            sketchProblem = new Problem(
                "The sketch file couldn't be read",
                $"Sketch ids are shown as they are. {why}");
            source = source with { SketchFileName = null };
        }

        Review = Review.Of(glossary, sketch);
        Source = source;
        SketchFileProblem = sketchProblem;
        Problem = Problem.None;
    }

    private static Problem Unreviewable(string text) =>
        new("That glossary can't be reviewed", text);

    private static bool EndsWith(LocalFile file, params string[] endings) =>
        endings.Any(end =>
            file.Name.EndsWith(end, StringComparison.OrdinalIgnoreCase));
}
