using System.Text.RegularExpressions;

namespace Motif.Glossary;

/// <summary>A public GitHub repo, owner/name.</summary>
public sealed partial record RepoName(string Owner, string Name)
{
    public override string ToString() => $"{Owner}/{Name}";

    /// <summary>Reads "owner/name", or a github.com URL to the repo or anything in it.</summary>
    public static bool TryParse(string? text, out RepoName repo)
    {
        repo = null!;
        var s = text?.Trim() ?? "";
        var url = false;
        foreach (var prefix in new[] { "https://", "http://", "www.", "github.com/" })
        {
            if (!s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
            s = s[prefix.Length..];
            url = true;
        }
        var parts = s.Split(['/', '?', '#'], StringSplitOptions.RemoveEmptyEntries);
        // A plain name is exactly owner/name; a URL can go on into the repo.
        if (parts.Length < 2 || (!url && parts.Length > 2)) return false;
        var name = parts[1].EndsWith(".git", StringComparison.OrdinalIgnoreCase) ? parts[1][..^4] : parts[1];
        if (!OwnerPattern().IsMatch(parts[0]) || !NamePattern().IsMatch(name) || name is "." or "..") return false;
        repo = new RepoName(parts[0], name);
        return true;
    }

    [GeneratedRegex(@"^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$")]
    private static partial Regex OwnerPattern();

    [GeneratedRegex(@"^[A-Za-z0-9._-]{1,100}$")]
    private static partial Regex NamePattern();
}

/// <summary>
/// A link ending #repo=owner/name that opens straight into a review of that repo's glossary. It carries only
/// the repo's name, so it always shows the latest glossary on the default branch.
/// </summary>
public static class RepoLink
{
    public const string Prefix = "repo=";

    public static string Fragment(RepoName repo) => Prefix + repo;

    public static bool TryRead(string? fragment, out RepoName repo)
    {
        repo = null!;
        var f = Uri.UnescapeDataString(fragment?.TrimStart('#') ?? "");
        return f.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
            && RepoName.TryParse(f[Prefix.Length..], out repo);
    }
}
