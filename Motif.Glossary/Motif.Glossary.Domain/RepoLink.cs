using System.Diagnostics.CodeAnalysis;

namespace Motif.Glossary;

/// <summary>
/// A link ending #repo=owner/name that opens straight into a review of that
/// repo's glossary. It carries only the repo's name, so it always shows the
/// latest glossary on the default branch.
/// </summary>
public static class RepoLink
{
    public const string Prefix = "repo=";

    public static string Fragment(RepoName repo) => Prefix + repo;

    public static bool TryRead(
        string? fragment, [NotNullWhen(true)] out RepoName? repo)
    {
        repo = null;
        var unescaped = Uri.UnescapeDataString(fragment?.TrimStart('#') ?? "");
        return unescaped.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase)
            && RepoName.TryParse(unescaped[Prefix.Length..], out repo);
    }
}
