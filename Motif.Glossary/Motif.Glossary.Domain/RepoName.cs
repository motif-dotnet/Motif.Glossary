using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Motif.Glossary;

/// <summary>
/// A public GitHub repo, owner/name.
/// </summary>
public sealed partial record RepoName(string Owner, string Name)
{
    public override string ToString() => $"{Owner}/{Name}";

    /// <summary>
    /// Reads "owner/name", or a github.com URL to the repo or anything in it.
    /// </summary>
    public static bool TryParse(
        string? text, [NotNullWhen(true)] out RepoName? repo)
    {
        repo = null;
        var rest = text?.Trim() ?? "";
        var url = false;
        string[] prefixes = ["https://", "http://", "www.", "github.com/"];
        foreach (var prefix in prefixes)
        {
            if (!rest.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;
            rest = rest[prefix.Length..];
            url = true;
        }
        var parts = rest.Split(
            ['/', '?', '#'], StringSplitOptions.RemoveEmptyEntries);
        // A plain name is exactly owner/name; a URL can go on into the repo.
        if (parts.Length < 2 || (!url && parts.Length > 2)) return false;
        var (owner, name) = (parts[0], parts[1]);
        if (name.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];
        if (!OwnerPattern().IsMatch(owner) || !NamePattern().IsMatch(name)
            || name is "." or "..")
        {
            return false;
        }
        repo = new RepoName(owner, name);
        return true;
    }

    [GeneratedRegex(@"^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$")]
    private static partial Regex OwnerPattern();

    [GeneratedRegex(@"^[A-Za-z0-9._-]{1,100}$")]
    private static partial Regex NamePattern();
}
