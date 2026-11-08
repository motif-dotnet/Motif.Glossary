namespace Motif.Glossary.Fakes;

/// <summary>Files the Reviewer will pick or has dropped, set up by the spec. Picking with nothing set up is a cancel.</summary>
public sealed class FakeFiles : Files
{
    private IReadOnlyList<LocalFile> toPick = [];
    private IReadOnlyList<LocalFile> dropped = [];

    public FakeFiles WillPick(params LocalFile[] files)
    {
        toPick = files;
        return this;
    }

    public FakeFiles Drop(params LocalFile[] files)
    {
        dropped = files;
        return this;
    }

    public Task<IReadOnlyList<LocalFile>> Pick()
    {
        var picked = toPick;
        toPick = [];
        return Task.FromResult(picked);
    }

    public Task<IReadOnlyList<LocalFile>> TakeDropped()
    {
        var files = dropped;
        dropped = [];
        return Task.FromResult(files);
    }
}
