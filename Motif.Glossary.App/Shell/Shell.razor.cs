using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Motif.Suite;

namespace Motif.Glossary.App;

/// <summary>
/// The Web App's one page: the four ways in, then the Review. All the logic is
/// behind the User port.
/// </summary>
public partial class Shell : IDisposable
{
    private enum Menu { None, Tools, Theme }

    private enum Dialog { None, Paste, Repo }

    private bool started;
    private Menu menu;
    private Dialog dialog;
    private string theme = "system";
    private string pasteText = "";
    private string repoText = "";
    private int dragDepth;
    private bool copied;
    private bool helpOpen;
    private Links links = new(null, null, _ => { });
    private DotNetObjectReference<Shell>? self;
    private Review? shown;

    [Inject] private MotifGlossaryApplication App { get; set; } = null!;
    [Inject] private Browser Browser { get; set; } = null!;
    [Inject] private Toaster Toaster { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private string PageTitle => App.Source is { } source
        ? $"{source.Name} · Motif Glossary"
        : "Motif Glossary";

    private string? RepoLinkUrl => App.RepoLinkFragment is { } fragment
        ? BaseUrl() + "#" + fragment
        : null;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Start();
        }
        else if (shown != App.Review)
        {
            // A newly opened glossary starts at the top.
            shown = App.Review;
            Browser.ScrollToTop(".scroll");
        }
    }

    private async Task Start()
    {
        await Browser.Load();
        theme = Browser.Theme();
        Toaster.Changed += Refresh;
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
        // A repo review carries its repo link in the address bar; anything
        // else clears a stale one.
        if (App.RepoLinkFragment is { } fragment)
        {
            Browser.SetFragment(fragment);
        }
        else if (App.Problem == Problem.None
                 && RepoLink.TryRead(Browser.Fragment(), out _))
        {
            Browser.SetFragment(null);
        }
        // What went wrong is the outcome of a command, so it's a toast, as in
        // every Motif tool.
        if (App.Problem != Problem.None)
        {
            Toaster.Problem(App.Problem.Title, [App.Problem.Text]);
            App.DismissProblem();
        }
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
        dialog = Dialog.Paste;
        _ = FocusSoon("#paste-box");
    }

    private void ShowRepo()
    {
        dialog = Dialog.Repo;
        _ = FocusSoon("#repo-box");
    }

    private async Task FocusSoon(string selector)
    {
        await Task.Yield();
        StateHasChanged();
        await Task.Delay(1);
        Browser.Focus(selector);
    }

    private void CloseDialog() => dialog = Dialog.None;

    private void SubmitPaste()
    {
        App.Paste(pasteText);
        if (App.Problem == Problem.None)
        {
            dialog = Dialog.None;
            pasteText = "";
        }
        Opened();
    }

    private async Task SubmitRepo()
    {
        if (string.IsNullOrWhiteSpace(repoText)) return;
        dialog = Dialog.None;
        var opening = App.OpenRepo(repoText);
        StateHasChanged();
        await opening;
        if (App.Problem == Problem.None) repoText = "";
        Opened();
    }

    private void StartAgain()
    {
        // Back to the four ways in. The application keeps nothing else, so a
        // fresh one is simply a reload.
        Browser.SetFragment(null);
        Navigation.NavigateTo(Navigation.BaseUri, forceLoad: true);
    }

    private void ShowProposedOnly(bool value) => App.ShowProposedOnly(value);

    private async Task CopyRepoLink()
    {
        if (RepoLinkUrl is { } url) copied = await Browser.Copy(url);
    }

    private void ScrollTo(Term term) => Browser.ScrollTo("term-" + term.Slug);

    private void ScrollToGroup(string id) => Browser.ScrollTo(id);

    private void DragEnter() => dragDepth++;

    private void DragLeave() => dragDepth = Math.Max(0, dragDepth - 1);

    private async Task Drop()
    {
        dragDepth = 0;
        await App.OpenDroppedFiles();
        Opened();
    }

    private void Toggle(Menu name)
    {
        menu = menu == name ? Menu.None : name;
        helpOpen = false;
    }

    private void ToggleHelp()
    {
        helpOpen = !helpOpen;
        menu = Menu.None;
    }

    private void Refresh() => InvokeAsync(StateHasChanged);

    private void CloseMenus() => menu = Menu.None;

    private void SetTheme(string value)
    {
        theme = value;
        Browser.SetTheme(value);
        menu = Menu.None;
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

    public void Dispose()
    {
        Toaster.Changed -= Refresh;
        self?.Dispose();
    }
}
