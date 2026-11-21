using System.Text;

namespace Motif.Glossary;

public enum SpanKind
{
    Text,
    Code,
    Strong,
    Emphasis,
    Link,
}

/// <summary>
/// A run of inline Markdown: plain text, a code span, bold, italic or a link.
/// </summary>
public sealed record InlineSpan(SpanKind Kind, string Text, string? Href = null);

/// <summary>
/// The little inline Markdown a glossary uses: `code`, **bold**, *italic* and
/// [links](#anchor). Nesting is flattened: bold inside a link is shown as the
/// link's plain text.
/// </summary>
public static class Inline
{
    public static IReadOnlyList<InlineSpan> Parse(string markdown)
    {
        var spans = new List<InlineSpan>();
        var text = new StringBuilder();
        var i = 0;

        void Flush()
        {
            if (text.Length == 0) return;
            spans.Add(new InlineSpan(SpanKind.Text, text.ToString()));
            text.Clear();
        }

        while (i < markdown.Length)
        {
            var c = markdown[i];
            if (c == '\\' && i + 1 < markdown.Length
                && IsEscapable(markdown[i + 1]))
            {
                text.Append(markdown[i + 1]);
                i += 2;
                continue;
            }
            if (c == '`')
            {
                var ticks = Run(markdown, i, '`');
                var close = markdown.IndexOf(
                    new string('`', ticks), i + ticks, StringComparison.Ordinal);
                if (close > 0)
                {
                    Flush();
                    var code = markdown[(i + ticks)..close].Trim();
                    spans.Add(new InlineSpan(SpanKind.Code, code));
                    i = close + ticks;
                    continue;
                }
            }
            if (c == '['
                && TryLink(markdown, i, out var linkText, out var href, out var end))
            {
                Flush();
                spans.Add(new InlineSpan(SpanKind.Link, PlainText(linkText), href));
                i = end;
                continue;
            }
            if (c == '*')
            {
                var stars = Math.Min(Run(markdown, i, '*'), 2);
                var marker = new string('*', stars);
                var close = markdown.IndexOf(
                    marker, i + stars, StringComparison.Ordinal);
                if (close > i + stars && !char.IsWhiteSpace(markdown[i + stars]))
                {
                    Flush();
                    var kind = stars == 2 ? SpanKind.Strong : SpanKind.Emphasis;
                    var inner = markdown[(i + stars)..close];
                    spans.Add(new InlineSpan(kind, PlainText(inner)));
                    i = close + stars;
                    continue;
                }
            }
            text.Append(c);
            i++;
        }
        Flush();
        return spans;
    }

    /// <summary>
    /// The text with its Markdown markers taken out.
    /// </summary>
    public static string PlainText(string markdown)
    {
        if (markdown.IndexOfAny(['`', '*', '[', '\\']) < 0) return markdown;
        return string.Concat(Parse(markdown).Select(s => s.Text));
    }

    private static bool TryLink(
        string markdown, int start,
        out string text, out string href, out int end)
    {
        text = href = "";
        end = start;
        var depth = 0;
        var closeBracket = -1;
        for (var j = start; j < markdown.Length; j++)
        {
            if (markdown[j] == '[') depth++;
            else if (markdown[j] == ']' && --depth == 0)
            {
                closeBracket = j;
                break;
            }
        }
        if (closeBracket < 0 || closeBracket + 1 >= markdown.Length
            || markdown[closeBracket + 1] != '(')
        {
            return false;
        }
        var closeParen = markdown.IndexOf(')', closeBracket + 2);
        if (closeParen < 0) return false;
        text = markdown[(start + 1)..closeBracket];
        href = markdown[(closeBracket + 2)..closeParen].Trim();
        end = closeParen + 1;
        return true;
    }

    /// <summary>
    /// How many of the character there are in a row from the start.
    /// </summary>
    private static int Run(string markdown, int start, char c)
    {
        var n = 0;
        while (start + n < markdown.Length && markdown[start + n] == c) n++;
        return n;
    }

    private static bool IsEscapable(char c) =>
        c is '\\' or '`' or '*' or '_' or '[' or ']' or '(' or ')' or '#' or '-';
}
