using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Motif.Glossary.BrowserSpecs;

[CollectionDefinition(Name)]
public sealed class SiteCollection : ICollectionFixture<Site>
{
    public const string Name = "Site";
}

/// <summary>
/// The app as it's deployed: published in Release (trimmed), served as
/// static files, and opened in a real browser. Each page gets a fresh
/// browser profile, so preferences start empty. Nothing leaves the machine:
/// GitHub has only the repos below, made from this repo's own files, and
/// every other request (such as the visit counter) is blocked.
/// </summary>
public sealed class Site : IAsyncLifetime
{
    /// <summary>This repo, with its glossary and sketch file.</summary>
    public const string ThisRepo = "motif-dotnet/Motif.Glossary";

    /// <summary>This repo's glossary, with a sketch file that isn't one.</summary>
    public const string BrokenSketchRepo = "motif-dotnet/Broken.Sketch";

    private const string GitHub = "https://raw.githubusercontent.com/";
    private const string SketchFile = "motif/sketch.motifsketch.json";

    private readonly string repo = RepoRoot();
    private WebApplication? server;
    private IPlaywright? playwright;
    private IBrowser? browser;

    public string Url { get; private set; } = "";

    public async Task InitializeAsync()
    {
        var site = Path.Combine(AppContext.BaseDirectory, "site");
        await Publish(site);
        server = Serve(Path.Combine(site, "wwwroot"));
        await server.StartAsync();
        Url = server.Services.GetRequiredService<IServer>()
            .Features.GetRequiredFeature<IServerAddressesFeature>()
            .Addresses.Single() + "/";

        playwright = await Playwright.CreateAsync();
        // The Edge every Windows machine has, so there's nothing to download.
        // MOTIF_BROWSER picks another channel, such as "chrome".
        browser = await playwright.Chromium.LaunchAsync(new()
        {
            Channel = Environment.GetEnvironmentVariable("MOTIF_BROWSER")
                ?? "msedge",
        });
    }

    /// <summary>A browser profile of its own, at this window width.</summary>
    public async Task<IBrowserContext> Profile(int width = 1280)
    {
        var profile = await browser!.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = 812 },
        });
        await profile.RouteAsync("**/*", Route);
        return profile;
    }

    /// <summary>Opens the app at this fragment and waits for it to start.</summary>
    public async Task<IPage> Open(
        IBrowserContext profile, string fragment = "")
    {
        var page = await profile.NewPageAsync();
        await Go(page, fragment);
        return page;
    }

    public async Task Go(IPage page, string fragment = "")
    {
        await page.GotoAsync(Url + fragment);
        await Started(page);
    }

    public static Task Started(IPage page) =>
        page.Locator("header.appbar").WaitForAsync();

    /// <summary>The text of this file in this repo, such as the glossary.</summary>
    public string Read(string file) =>
        File.ReadAllText(Path.Combine(repo, file));

    private async Task Route(IRoute route)
    {
        var url = route.Request.Url;
        if (url.StartsWith(Url, StringComparison.Ordinal))
            await route.ContinueAsync();
        else if (url.StartsWith(GitHub, StringComparison.Ordinal))
            await FromGitHub(route, url[GitHub.Length..]);
        else
            await route.AbortAsync();
    }

    /// <summary>
    /// A file at owner/name/HEAD/path, or a 404 as GitHub gives for a file
    /// or repo that isn't there.
    /// </summary>
    private async Task FromGitHub(IRoute route, string path)
    {
        const string Head = "/HEAD/";
        var at = path.IndexOf(Head, StringComparison.Ordinal);
        var text = at < 0
            ? null
            : FileOnGitHub(path[..at], path[(at + Head.Length)..]);
        if (text is null)
            await route.FulfillAsync(new() { Status = 404 });
        else
            await route.FulfillAsync(new()
            {
                Body = text,
                Headers = new Dictionary<string, string>
                {
                    ["Access-Control-Allow-Origin"] = "*",
                },
            });
    }

    private string? FileOnGitHub(string name, string path)
    {
        if (name == BrokenSketchRepo && path == SketchFile)
            return "{ this isn't a sketch file";
        if (name is not (ThisRepo or BrokenSketchRepo)) return null;
        var file = Path.Combine(repo, path);
        return File.Exists(file) ? File.ReadAllText(file) : null;
    }

    private async Task Publish(string output)
    {
        var app = Path.Combine(repo, "Motif.Glossary.App");
        using var publish = Process.Start(new ProcessStartInfo(
            "dotnet", ["publish", app, "-c", "Release", "-o", output])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        })!;
        var log = publish.StandardOutput.ReadToEndAsync();
        var errors = publish.StandardError.ReadToEndAsync();
        await publish.WaitForExitAsync();
        if (publish.ExitCode != 0)
            throw new InvalidOperationException(
                "Publishing the app failed:\n" + await log + await errors);
    }

    private static WebApplication Serve(string root)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        var app = builder.Build();
        var files = new PhysicalFileProvider(root);
        app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = files });
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = files,
            ContentTypeProvider = new FileExtensionContentTypeProvider(),
            ServeUnknownFileTypes = true,
        });
        return app;
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(dir.FullName, "Motif.Glossary.sln")))
            dir = dir.Parent ?? throw new InvalidOperationException(
                "The specs must run inside the Motif.Glossary repo.");
        return dir.FullName;
    }

    public async Task DisposeAsync()
    {
        if (browser is not null) await browser.DisposeAsync();
        playwright?.Dispose();
        if (server is not null) await server.DisposeAsync();
    }
}
