using System.Text;

namespace Motif.Glossary;

/// <summary>Heading anchors, made the way GitHub makes them, so See also links resolve as they do there.</summary>
public static class Slug
{
    public static string Of(string heading)
    {
        var text = Inline.PlainText(heading).Trim().ToLowerInvariant();
        var slug = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (char.IsLetterOrDigit(c) || c is '-' or '_') slug.Append(c);
            else if (c == ' ') slug.Append('-');
        }
        return slug.ToString();
    }
}
