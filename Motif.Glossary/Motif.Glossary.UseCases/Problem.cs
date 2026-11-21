namespace Motif.Glossary;

/// <summary>
/// Why something didn't work, as a title and a sentence or two that any UI
/// can show. <see cref="None"/> when all is well.
/// </summary>
public sealed record Problem(string Title, string Text)
{
    public static Problem None { get; } = new("", "");
}
