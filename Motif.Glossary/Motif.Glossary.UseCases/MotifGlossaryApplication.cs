namespace Motif.Glossary;

/// <summary>The application: reads a glossary from one of the four sources and keeps its review.</summary>
public sealed class MotifGlossaryApplication(Repos repos, Files files) : UserTasks, UserQueries
{
    private int attempt;

    public Review? Review { get; private set; }
    public Source? Source { get; private set; }
    public bool ProposedOnly { get; private set; }
    public IReadOnlyList<KindGroup> Groups => Review?.GroupsFor(ProposedOnly) ?? [];
    public string? Problem { get; private set; }
    public string? SketchFileProblem { get; private set; }
    public bool Loading { get; private set; }

    public string? RepoLinkFragment =>
        Source is { Kind: SourceKind.Repo, Repo: { } repo } ? RepoLink.Fragment(repo) : null;

    public void OpenExample()
    {
        attempt++;
        Show(new Source(SourceKind.Example, Example.Name, "sketch.motifsketch.json"), Example.Glossary(), Example.SketchFile());
    }

    public void Paste(string markdown)
    {
        attempt++;
        if (string.IsNullOrWhiteSpace(markdown))
        {
            Problem = "There's nothing to review: paste a glossary's Markdown first.";
            return;
        }
        Show(new Source(SourceKind.Pasted, "Pasted Markdown"), markdown, null);
    }

    public async Task OpenFiles() => OpenLocal(await files.Pick(), dropped: false);

    public async Task OpenDroppedFiles() => OpenLocal(await files.TakeDropped(), dropped: true);

    private void OpenLocal(IReadOnlyList<LocalFile> picked, bool dropped)
    {
        if (picked.Count == 0) return;
        attempt++;
        var glossary = picked.FirstOrDefault(f => f.Name.Equals("glossary.md", StringComparison.OrdinalIgnoreCase))
            ?? picked.FirstOrDefault(f => HasExtension(f, ".md", ".markdown", ".txt"));
        var sketch = picked.FirstOrDefault(f => f.Name.EndsWith(".motifsketch.json", StringComparison.OrdinalIgnoreCase))
            ?? picked.FirstOrDefault(f => HasExtension(f, ".json"));

        if (glossary is null && sketch is not null && Review is not null && Source is not null)
        {
            // A sketch file on its own joins the glossary that's already open.
            Show(Source with { SketchFileName = sketch.Name }, null, sketch.Text, Review.Glossary);
            return;
        }
        if (glossary is null)
        {
            Problem = $"{(dropped ? "Drop" : "Pick")} a glossary.md, and its sketch file too if you like. "
                + (sketch is not null ? "A sketch file on its own needs a glossary open first." : "None of those was a Markdown file.");
            return;
        }
        Show(new Source(SourceKind.Files, glossary.Name, sketch?.Name), glossary.Text, sketch?.Text);
    }

    public async Task OpenRepo(string repo)
    {
        if (!RepoName.TryParse(repo, out var name))
        {
            attempt++;
            Problem = $"“{repo.Trim()}” isn't a GitHub repo. Give it as owner/name, such as motif-dotnet/Motif.Sketch.";
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
        var mine = ++attempt;
        Loading = true;
        Problem = null;
        try
        {
            var read = await repos.Read(repo);
            if (mine != attempt) return;
            if (read.Glossary is null)
            {
                Problem = $"{repo} has no motif/glossary.md on its default branch, or it isn't a public repo.";
                return;
            }
            Show(new Source(SourceKind.Repo, repo.ToString(), read.SketchFile is null ? null : "sketch.motifsketch.json", repo),
                read.Glossary, read.SketchFile);
        }
        catch (HttpRequestException e)
        {
            if (mine == attempt) Problem = $"GitHub couldn't be reached for {repo} ({e.Message}).";
        }
        finally
        {
            if (mine == attempt) Loading = false;
        }
    }

    public void ShowProposedOnly(bool proposedOnly) => ProposedOnly = proposedOnly;

    public void DismissProblem() => Problem = null;

    private void Show(Source source, string? markdown, string? sketchJson, Glossary? glossary = null)
    {
        glossary ??= GlossaryReader.Read(markdown ?? "");
        Loading = false;
        if (glossary.Terms.Count == 0)
        {
            Problem = $"No terms in {source.Name}. A glossary has one ## heading per term.";
            return;
        }

        SketchFile? sketch = null;
        string? sketchProblem = null;
        if (sketchJson is not null && !SketchFileReader.TryRead(sketchJson, out sketch, out var why))
        {
            sketchProblem = $"The sketch file couldn't be read, so sketch ids are shown as they are. {why}";
            source = source with { SketchFileName = null };
        }

        Review = Review.Of(glossary, sketch);
        Source = source;
        SketchFileProblem = sketchProblem;
        Problem = null;
    }

    private static bool HasExtension(LocalFile file, params string[] extensions) =>
        extensions.Any(e => file.Name.EndsWith(e, StringComparison.OrdinalIgnoreCase));
}
