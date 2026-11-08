using System.Text;
using System.Text.RegularExpressions;

namespace Motif.Glossary;

/// <summary>
/// Reads a motif/glossary.md into a <see cref="Glossary"/>. Hand-written rather than Markdig: the format is a
/// fixed, line-based shape (a # title, ## per term, a bold-labelled field list), so a small reader is enough,
/// and the core keeps no package references for the dotnet motif CLI to inherit.
/// It never fails: whatever doesn't fit the format is kept as definition text, and the notes point it out.
/// </summary>
public static partial class GlossaryReader
{
    public static Glossary Read(string markdown)
    {
        var lines = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        string? title = null;
        var intro = new List<string>();
        var terms = new List<Term>();
        TermBuilder? current = null;
        var paragraph = new StringBuilder();
        string? fence = null;

        void EndParagraph()
        {
            if (paragraph.Length == 0) return;
            (current?.Definition ?? intro).Add(paragraph.ToString());
            paragraph.Clear();
        }

        void AddToParagraph(string text)
        {
            if (paragraph.Length > 0) paragraph.Append(' ');
            paragraph.Append(text.Trim());
        }

        for (var n = 0; n < lines.Length; n++)
        {
            var line = lines[n];
            var trimmed = line.Trim();

            // Code fences: nothing inside one is a heading or a field.
            if (fence is not null)
            {
                if (trimmed.StartsWith(fence, StringComparison.Ordinal)) fence = null;
                continue;
            }
            if (trimmed.StartsWith("```", StringComparison.Ordinal) || trimmed.StartsWith("~~~", StringComparison.Ordinal))
            {
                EndParagraph();
                fence = trimmed[..3];
                continue;
            }

            if (trimmed.Length == 0)
            {
                EndParagraph();
                continue;
            }

            if (Heading(line) is (int level, string text))
            {
                EndParagraph();
                if (level == 2)
                {
                    if (current is not null) terms.Add(current.Build());
                    current = new TermBuilder(text, n + 1);
                    continue;
                }
                if (level == 1 && current is null && title is null)
                {
                    title = text;
                    continue;
                }
                AddToParagraph(text);
                EndParagraph();
                continue;
            }

            if (current is not null && FieldLine().Match(line) is { Success: true } field)
            {
                EndParagraph();
                var value = new StringBuilder(field.Groups["value"].Value.Trim());
                // An indented line straight after a field carries on its value.
                while (n + 1 < lines.Length && lines[n + 1].Length > 0 && char.IsWhiteSpace(lines[n + 1][0])
                       && lines[n + 1].Trim().Length > 0 && !ListItem().IsMatch(lines[n + 1]))
                {
                    value.Append(' ').Append(lines[++n].Trim());
                }
                current.Field(field.Groups["label"].Value.Trim(), value.ToString());
                continue;
            }

            if (ListItem().IsMatch(line))
            {
                EndParagraph();
                AddToParagraph(trimmed);
                EndParagraph();
                continue;
            }

            AddToParagraph(trimmed);
        }

        EndParagraph();
        if (current is not null) terms.Add(current.Build());
        return new Glossary(title, intro, terms);
    }

    private static (int Level, string Text)? Heading(string line)
    {
        var m = HeadingLine().Match(line);
        if (!m.Success) return null;
        return (m.Groups["hashes"].Length, m.Groups["text"].Value.Trim().TrimEnd('#').Trim());
    }

    [GeneratedRegex(@"^ {0,3}(?<hashes>#{1,6})\s+(?<text>.+)$")]
    private static partial Regex HeadingLine();

    [GeneratedRegex(@"^\s{0,3}[-*+]\s+\*\*(?<label>[^*]+?)\s*:?\s*\*\*\s*:?\s*(?<value>.*)$")]
    private static partial Regex FieldLine();

    [GeneratedRegex(@"^\s*([-*+]|\d+\.)\s+")]
    private static partial Regex ListItem();

    [GeneratedRegex(@"`([^`]+)`")]
    private static partial Regex CodeSpan();

    [GeneratedRegex(@"\[(?<text>[^\]]+)\]\((?<href>[^)]*)\)")]
    private static partial Regex Link();

    [GeneratedRegex(@"^[""“'`](?<word>[^""”'`]+)[""”'`]\s*(?<rest>.*)$")]
    private static partial Regex Quoted();

    private sealed class TermBuilder(string name, int line)
    {
        public List<string> Definition { get; } = [];
        private readonly List<string> kindTexts = [];
        private readonly List<AvoidWord> avoid = [];
        private readonly List<string> sketchIds = [];
        private readonly List<SeeAlsoLink> seeAlso = [];
        private readonly List<KeyValuePair<string, string>> other = [];
        private string? inCode;
        private string? statusText;
        private string? sketchText;

        public void Field(string label, string value)
        {
            switch (label.ToLowerInvariant())
            {
                case "kind":
                case "kinds":
                    kindTexts.AddRange(SplitTopLevel(value, ',', ';')
                        .Select(k => Collapse(StripNotes(Inline.PlainText(k)).Trim().Trim('.')))
                        .Where(k => k.Length > 0));
                    break;
                case "avoid":
                    avoid.AddRange(AvoidWords(value));
                    break;
                case "in code":
                    inCode = value;
                    break;
                case "status":
                    statusText = Inline.PlainText(value).Trim().Trim('.');
                    break;
                case "sketch":
                    sketchText = value;
                    sketchIds.AddRange(SketchIds(value));
                    break;
                case "see also":
                    seeAlso.AddRange(SeeAlsoLinks(value));
                    break;
                default:
                    other.Add(new(label, value));
                    break;
            }
        }

        public Term Build() => new()
        {
            Name = Inline.PlainText(name),
            Slug = Slug.Of(name),
            Definition = Definition,
            KindTexts = kindTexts,
            Kinds = kindTexts.Select(k => Kinds.TryRead(k, out var kind) ? kind : (Kind?)null)
                .OfType<Kind>().Distinct().ToList(),
            Avoid = avoid,
            InCode = inCode,
            Status = string.Equals(FirstWord(statusText), "proposed", StringComparison.OrdinalIgnoreCase)
                ? Status.Proposed : Status.Agreed,
            StatusText = statusText,
            SketchIds = sketchIds,
            SketchText = sketchText,
            SeeAlso = seeAlso,
            OtherFields = other,
            Line = line,
        };

        private static string? FirstWord(string? text) =>
            text?.Split([' ', ',', ';', '('], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
    }

    private static IEnumerable<string> SketchIds(string value)
    {
        var codes = CodeSpan().Matches(value).Select(m => m.Groups[1].Value.Trim()).ToList();
        if (codes.Count > 0) return codes;
        return SplitTopLevel(StripNotes(value), ',', ';')
            .Select(p => p.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "")
            .Where(id => id.Length > 0);
    }

    private static IEnumerable<SeeAlsoLink> SeeAlsoLinks(string value)
    {
        var links = Link().Matches(value).ToList();
        if (links.Count > 0)
        {
            return links.Select(m =>
            {
                var href = m.Groups["href"].Value.Trim();
                return new SeeAlsoLink(Inline.PlainText(m.Groups["text"].Value), href.StartsWith('#') ? href[1..] : null);
            });
        }
        return SplitTopLevel(value, ',', ';')
            .Select(p => Inline.PlainText(p).Trim())
            .Where(p => p.Length > 0)
            .Select(p => new SeeAlsoLink(p, null));
    }

    /// <summary>
    /// Avoid words, with the notes that go with them. "user (that's the port)" is the word "user" with a note;
    /// "project, on its own" is "project" with the note "on its own"; "primary and secondary" is two words.
    /// </summary>
    internal static IEnumerable<AvoidWord> AvoidWords(string value)
    {
        var words = new List<AvoidWord>();
        foreach (var piece in SplitTopLevel(value, ',', ';'))
        {
            var (rest, notes) = TakeNotes(piece.Trim());
            rest = rest.Trim().TrimEnd('.');
            foreach (var lead in new[] { "and ", "or " })
            {
                if (rest.StartsWith(lead, StringComparison.OrdinalIgnoreCase)) rest = rest[lead.Length..].TrimStart();
            }

            // "on its own", "for a person": a note on the word before.
            if (words.Count > 0 && (rest.StartsWith("on its own", StringComparison.OrdinalIgnoreCase)
                || rest.StartsWith("for ", StringComparison.OrdinalIgnoreCase)
                || rest.StartsWith("which ", StringComparison.OrdinalIgnoreCase)))
            {
                var last = words[^1];
                words[^1] = last with { Note = Join(last.Note, Join(rest, notes)) };
                continue;
            }

            if (Quoted().Match(rest) is { Success: true } quoted)
            {
                var after = quoted.Groups["rest"].Value.Trim();
                words.Add(new AvoidWord(quoted.Groups["word"].Value.Trim(), Join(after.Length > 0 ? after : null, notes)));
                continue;
            }

            var parts = rest.Split(" and ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (var p = 0; p < parts.Length; p++)
            {
                var word = Inline.PlainText(parts[p]).Trim().Trim('"', '“', '”');
                if (word.Length == 0) continue;
                words.Add(new AvoidWord(word, p == parts.Length - 1 ? notes : null));
            }
        }
        return words;
    }

    private static string? Join(string? a, string? b) =>
        string.IsNullOrWhiteSpace(a) ? b : string.IsNullOrWhiteSpace(b) ? a : $"{a}; {b}";

    /// <summary>Takes the top-level parenthesised notes out of a piece of text.</summary>
    private static (string Remainder, string? Notes) TakeNotes(string text)
    {
        var rest = new StringBuilder();
        var notes = new List<string>();
        var note = new StringBuilder();
        var depth = 0;
        foreach (var c in text)
        {
            if (c == '(' && depth++ == 0) continue;
            if (c == ')' && depth > 0 && --depth == 0)
            {
                notes.Add(note.ToString().Trim());
                note.Clear();
                continue;
            }
            (depth > 0 ? note : rest).Append(c);
        }
        if (note.Length > 0) rest.Append('(').Append(note);
        return (Collapse(rest.ToString()), notes.Count == 0 ? null : string.Join("; ", notes));
    }

    private static string StripNotes(string text) => TakeNotes(text).Remainder;

    private static string Collapse(string text) => string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries));

    /// <summary>Splits on the separators, but not inside parentheses, brackets or code spans.</summary>
    private static List<string> SplitTopLevel(string text, params char[] separators)
    {
        var parts = new List<string>();
        var part = new StringBuilder();
        var depth = 0;
        var inCode = false;
        foreach (var c in text)
        {
            if (c == '`') inCode = !inCode;
            else if (!inCode && c is '(' or '[') depth++;
            else if (!inCode && c is ')' or ']' && depth > 0) depth--;
            if (!inCode && depth == 0 && separators.Contains(c))
            {
                parts.Add(part.ToString());
                part.Clear();
                continue;
            }
            part.Append(c);
        }
        parts.Add(part.ToString());
        return parts.Where(p => p.Trim().Length > 0).ToList();
    }
}
