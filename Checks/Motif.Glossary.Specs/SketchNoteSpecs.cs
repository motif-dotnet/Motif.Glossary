namespace Motif.Glossary.Specs;

/// <summary>Sketch notes, from comparing the glossary with its sketch file (decision 8).</summary>
public class SketchNoteSpecs
{
    private const string Sketch = """
        {
          "schemaVersion": 1,
          "title": "Tiny",
          "contextDiagram": { "elements": [
            { "id": "e1", "type": "system", "name": "Tiny" },
            { "id": "e2", "type": "storage", "name": "Shelf" }
          ] },
          "sketch": { "ports": [
            { "id": "p1", "name": "User", "side": "driving", "adapters": [ { "id": "a1", "name": "Web App", "kind": "real" } ] }
          ] }
        }
        """;

    private const string Glossary = """
        # Glossary

        ## Tiny

        The system.

        - **Kind:** system
        - **Sketch:** `e1`

        ## User port

        - **Kind:** port
        - **Sketch:** `p1`, `gone`

        ## Web App

        - **Kind:** adapter
        - **Sketch:** `a1`
        """;

    private static Review Open(string glossary, string? sketch)
    {
        var f = new Fixture();
        f.Files.WillPick(sketch is null ? [Fixture.GlossaryFile(glossary)] : [Fixture.GlossaryFile(glossary), Fixture.SketchFileFile(sketch)]);
        // The fakes answer at once, so this never blocks.
        f.Tasks.OpenFiles().Wait();
        return f.Queries.Review!;
    }

    [Fact]
    public void A_sketch_id_no_longer_in_the_sketch_gets_a_note()
    {
        var review = Open(Glossary, Sketch);

        Fixture.Term(review, "User port").SketchNotes.Single().Should().Be(
            new SketchNote(SketchNoteKind.IdNotInSketch, "gone", "Sketch id gone isn't in the sketch file any more."));
        Fixture.Term(review, "User port").Sketch.Select(s => s.Item?.Name).Should().Equal("User", null);
    }

    [Fact]
    public void Elements_and_ports_with_no_term_are_listed_but_adapters_are_not_expected()
    {
        var review = Open(Glossary.Replace("- **Sketch:** `a1`", ""), Sketch);

        review.WithoutTerms.Should().Equal(new SketchNote(SketchNoteKind.NoTerm, "e2", "Shelf (storage) has no term."));
    }

    [Fact]
    public void An_adapter_id_resolves_too()
    {
        var review = Open(Glossary, Sketch);

        Fixture.Term(review, "Web App").Sketch.Single().Item!.Type.Should().Be("real adapter");
    }

    [Fact]
    public void Without_a_sketch_file_ids_stay_plain_and_there_are_no_sketch_notes()
    {
        var review = Open(Glossary, null);

        review.SketchFile.Should().BeNull();
        review.Terms.SelectMany(t => t.Sketch).Should().OnlyContain(s => s.Item == null);
        review.Terms.SelectMany(t => t.SketchNotes).Should().BeEmpty();
        review.WithoutTerms.Should().BeEmpty();
    }

    [Fact]
    public async Task A_sketch_file_that_cant_be_read_leaves_the_glossary_reviewed_without_it()
    {
        var f = new Fixture();
        f.Files.WillPick(Fixture.GlossaryFile(Glossary), Fixture.SketchFileFile("{ not json"));
        await f.Tasks.OpenFiles();

        f.Queries.Review!.Terms.Should().HaveCount(3);
        f.Queries.Review.SketchFile.Should().BeNull();
        f.Queries.SketchFileProblem.Should().StartWith("The sketch file couldn't be read");
        f.Queries.Source!.SketchFileName.Should().BeNull();
    }

    [Fact]
    public void The_sketch_id_field_can_say_which_id_is_which()
    {
        var review = Fixture.Pasted("## Clipboard\n\n- **Kind:** external system, port\n- **Sketch:** `c5a10a0a` (element), `d6b20c06` (port)\n");

        Fixture.Term(review, "Clipboard").Term.SketchIds.Should().Equal("c5a10a0a", "d6b20c06");
    }
}
