namespace Motif.Glossary.App;

/// <summary>Where links in a glossary go: to another term on the page, out to the web, or into the repo it came from.</summary>
public sealed class Links(Review? review, Source? source, Action<Term> scrollTo)
{
    public Review? Review { get; } = review;

    public void ScrollTo(Term term) => scrollTo(term);

    /// <summary>A link's address, or null when it can't go anywhere (a relative link with no repo to resolve it in).</summary>
    public string? Url(string href)
    {
        if (href.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || href.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            return href;
        if (href.Length == 0 || href.StartsWith('#') || source?.Repo is not { } repo) return null;
        // Relative to motif/glossary.md in the repo.
        var path = href.StartsWith('/') ? href.TrimStart('/') : "motif/" + href;
        return $"https://github.com/{repo}/blob/HEAD/{path}";
    }
}
