namespace Motif.Glossary.Specs;

/// <summary>The two real glossaries: this repo's own, opened as files, and the Example, which is Motif Sketch's.</summary>
public class ReviewingTheFixtures
{
    private static Review Own()
    {
        var f = new Fixture();
        f.Files.WillPick(Fixture.GlossaryFile(), Fixture.SketchFileFile());
        f.Tasks.OpenFiles().GetAwaiter().GetResult();
        return f.Queries.Review!;
    }

    private static Review TheExample()
    {
        var f = new Fixture();
        f.Tasks.OpenExample();
        return f.Queries.Review!;
    }

    [Fact]
    public void Motif_Glossarys_own_glossary_has_its_twenty_two_terms_in_order()
    {
        var review = Own();

        review.Glossary.Title.Should().Be("Glossary");
        review.Glossary.Intro.Should().HaveCount(3);
        review.Terms.Should().HaveCount(22);
        review.Terms[0].Term.Name.Should().Be("Avoid word");
        review.Terms[^1].Term.Name.Should().Be("User port");
    }

    [Fact]
    public void Every_field_of_a_term_is_read()
    {
        var files = Fixture.Term(Own(), "Files").Term;

        files.Definition.Should().ContainSingle().Which.Should().StartWith("The port that reads a glossary file");
        files.Kinds.Should().Equal(Kind.Port);
        files.InCode.Should().Be("`Files`; real adapter `BrowserFiles`");
        files.SketchIds.Should().Equal("9e1a0c03");
        files.SeeAlso.Should().Equal(new SeeAlsoLink("Glossary Files", "glossary-files"));
        files.Status.Should().Be(Status.Agreed);
    }

    [Fact]
    public void Avoid_words_keep_their_notes()
    {
        var review = Own();

        Fixture.Term(review, "Glossary").Term.Avoid.Should().Equal(
            new AvoidWord("dictionary"), new AvoidWord("language"), new AvoidWord("ubiquitous language", "ambiguous in .NET"));
        Fixture.Term(review, "Reviewer").Term.Avoid.Select(a => a.Word).Should().Equal("user", "viewer", "reader");
        Fixture.Term(review, "Sketch file").Term.Avoid.Should().Equal(
            new AvoidWord("sketch", "on its own; in Motif Sketch that's the ports-and-adapters view"),
            new AvoidWord("project", "Motif Sketch's word for the file; here, \"project\" means the app a glossary belongs to"));
        Fixture.Term(review, "Term").Term.Avoid.Should().Equal(
            new AvoidWord("word", "for the whole entry; the heading is the term's name"),
            new AvoidWord("definition", "that's one part of a term"));
    }

    [Fact]
    public void Proposed_terms_are_picked_out()
    {
        var review = Own();

        review.Terms.Where(t => t.Term.IsProposed).Select(t => t.Term.Name).Should().Equal(
            "Format note", "Repo link", "Review", "Sketch file", "Sketch item", "Sketch note", "Source");
        review.ProposedCount.Should().Be(7);
    }

    [Fact]
    public void Terms_are_grouped_by_the_seven_kinds_in_order()
    {
        var review = Own();

        review.Groups.Select(g => g.Kind).Should().Equal(
            Kind.System, Kind.Actor, Kind.ExternalSystem, Kind.Storage, Kind.Port, Kind.DomainConcept);
        review.Groups.Single(g => g.Kind == Kind.Port).Terms.Select(t => t.Term.Name).Should().Equal("Files", "Repos", "User port");
    }

    [Fact]
    public void Sketch_ids_resolve_to_names_in_the_sketch_file()
    {
        var review = Own();

        Fixture.Term(review, "Motif Glossary").Sketch.Single().Item!.Name.Should().Be("Motif.Glossary");
        Fixture.Term(review, "Reviewer").Sketch.Single().Item!.Type.Should().Be("primary actor");
        Fixture.Term(review, "User port").Sketch.Single().Item.Should().Be(new SketchItem("9e1a0c01", "User", "driving port", Kind.Port));
    }

    [Fact]
    public void Motif_Glossarys_own_glossary_follows_the_format_and_its_sketch()
    {
        var review = Own();

        review.Terms.SelectMany(t => t.FormatNotes).Should().BeEmpty();
        review.Terms.SelectMany(t => t.SketchNotes).Should().BeEmpty();
        review.WithoutTerms.Should().BeEmpty();
        review.Terms.SelectMany(t => t.SeeAlso).Should().OnlyContain(s => s.Term != null);
    }

    [Fact]
    public void The_example_is_Motif_Sketchs_glossary_with_its_sketch_file()
    {
        var f = new Fixture();
        f.Tasks.OpenExample();

        f.Queries.Source.Should().Be(new Source(SourceKind.Example, "Motif Sketch's glossary", "sketch.motifsketch.json"));
        f.Queries.Review!.Terms.Should().HaveCount(37);
        f.Queries.Review.SketchFile!.Title.Should().Be("Motif Sketch");
        f.Queries.RepoLinkFragment.Should().BeNull();
    }

    [Fact]
    public void A_term_with_two_kinds_is_in_both_groups_with_both_sketch_ids_resolved()
    {
        var review = TheExample();
        var clipboard = Fixture.Term(review, "Clipboard");

        clipboard.Term.Kinds.Should().Equal(Kind.ExternalSystem, Kind.Port);
        review.Groups.Single(g => g.Kind == Kind.ExternalSystem).Terms.Should().Contain(clipboard);
        review.Groups.Single(g => g.Kind == Kind.Port).Terms.Should().Contain(clipboard);
        clipboard.Sketch.Select(s => s.Item!.Type).Should().Equal("external system", "driven port");
    }

    [Fact]
    public void The_example_follows_the_format_and_its_sketch()
    {
        var review = TheExample();

        review.Terms.Where(t => t.NoteCount > 0)
            .Select(t => $"{t.Term.Name}: {string.Join(", ", t.FormatNotes.Select(n => n.Text).Concat(t.SketchNotes.Select(n => n.Text)))}")
            .Should().BeEmpty();
        review.WithoutTerms.Should().BeEmpty();
    }

    [Fact]
    public void A_qualified_avoid_word_that_is_a_term_gets_no_note()
    {
        var review = TheExample();

        // "project, on its own" and "fake (a fake stands in ...)" are explained clashes, not slips.
        Fixture.Term(review, "Planned project").Term.Avoid.First().Should().Be(new AvoidWord("project", "on its own"));
        Fixture.Term(review, "Checks adapter").Term.Avoid.Single().Word.Should().Be("fake");
        Fixture.Term(review, "Sketch").Term.Avoid.Select(a => a.Word).Should().Equal("hexagon", "sketch");
    }
}
