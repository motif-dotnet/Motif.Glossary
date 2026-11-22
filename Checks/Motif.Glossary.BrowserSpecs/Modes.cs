namespace Motif.Glossary.BrowserSpecs;

/// <summary>What the page is showing when it is measured.</summary>
public enum Showing
{
    /// <summary>The four ways in, with no glossary yet.</summary>
    Start,

    Example,

    /// <summary>A repo link, whose pill carries the repo's long name.</summary>
    RepoLink,

    Pasted,

    /// <summary>A repo whose sketch file couldn't be read, so a banner.</summary>
    BrokenSketch,
}

/// <summary>Opens the app in each mode it shows.</summary>
public sealed class Modes(Site site)
{
    public async Task<IPage> Open(IBrowserContext profile, Showing showing)
    {
        switch (showing)
        {
            case Showing.Start:
                return await site.Open(profile);
            case Showing.RepoLink:
                return await Reviewing(
                    await site.Open(profile, "#repo=" + Site.ThisRepo));
            case Showing.BrokenSketch:
                return await Reviewing(await site.Open(
                    profile, "#repo=" + Site.BrokenSketchRepo));
            case Showing.Pasted:
                var page = await site.Open(profile);
                await page.GetByTitle("Paste a glossary's Markdown")
                    .ClickAsync();
                await page.Locator("#paste-box")
                    .FillAsync(site.Read("motif/glossary.md"));
                await page.GetByRole(AriaRole.Dialog)
                    .GetByRole(AriaRole.Button, new() { Name = "Review" })
                    .ClickAsync();
                return await Reviewing(page);
            default:
                var example = await site.Open(profile);
                await example.GetByTitle(
                        "Motif Sketch's glossary, with its sketch file")
                    .ClickAsync();
                return await Reviewing(example);
        }
    }

    /// <summary>Waits for the glossary's review to show.</summary>
    public static async Task<IPage> Reviewing(IPage page)
    {
        await page.Locator(".review h1").WaitForAsync();
        return page;
    }
}
