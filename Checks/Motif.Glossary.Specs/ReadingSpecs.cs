namespace Motif.Glossary.Specs;

/// <summary>The "Proposed only" filter, and the corners of the format the reader has to cope with.</summary>
public class ReadingSpecs
{
    [Fact]
    public void Proposed_only_shows_just_the_proposed_terms()
    {
        var f = new Fixture();
        f.Tasks.Paste(Fixture.OwnGlossary);

        f.Tasks.ShowProposedOnly(true);

        f.Queries.ProposedOnly.Should().BeTrue();
        f.Queries.Groups.SelectMany(g => g.Terms).Select(t => t.Term.Name).Should().Equal(
            "Format note", "Repo link", "Review", "Sketch file", "Sketch item", "Sketch note", "Source");
        f.Queries.Groups.Select(g => g.Kind).Should().Equal(Kind.DomainConcept);

        f.Tasks.ShowProposedOnly(false);
        f.Queries.Groups.SelectMany(g => g.Terms).Should().HaveCount(22);
    }

    [Fact]
    public void Headings_inside_code_fences_are_not_terms()
    {
        var review = Fixture.Pasted("## Alpha\n\n```markdown\n## Not a term\n- **Kind:** actor\n```\n\n- **Kind:** actor\n");

        review.Terms.Should().ContainSingle().Which.Term.Kinds.Should().Equal(Kind.Actor);
    }

    [Fact]
    public void Definitions_keep_their_paragraphs_and_fields_can_be_in_any_case()
    {
        var review = Fixture.Pasted("## Alpha\n\nFirst line\ncarries on.\n\nSecond.\n\n* **kind**: Domain Concept\n- **Status:** *proposed*\n- **Owner:** Sam\n");

        var alpha = Fixture.Term(review, "Alpha").Term;
        alpha.Definition.Should().Equal("First line carries on.", "Second.");
        alpha.Kinds.Should().Equal(Kind.DomainConcept);
        alpha.Status.Should().Be(Status.Proposed);
        alpha.OtherFields.Should().ContainSingle().Which.Should().Be(new KeyValuePair<string, string>("Owner", "Sam"));
    }

    [Fact]
    public void Windows_line_endings_are_read_the_same()
    {
        var review = Fixture.Pasted(Fixture.OwnGlossary.Replace("\r\n", "\n").Replace("\n", "\r\n"));

        review.Terms.Should().HaveCount(22);
        review.NoteCount.Should().Be(0);
    }

    [Fact]
    public void Slugs_are_made_the_way_GitHub_makes_them()
    {
        Slug.Of("Sketch id").Should().Be("sketch-id");
        Slug.Of("Driving and driven").Should().Be("driving-and-driven");
        Slug.Of("`Files` port (driven)").Should().Be("files-port-driven");
    }

    [Fact]
    public void Inline_markdown_is_read_into_spans()
    {
        Inline.Parse("A `code` and **bold**, *italic* [link](#term).").Should().Equal(
            new InlineSpan(SpanKind.Text, "A "),
            new InlineSpan(SpanKind.Code, "code"),
            new InlineSpan(SpanKind.Text, " and "),
            new InlineSpan(SpanKind.Strong, "bold"),
            new InlineSpan(SpanKind.Text, ", "),
            new InlineSpan(SpanKind.Emphasis, "italic"),
            new InlineSpan(SpanKind.Text, " "),
            new InlineSpan(SpanKind.Link, "link", "#term"),
            new InlineSpan(SpanKind.Text, "."));
        Inline.PlainText("2 * 3 = 6").Should().Be("2 * 3 = 6");
    }
}
