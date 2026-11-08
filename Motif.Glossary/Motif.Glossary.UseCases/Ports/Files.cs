namespace Motif.Glossary;

/// <summary>A file on the Reviewer's computer: its name and its text.</summary>
public sealed record LocalFile(string Name, string Text);

/// <summary>Reads a glossary.md the Reviewer opens or drops in, and a sketch file with it if there is one.</summary>
public interface Files
{
    /// <summary>Shows the file picker. Empty if the Reviewer cancels.</summary>
    Task<IReadOnlyList<LocalFile>> Pick();

    /// <summary>The files of the most recent drop onto the page, read once. Empty if there were none.</summary>
    Task<IReadOnlyList<LocalFile>> TakeDropped();
}
