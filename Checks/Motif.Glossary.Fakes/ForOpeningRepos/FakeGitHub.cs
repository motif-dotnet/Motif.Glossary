using System.Net;

namespace Motif.Glossary.Fakes;

/// <summary>
/// raw.githubusercontent.com with no network, behind the platform's
/// <see cref="HttpMessageHandler"/> Seam. It answers as GitHub does: a GET of
/// https://raw.githubusercontent.com/owner/name/HEAD/path is 200 with the text
/// of the file committed at that path on the repo's default branch, and 404
/// when the file or the repo isn't there, or the repo isn't public. Owners and
/// repo names are matched in any case, paths exactly.
/// </summary>
public sealed class FakeGitHub : HttpMessageHandler
{
    private const string Host = "raw.githubusercontent.com";

    private readonly Dictionary<string, string> files = [];

    private TaskCompletionSource? held;

    private HttpStatusCode? down;

    /// <summary>
    /// Every request fails as a browser's fetch does with no network.
    /// </summary>
    public bool Unreachable { get; set; }

    /// <summary>
    /// The URLs asked for, in order.
    /// </summary>
    public List<string> Requests { get; } = [];

    /// <summary>
    /// Puts a file on a public repo's default branch.
    /// </summary>
    public FakeGitHub Commit(string repo, string path, string text)
    {
        files[Key(repo, path)] = text;
        return this;
    }

    /// <summary>
    /// Answers every request with this status, as GitHub does when it's down.
    /// </summary>
    public FakeGitHub Down(HttpStatusCode status)
    {
        down = status;
        return this;
    }

    /// <summary>
    /// Holds every request until <see cref="Release"/>, so a spec can act
    /// while a repo is still being read.
    /// </summary>
    public FakeGitHub Hold()
    {
        held = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        return this;
    }

    public void Release()
    {
        var gate = held;
        held = null;
        gate?.SetResult();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri!;
        lock (Requests) Requests.Add(url.AbsoluteUri);
        if (held is { } gate) await gate.Task;
        if (Unreachable) throw new HttpRequestException("Failed to fetch");
        if (down is { } status) return new HttpResponseMessage(status);
        return Find(request.Method, url) is { } text
            ? new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(text),
            }
            : new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    private string? Find(HttpMethod method, Uri url)
    {
        if (method != HttpMethod.Get || url.Scheme != "https"
            || url.Host != Host) return null;
        var parts = url.AbsolutePath.TrimStart('/').Split('/', 4);
        if (parts is not [var owner, var name, "HEAD", var path]) return null;
        var repo = Uri.UnescapeDataString(owner) + "/"
            + Uri.UnescapeDataString(name);
        return files.GetValueOrDefault(Key(repo, path));
    }

    private static string Key(string repo, string path) =>
        repo.ToLowerInvariant() + "|" + path;
}
