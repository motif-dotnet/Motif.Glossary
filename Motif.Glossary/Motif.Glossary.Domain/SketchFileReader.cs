using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Motif.Glossary;

public static class SketchFileReader
{
    private static readonly JsonDocumentOptions Lenient = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Reads a sketch file, or says gently why it couldn't.
    /// </summary>
    public static bool TryRead(
        string json,
        [NotNullWhen(true)] out SketchFile? file,
        [NotNullWhen(false)] out string? problem)
    {
        file = null;
        problem = null;
        try
        {
            using var doc = JsonDocument.Parse(json, Lenient);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                problem = "It isn't a Motif Sketch file.";
                return false;
            }

            var items = Items(root).ToList();
            if (items.Count == 0 && Property(root, "schemaVersion") is null)
            {
                problem = "It isn't a Motif Sketch file: "
                    + "there are no elements or ports in it.";
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

    private static IEnumerable<SketchItem> Items(JsonElement root)
    {
        var context = Property(root, "contextDiagram");
        foreach (var element in ArrayItems(context, "elements"))
        {
            var (type, kind) = ElementType(Text(element, "type"));
            if (Item(element, type, kind) is { } item) yield return item;
        }
        foreach (var port in ArrayItems(Property(root, "sketch"), "ports"))
        {
            var side = Text(port, "side");
            var type = side is null ? "port" : $"{side} port";
            if (Item(port, type, Kind.Port) is { } item) yield return item;

            foreach (var adapter in ArrayItems(port, "adapters"))
            {
                var kind = Text(adapter, "kind");
                var adapterType = kind is null ? "adapter" : $"{kind} adapter";
                if (Item(adapter, adapterType, Kind.Adapter) is { } adapterItem)
                    yield return adapterItem;
            }
        }
    }

    private static SketchItem? Item(JsonElement element, string type, Kind kind) =>
        Text(element, "id") is { Length: > 0 } id
            ? new SketchItem(id, Text(element, "name") ?? id, type, kind)
            : null;

    private static (string Type, Kind Kind) ElementType(string? type) =>
        type switch
        {
            "system" => ("system", Kind.System),
            "primaryActor" => ("primary actor", Kind.Actor),
            "secondaryActor" => ("secondary actor", Kind.Actor),
            "externalSystem" => ("external system", Kind.ExternalSystem),
            "storage" => ("storage", Kind.Storage),
            _ => (type ?? "element", Kind.DomainConcept),
        };

    /// <summary>
    /// The items of an array property, or none if it isn't an array.
    /// </summary>
    private static IEnumerable<JsonElement> ArrayItems(
        JsonElement? parent, string name) =>
        parent is { } element
            && Property(element, name) is { ValueKind: JsonValueKind.Array } items
            ? items.EnumerateArray()
            : [];

    private static JsonElement? Property(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind != JsonValueKind.Null
            ? value
            : null;

    private static string? Text(JsonElement element, string name) =>
        Property(element, name) is { ValueKind: JsonValueKind.String } value
            ? value.GetString()
            : null;
}
