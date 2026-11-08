namespace Motif.Glossary;

/// <summary>The bundled glossary: Motif Sketch's own, with its sketch file, compiled into the core.</summary>
public static class Example
{
    public const string GlossaryResource = "Motif.Glossary.example.glossary.md";
    public const string SketchFileResource = "Motif.Glossary.example.sketch.motifsketch.json";

    public const string Name = "Motif Sketch's glossary";

    public static string Glossary() => Read(GlossaryResource);
    public static string SketchFile() => Read(SketchFileResource);

    private static string Read(string resource)
    {
        using var stream = typeof(Example).Assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"The example's {resource} is missing from the build.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
