namespace Motif.Glossary.BrowserSpecs;

/// <summary>Choices kept in the browser, across reloads.</summary>
[Collection(SiteCollection.Name)]
public sealed class Preferences(Site site)
{
    [Fact]
    public async Task The_theme_is_kept_after_a_reload()
    {
        await using var profile = await site.Profile();
        var page = await site.Open(profile);
        await page.GetByTitle("Theme").ClickAsync();
        await page.GetByRole(AriaRole.Menuitemradio, new() { Name = "Dark" })
            .ClickAsync();

        await site.Go(page);

        await Assertions.Expect(page.Locator("html"))
            .ToHaveAttributeAsync("data-theme", "dark");
        await page.GetByTitle("Theme").ClickAsync();
        await Assertions.Expect(page.GetByRole(
                AriaRole.Menuitemradio, new() { Name = "Dark" }))
            .ToHaveAttributeAsync("aria-checked", "true");
    }
}
