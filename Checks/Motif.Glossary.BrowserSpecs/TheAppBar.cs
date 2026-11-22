namespace Motif.Glossary.BrowserSpecs;

/// <summary>
/// At a phone's width, between the phone and narrow breakpoints, and on a
/// computer, in each mode the app shows, the app bar's content is no wider
/// than the bar and nothing in it overlaps.
/// </summary>
[Collection(SiteCollection.Name)]
public sealed class TheAppBar(Site site)
{
    public static TheoryData<int, Showing> Cases()
    {
        var cases = new TheoryData<int, Showing>();
        foreach (var width in new[] { 375, 637, 1280 })
            foreach (var showing in Enum.GetValues<Showing>())
                cases.Add(width, showing);
        return cases;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Fits_with_nothing_overlapping(int width, Showing showing)
    {
        await using var profile = await site.Profile(width);
        var page = await new Modes(site).Open(profile, showing);

        var bar = await Measuring.Measure(page, "header.appbar");

        bar.ContentWidth.Should().BeLessThanOrEqualTo(bar.Width);
        bar.Overlapping.Should().BeEmpty();
    }
}
