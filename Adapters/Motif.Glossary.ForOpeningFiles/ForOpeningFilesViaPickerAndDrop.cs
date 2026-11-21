namespace Motif.Glossary.Adapters;

/// <summary>
/// Files picked with the file picker, or dropped onto the page, over the
/// <see cref="LocalFiles"/> Seam.
/// </summary>
public sealed class ForOpeningFilesViaPickerAndDrop(LocalFiles local)
    : ForOpeningFiles
{
    private int taken;

    public Task<IReadOnlyList<LocalFile>> Pick() => local.Pick();

    public async Task<IReadOnlyList<LocalFile>> TakeDropped()
    {
        var drop = await local.LatestDrop();
        // The page hears of every drop, files or not, so a drop of text
        // would otherwise open the files dropped before it again.
        if (drop.Number == taken) return [];
        taken = drop.Number;
        return drop.Files;
    }
}
