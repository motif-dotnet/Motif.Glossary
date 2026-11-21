using Microsoft.JSInterop;

namespace Motif.Glossary.App;

/// <summary>
/// The Web App's own browser plumbing (wwwroot/js/app.js). Synchronous where
/// WebAssembly allows it.
/// </summary>
public sealed class Browser(IJSRuntime js)
{
    private IJSInProcessObjectReference? module;

    public async Task Load() =>
        module ??= await js.InvokeAsync<IJSInProcessObjectReference>(
            "import", "./js/app.js");

    private IJSInProcessObjectReference Module =>
        module ?? throw new InvalidOperationException("app.js isn't loaded yet.");

    public string Fragment() => Module.Invoke<string>("fragment");

    public string PageUrl() => Module.Invoke<string>("pageUrl");

    public void SetFragment(string? fragment) =>
        Module.InvokeVoid("setFragment", fragment);

    public void ListenForFragment<T>(DotNetObjectReference<T> target)
        where T : class =>
        Module.InvokeVoid("listenForFragment", target);

    /// <summary>
    /// Scrolls to the element with this id, a term or a group, and flashes it.
    /// </summary>
    public void ScrollTo(string id) => Module.InvokeVoid("scrollToTerm", id);

    public async Task<bool> Copy(string text) =>
        await Module.InvokeAsync<bool>("copy", text);

    public void Focus(string selector) => Module.InvokeVoid("focus", selector);

    public void ScrollToTop(string selector) =>
        Module.InvokeVoid("scrollToTop", selector);

    public string Theme() => Module.Invoke<string>("theme");

    public void SetTheme(string theme) => Module.InvokeVoid("setTheme", theme);
}
