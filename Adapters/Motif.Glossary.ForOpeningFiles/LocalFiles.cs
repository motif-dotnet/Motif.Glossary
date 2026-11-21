namespace Motif.Glossary.Adapters;

/// <summary>
/// The Local Files Seam: what opening files needs from the Reviewer's
/// computer, one call each. Every Fitting must behave as these comments say.
/// </summary>
public interface LocalFiles
{
    /// <summary>
    /// Shows the file picker, which takes several files at once, and reads
    /// each file picked. Empty when the Reviewer closes it.
    /// </summary>
    Task<IReadOnlyList<LocalFile>> Pick();

    /// <summary>
    /// The latest drop of files onto the page, read, as often as it's asked
    /// for. Dropping something that isn't files, such as text, isn't a drop
    /// of files. Before the first, drop 0, with no files.
    /// </summary>
    Task<Drop> LatestDrop();
}
