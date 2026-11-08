using System.Text.Json;

namespace Motif.Glossary;

/// <summary>An element, port or adapter in a sketch file: all Motif Glossary reads of it.</summary>
/// <param name="Type">As Motif Sketch says it: "primary actor", "external system", "driving port", "real adapter".</param>
/// <param name="Kind">The glossary kind a term for it would have.</param>
public sealed record SketchItem(string Id, string Name, string Type, Kind Kind)
{
    /// <summary>Elements and ports are expected to have a term; adapters usually aren't.</summary>
    public bool ExpectsTerm => Kind != Kind.Adapter;
}

/// <summary>A Motif Sketch project (motif/sketch.motifsketch.json), as far as ids, names and types go.</summary>
public sealed record SketchFile(string? Title, IReadOnlyList<SketchItem> Items)
{
    public SketchItem? Find(string id) =>
        Items.FirstOrDefault(i => string.Equals(i.Id, id, StringComparison.OrdinalIgnoreCase));
}

public static class SketchFileReader
{
    /// <summary>Reads a sketch file, or says gently why it couldn't.</summary>
    public static bool TryRead(string json, out SketchFile? file, out string? problem)
    {
        file = null;
        problem = null;
        try
        {
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                problem = "It isn't a Motif Sketch file.";
                return false;
            }

            var items = new List<SketchItem>();
            if (Property(root, "contextDiagram") is { } context && Property(context, "elements") is { ValueKind: JsonValueKind.Array } elements)
            {
                foreach (var e in elements.EnumerateArray())
                {
                    var (type, kind) = ElementType(Text(e, "type"));
                    Add(items, e, type, kind);
                }
            }
            if (Property(root, "sketch") is { ValueKind: JsonValueKind.Object } sketch && Property(sketch, "ports") is { ValueKind: JsonValueKind.Array } ports)
            {
                foreach (var p in ports.EnumerateArray())
                {
                    var side = Text(p, "side");
                    Add(items, p, side is null ? "port" : $"{side} port", Kind.Port);
                    if (Property(p, "adapters") is { ValueKind: JsonValueKind.Array } adapters)
                    {
                        foreach (var a in adapters.EnumerateArray())
                        {
                            var kind = Text(a, "kind");
                            Add(items, a, kind is null ? "adapter" : $"{kind} adapter", Kind.Adapter);
                        }
                    }
                }
            }

            if (items.Count == 0 && Property(root, "schemaVersion") is null)
            {
                problem = "It isn't a Motif Sketch file: there are no elements or ports in it.";
                return false;
            }
            file = new SketchFile(Text(root, "title"), items);
            return true;
        }
        catch (JsonException e)
        {
            problem = $"It isn't valid JSON ({e.Message.Split('.')[0]}).";
            return false;
        }
    }

    private static void Add(List<SketchItem> items, JsonElement e, string type, Kind kind)
    {
        if (Text(e, "id") is not { Length: > 0 } id) return;
        items.Add(new SketchItem(id, Text(e, "name") ?? id, type, kind));
    }

    private static (string Type, Kind Kind) ElementType(string? type) => type switch
    {
        "system" => ("system", Kind.System),
        "primaryActor" => ("primary actor", Kind.Actor),
        "secondaryActor" => ("secondary actor", Kind.Actor),
        "externalSystem" => ("external system", Kind.ExternalSystem),
        "storage" => ("storage", Kind.Storage),
        _ => (type ?? "element", Kind.DomainConcept),
    };

    private static JsonElement? Property(JsonElement e, string name) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null ? value : null;

    private static string? Text(JsonElement e, string name) =>
        Property(e, name) is { ValueKind: JsonValueKind.String } value ? value.GetString() : null;
}
