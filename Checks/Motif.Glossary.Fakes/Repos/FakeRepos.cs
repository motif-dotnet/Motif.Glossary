namespace Motif.Glossary.Fakes;

/// <summary>Public repos held in memory. A repo not added has no glossary; <see cref="Unreachable"/> makes every read fail.</summary>
public sealed class FakeRepos : Repos
{
    private readonly Dictionary<string, RepoFiles> repos = new(StringComparer.OrdinalIgnoreCase);

    public bool Unreachable { get; set; }

    /// <summary>The repos read, in order.</summary>
    public List<RepoName> Reads { get; } = [];

    public FakeRepos Add(string repo, string? glossary, string? sketchFile = null)
    {
        repos[repo] = new RepoFiles(glossary, sketchFile);
        return this;
    }

    private TaskCompletionSource? held;

    /// <summary>Holds every read until <see cref="Release"/>, so a spec can act while a repo is still being read.</summary>
    public FakeRepos Hold()
    {
        held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        return this;
    }

    public void Release()
    {
        var gate = held;
        held = null;
        gate?.SetResult();
    }

    public async Task<RepoFiles> Read(RepoName repo, CancellationToken cancellationToken = default)
    {
        Reads.Add(repo);
        if (held is { } gate) await gate.Task;
        if (Unreachable) throw new HttpRequestException("no network");
        return repos.TryGetValue(repo.ToString(), out var files) ? files : new RepoFiles(null, null);
    }
}
