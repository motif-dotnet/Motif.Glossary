using System.Net;

namespace Motif.Glossary.Adapters;

/// <summary>
/// Reads a public repo's motif/ folder from raw.githubusercontent.com, at HEAD
/// (the default branch). No sign-in and no API rate limit, and
/// raw.githubusercontent.com allows cross-origin reads, so this works from
/// Pages. Its Seam is the platform's <see cref="HttpMessageHandler"/>: the
/// browser's in the app, a fake GitHub in specs.
/// </summary>
public sealed class ForOpeningReposViaGitHub(HttpMessageHandler github)
    : ForOpeningRepos
{
    private const string Host = "https://raw.githubusercontent.com";

    private readonly HttpClient http = new(github);

    public async Task<RepoFiles> Read(
        RepoName repo, CancellationToken cancellationToken = default)
    {
        var glossary = Fetch(repo, "motif/glossary.md", cancellationToken);
        var sketch = Fetch(
            repo, "motif/sketch.motifsketch.json", cancellationToken);
        return new RepoFiles(await glossary, await sketch);
    }

    /// <summary>
    /// The file's text, or null if it isn't there (or the repo isn't public:
    /// GitHub says 404 for both).
    /// </summary>
    private async Task<string?> Fetch(
        RepoName repo, string path, CancellationToken cancellationToken)
    {
        using var response =
            await http.GetAsync(Url(repo, path), cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static Uri Url(RepoName repo, string path) =>
        new($"{Host}/{Uri.EscapeDataString(repo.Owner)}/"
            + $"{Uri.EscapeDataString(repo.Name)}/HEAD/{path}");
}
