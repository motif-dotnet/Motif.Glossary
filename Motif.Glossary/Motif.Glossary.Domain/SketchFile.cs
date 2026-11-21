namespace Motif.Glossary;

/// <summary>
/// An element, port or adapter in a sketch file: all Motif Glossary reads of
/// it.
/// </summary>
/// <param name="Type">
/// As Motif Sketch says it: "primary actor", "external system", "driving
/// port", "real adapter".
/// </param>
/// <param name="Kind">The glossary kind a term for it would have.</param>
public sealed record SketchItem(string Id, string Name, string Type, Kind Kind)
{
    /// <summary>
    /// Elements and ports are expected to have a term; adapters usually
    /// aren't.
    /// </summary>
    public bool ExpectsTerm => Kind != Kind.Adapter;
}

/// <summary>
/// A Motif Sketch project (motif/sketch.motifsketch.json), as far as ids,
/// names and types go.
/// </summary>
public sealed record SketchFile(string? Title, IReadOnlyList<SketchItem> Items)
{
    public const string FileName = "sketch.motifsketch.json";

    public SketchItem? Find(string id) =>
        Items.FirstOrDefault(item =>
            string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
}
