using Microsoft.JSInterop;

namespace Motif.Glossary.App;

/// <summary>The Web App's own browser plumbing (wwwroot/js/app.js). Synchronous where WebAssembly allows it.</summary>
public sealed class Browser(IJSRuntime js)
{
    private IJSInProcessObjectReference? module;

    public async Task Load() =>
        module ??= await js.InvokeAsync<IJSInProcessObjectReference>("import", "./js/app.js");

    private IJSInProcessObjectReference M => module ?? throw new InvalidOperationException("app.js isn't loaded yet.");

    public string Fragment() => M.Invoke<string>("fragment");
    public string PageUrl() => M.Invoke<string>("pageUrl");
    public void SetFragment(string? fragment) => M.InvokeVoid("setFragment", fragment);
    public void ListenForFragment<T>(DotNetObjectReference<T> target) where T : class => M.InvokeVoid("listenForFragment", target);
    public void ScrollToTerm(string id) => M.InvokeVoid("scrollToTerm", id);
    public async Task<bool> Copy(string text) => await M.InvokeAsync<bool>("copy", text);
    public void Focus(string selector) => M.InvokeVoid("focus", selector);
    public void ScrollToTop(string selector) => M.InvokeVoid("scrollToTop", selector);
    public string Theme() => M.Invoke<string>("theme");
    public void SetTheme(string theme) => M.InvokeVoid("setTheme", theme);
}
