using Microsoft.JSInterop;

namespace Motif.Glossary.Adapters;

/// <summary>
/// The browser's Local Files Fitting: its file picker, and drops onto the
/// page (wwwroot/files.js). The module catches drops itself, before the
/// page's own handler runs, so call <see cref="Listen"/> once at startup.
/// </summary>
public sealed class JsInteropLocalFiles(IJSRuntime js)
    : LocalFiles, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> module = new(() => js
        .InvokeAsync<IJSObjectReference>(
            "import", "./_content/Motif.Glossary.ForOpeningFiles/files.js")
        .AsTask());

    /// <summary>
    /// Starts listening for drops onto the page.
    /// </summary>
    public async Task Listen() =>
        await (await module.Value).InvokeVoidAsync("listen");

    public async Task<IReadOnlyList<LocalFile>> Pick() =>
        await (await module.Value).InvokeAsync<LocalFile[]>("pick");

    public async Task<Drop> LatestDrop() =>
        await (await module.Value).InvokeAsync<Drop>("latestDrop");

    public async ValueTask DisposeAsync()
    {
        if (module.IsValueCreated) await (await module.Value).DisposeAsync();
    }
}
