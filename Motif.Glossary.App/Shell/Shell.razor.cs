using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Motif.Glossary.App;

/// <summary>The Web App's one page: the four ways in, then the Review. All the logic is behind the User port.</summary>
public partial class Shell : IDisposable
{
    private bool started;
    private string? menu;
    private string? dialog;
    private string theme = "system";
    private string pasteText = "";
    private string repoText = "";
    private int dragDepth;
    private bool copied;
    private Links links = new(null, null, _ => { });
    private DotNetObjectReference<Shell>? self;
    private Review? shown;

    private string PageTitle => App.Source is { } s ? $"{s.Name} · Motif Glossary" : "Motif Glossary";

    private string? RepoLinkUrl => App.RepoLinkFragment is { } f ? BaseUrl() + "#" + f : null;

    private MarkupString ThemeIcon => theme switch { "light" => Icons.Sun, "dark" => Icons.Moon, _ => Icons.Monitor };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            // A newly opened glossary starts at the top.
            if (shown != App.Review)
            {
                shown = App.Review;
                Browser.ScrollToTop(".scroll");
            }
            return;
        }
        await Browser.Load();
        theme = Browser.Theme();
        self = DotNetObjectReference.Create(this);
        Browser.ListenForFragment(self);
        started = true;
        StateHasChanged();
        await OpenFragment(Browser.Fragment());
    }

    [JSInvokable]
    public async Task OnFragment(string fragment) => await OpenFragment(fragment);

    private async Task OpenFragment(string? fragment)
    {
        if (!RepoLink.TryRead(fragment, out _)) return;
        var opening = App.OpenLink(fragment);
        StateHasChanged(); // show the progress bar while GitHub is read
        await opening;
        Opened();
    }

    private void Opened()
    {
        links = new Links(App.Review, App.Source, ScrollTo);
        copied = false;
        // A repo review carries its repo link in the address bar; anything else clears a stale one.
        if (App.RepoLinkFragment is { } fragment) Browser.SetFragment(fragment);
        else if (App.Problem is null && RepoLink.TryRead(Browser.Fragment(), out _)) Browser.SetFragment(null);
        StateHasChanged();
    }

    private async Task OpenFiles()
    {
        await App.OpenFiles();
        Opened();
    }

    private void OpenExample()
    {
        App.OpenExample();
        Opened();
    }

    private void ShowPaste()
    {
        dialog = "paste";
        _ = FocusSoon("#paste-box");
    }

    private void ShowRepo()
    {
        dialog = "repo";
        _ = FocusSoon("#repo-box");
    }

    private async Task FocusSoon(string selector)
    {
        await Task.Yield();
        StateHasChanged();
        await Task.Delay(1);
        Browser.Focus(selector);
    }

    private void CloseDialog() => dialog = null;

    private void SubmitPaste()
    {
        App.Paste(pasteText);
        if (App.Problem is null)
        {
            dialog = null;
            pasteText = "";
        }
        Opened();
    }

    private async Task SubmitRepo()
    {
        if (string.IsNullOrWhiteSpace(repoText)) return;
        dialog = null;
        var opening = App.OpenRepo(repoText);
        StateHasChanged();
        await opening;
        if (App.Problem is null) repoText = "";
        Opened();
    }

    private void StartAgain()
    {
        // Back to the four ways in. The application keeps nothing else, so a fresh one is simply a reload.
        Browser.SetFragment(null);
        if (App.Review is not null || App.Problem is not null) Navigation.NavigateTo(Navigation.BaseUri, forceLoad: true);
    }

    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private void ShowProposedOnly(bool value) => App.ShowProposedOnly(value);

    private async Task CopyRepoLink()
    {
        if (RepoLinkUrl is { } url) copied = await Browser.Copy(url);
    }

    private void ScrollTo(Term term) => Browser.ScrollToTerm("term-" + term.Slug);

    private void ScrollToGroup(string id) => Browser.ScrollToTerm(id);

    private void DragEnter() => dragDepth++;

    private void DragLeave() => dragDepth = Math.Max(0, dragDepth - 1);

    private async Task Drop()
    {
        dragDepth = 0;
        await App.OpenDroppedFiles();
        Opened();
    }

    private void Toggle(string name) => menu = menu == name ? null : name;

    private void CloseMenus() => menu = null;

    private void SetTheme(string value)
    {
        theme = value;
        Browser.SetTheme(value);
        menu = null;
    }

    private string BaseUrl()
    {
        var url = Browser.PageUrl();
        var hash = url.IndexOf('#');
        return hash < 0 ? url : url[..hash];
    }

    private static MarkupString SourceIcon(SourceKind kind) => kind switch
    {
        SourceKind.Repo => Icons.Repo,
        SourceKind.Pasted => Icons.Paste,
        SourceKind.Example => Icons.Example,
        _ => Icons.Open,
    };

    private static string SourceTitle(Source s) => s.Kind switch
    {
        SourceKind.Repo => $"motif/glossary.md from {s.Name}'s default branch" + (s.SketchFileName is null ? ", with no sketch file" : ", with its sketch file"),
        SourceKind.Example => "The Example: Motif Sketch's glossary and sketch file",
        SourceKind.Pasted => "Pasted Markdown" + (s.SketchFileName is { } f ? $", with {f}" : ""),
        _ => s.Name + (s.SketchFileName is { } sketch ? $", with {sketch}" : ""),
    };

    public void Dispose() => self?.Dispose();
}
