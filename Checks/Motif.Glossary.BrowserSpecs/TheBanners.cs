namespace Motif.Glossary.BrowserSpecs;

/// <summary>
/// The same layout rule for the banner under the app bar: in each mode that
/// shows one, its content is no wider than it and nothing in it overlaps.
/// </summary>
[Collection(SiteCollection.Name)]
public sealed class TheBanners(Site site)
{
    public static TheoryData<int, Showing> Cases()
    {
        var cases = new TheoryData<int, Showing>();
        foreach (var width in new[] { 375, 637, 1280 })
            cases.Add(width, Showing.BrokenSketch);
        return cases;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Fits_with_nothing_overlapping(int width, Showing showing)
    {
        await using var profile = await site.Profile(width);
        var page = await new Modes(site).Open(profile, showing);

        await Assertions.Expect(page.Locator(".banner")).ToBeVisibleAsync();
        var banner = await Measuring.Measure(page, ".banner");

        banner.ContentWidth.Should().BeLessThanOrEqualTo(banner.Width);
        banner.Overlapping.Should().BeEmpty();
    }
}
