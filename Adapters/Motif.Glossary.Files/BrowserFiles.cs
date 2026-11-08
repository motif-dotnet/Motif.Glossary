using Microsoft.JSInterop;

namespace Motif.Glossary.Adapters;

/// <summary>
/// Files through the browser's file picker, and drag and drop onto the page (wwwroot/files.js).
/// The module catches drops itself, before the page's own handler runs, and keeps their text for
/// <see cref="TakeDropped"/>; so call <see cref="Start"/> once at startup.
/// </summary>
public sealed class BrowserFiles(IJSRuntime js) : Files, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> module =
        new(() => js.InvokeAsync<IJSObjectReference>("import", "./_content/Motif.Glossary.Files/files.js").AsTask());

    /// <summary>Starts listening for drops onto the page.</summary>
    public async Task Start() => await (await module.Value).InvokeVoidAsync("listen");

    public async Task<IReadOnlyList<LocalFile>> Pick() =>
        ToFiles(await (await module.Value).InvokeAsync<string[][]>("pick"));

    public async Task<IReadOnlyList<LocalFile>> TakeDropped() =>
        ToFiles(await (await module.Value).InvokeAsync<string[][]>("takeDropped"));

    private static List<LocalFile> ToFiles(string[][]? files) =>
        files?.Where(f => f.Length == 2).Select(f => new LocalFile(f[0], f[1])).ToList() ?? [];

    public async ValueTask DisposeAsync()
    {
        if (module.IsValueCreated) await (await module.Value).DisposeAsync();
    }
}
