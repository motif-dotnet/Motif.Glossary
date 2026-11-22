using System.Text.RegularExpressions;

namespace Motif.Glossary.BrowserSpecs;

/// <summary>A repo link in the address bar: shared, it opens the review.</summary>
[Collection(SiteCollection.Name)]
public sealed class Linking(Site site)
{
    [Fact]
    public async Task A_repo_link_opens_straight_into_the_review()
    {
        await using var profile = await site.Profile();

        var page = await site.Open(profile, "#repo=" + Site.ThisRepo);

        await Assertions.Expect(page.Locator(".review h1"))
            .ToHaveTextAsync("Glossary");
        await Assertions.Expect(page.Locator(".pill"))
            .ToContainTextAsync(Site.ThisRepo);
        await Assertions.Expect(page.Locator(".stats"))
            .ToContainTextAsync("Sketch file: Motif Glossary");
    }

    [Fact]
    public async Task Reviewing_a_repo_puts_its_link_in_the_address()
    {
        await using var profile = await site.Profile();
        var page = await site.Open(profile);

        await page.GetByLabel("GitHub repo, as owner/name")
            .FillAsync(Site.ThisRepo);
        await page.GetByRole(AriaRole.Button,
            new() { Name = "Review", Exact = true }).ClickAsync();

        await Assertions.Expect(page).ToHaveURLAsync(
            new Regex(Regex.Escape("#repo=" + Site.ThisRepo) + "$"));
    }
}
