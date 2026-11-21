using Motif.Glossary.Adapters;

namespace Motif.Glossary.Fakes;

/// <summary>
/// A Local Files Fitting with no browser: the files the Reviewer will pick,
/// and what's dropped onto the page, set up by the spec. Picking with nothing
/// set up is closing the picker.
/// </summary>
public sealed class FakeLocalFiles : LocalFiles
{
    private IReadOnlyList<LocalFile> toPick = [];
    private Drop latest = new(0, []);

    public FakeLocalFiles WillPick(params LocalFile[] files)
    {
        toPick = files;
        return this;
    }

    /// <summary>
    /// Drops files onto the page.
    /// </summary>
    public FakeLocalFiles Drop(params LocalFile[] files)
    {
        latest = new Drop(latest.Number + 1, files);
        return this;
    }

    public Task<IReadOnlyList<LocalFile>> Pick()
    {
        var picked = toPick;
        toPick = [];
        return Task.FromResult(picked);
    }

    public Task<Drop> LatestDrop() => Task.FromResult(latest);
}
