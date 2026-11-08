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
    public void Other_headings_are_kept_as_definition_text()
    {
        var glossary = Fixture.Pasted("# Words\n\n## Alpha ##\n\nFirst.\n\n### Usage\n\nSecond.\n\n- **Kind:** actor\n").Glossary;

        glossary.Title.Should().Be("Words");
        var alpha = glossary.Terms.Single();
        alpha.Name.Should().Be("Alpha");
        alpha.Line.Should().Be(3);
        alpha.Definition.Should().Equal("First.", "Usage", "Second.");
    }

    [Fact]
    public void Only_the_first_top_level_heading_before_the_terms_is_the_title()
    {
        // No blank lines, and no newline at the end: headings and fields end the paragraph before them.
        var glossary = Fixture.Pasted(
            "### Draft\n# Words\nIntro.\n## Alpha\nFirst.\n### Usage\nSecond.\n- **Kind:** actor\nAfter the fields.\n# Not a title\nLast.").Glossary;

        glossary.Title.Should().Be("Words");
        glossary.Intro.Should().Equal("Draft", "Intro.");
        glossary.Terms.Single().Definition.Should().Equal("First.", "Usage", "Second.", "After the fields.", "Not a title", "Last.");
    }

    [Fact]
    public void A_fence_runs_until_its_own_marker()
    {
        var review = Fixture.Pasted("## Alpha\n\nBefore the fence.\n~~~\n```\n## Not a term\n- **Kind:** storage\n~~~\nAfter the fence.\n\n- **Kind:** actor\n");

        var alpha = review.Terms.Should().ContainSingle().Which.Term;
        alpha.Definition.Should().Equal("Before the fence.", "After the fence.");
        alpha.Kinds.Should().Equal(Kind.Actor);
    }

    [Fact]
    public void A_list_in_a_definition_keeps_one_paragraph_per_item()
    {
        var alpha = Fixture.Term(Fixture.Pasted("## Alpha\n\nOne of:\n- this\n1. or that\n\n- **Kind:** actor\n"), "Alpha").Term;

        alpha.Definition.Should().Equal("One of:", "- this", "1. or that");
    }

    [Fact]
    public void A_field_carries_on_onto_an_indented_line()
    {
        var alpha = Fixture.Term(Fixture.Pasted(
            "## Alpha\n\n- **Kind:** actor\n- **In code:** `Alpha`,\n  `AlphaReader`\n   \n  Indented paragraph.\n"), "Alpha").Term;

        alpha.InCode.Should().Be("`Alpha`, `AlphaReader`");
        alpha.Kinds.Should().Equal(Kind.Actor);
        alpha.Definition.Should().Equal("Indented paragraph.");
    }

    [Fact]
    public void A_kind_can_carry_a_note()
    {
        var review = Fixture.Pasted("## Alpha\n\n- **Kind:** Actor (the person), (mostly)\n");

        Fixture.Term(review, "Alpha").Term.KindTexts.Should().Equal("Actor");
        Fixture.Term(review, "Alpha").Term.Kinds.Should().Equal(Kind.Actor);
        review.NoteCount.Should().Be(0);
    }

    [Fact]
    public void The_review_names_its_groups_in_the_order_of_the_seven_kinds()
    {
        var kinds = new[] { "system", "actor", "external system", "storage", "port", "adapter", "domain concept" };
        var review = Fixture.Pasted(string.Concat(kinds.Select((k, i) => $"## Term {i}\n\n- **Kind:** {k}\n\n")) + "## Term 7\n");

        review.Groups.Select(g => g.Name).Should().Equal(
            "System", "Actors", "External systems", "Storage", "Ports", "Adapters", "Domain concepts", "No kind");
    }

    [Fact]
    public void Avoid_words_can_be_joined_with_and_quoted_or_noted_after_a_comma()
    {
        var alpha = Fixture.Term(Fixture.Pasted("## Alpha\n\n- **Kind:** actor\n- **Avoid:** primary and secondary, \"the thing\" (too vague), user, for a person\n"), "Alpha").Term;

        alpha.Avoid.Should().Equal(
            new AvoidWord("primary"), new AvoidWord("secondary"), new AvoidWord("the thing", "too vague"), new AvoidWord("user", "for a person"));
    }

    [Fact]
    public void Avoid_words_split_only_at_the_top_level()
    {
        var alpha = Fixture.Term(Fixture.Pasted(
            "## Alpha\n\n- **Kind:** actor\n- **Avoid:** for loop, `a, b`, smiley :) face (a note), user (that's the port) (see below), and demo, or test, \"the thing\" itself, draft (unfinished\n"),
            "Alpha").Term;

        alpha.Avoid.Should().Equal(
            new AvoidWord("for loop"),
            new AvoidWord("a, b"),
            new AvoidWord("smiley :) face", "a note"),
            new AvoidWord("user", "that's the port; see below"),
            new AvoidWord("demo"),
            new AvoidWord("test"),
            new AvoidWord("the thing", "itself"),
            new AvoidWord("draft (unfinished"));
    }

    [Fact]
    public void A_status_is_proposed_by_its_first_word()
    {
        var review = Fixture.Pasted(
            "## Alpha\n\n- **Kind:** actor\n- **Status:** proposed (by an agent)\n\n## Beta\n\n- **Kind:** actor\n- **Status:** not proposed\n\n## Delta\n\n- **Kind:** actor\n- **Status:**\n\n## Gamma\n\n- **Kind:** actor\n");

        Fixture.Term(review, "Alpha").Term.Status.Should().Be(Status.Proposed);
        Fixture.Term(review, "Alpha").Term.StatusText.Should().Be("proposed (by an agent)");
        Fixture.Term(review, "Beta").Term.Status.Should().Be(Status.Agreed);
        Fixture.Term(review, "Delta").Term.Status.Should().Be(Status.Agreed);
        Fixture.Term(review, "Gamma").Term.Status.Should().Be(Status.Agreed);
        Fixture.Term(review, "Gamma").Term.StatusText.Should().BeNull();
    }

    [Fact]
    public void See_also_resolves_by_name_or_by_anchor()
    {
        var review = Fixture.Pasted(
            "## Alpha\n\n- **Kind:** actor\n- **See also:** Beta\n\n## Beta\n\n- **Kind:** actor\n\n## Delta\n\n- **Kind:** actor\n- **See also:** [Beta](https://example.com/beta)\n\n## Gamma\n\n- **Kind:** actor\n- **See also:** [the second one](#beta)\n");

        Fixture.Term(review, "Alpha").SeeAlso.Single().Should().Be(new SeeAlsoRef(new SeeAlsoLink("Beta", null), review.Glossary.Terms[1]));
        Fixture.Term(review, "Delta").Term.SeeAlso.Should().Equal(new SeeAlsoLink("Beta", null));
        Fixture.Term(review, "Gamma").SeeAlso.Single().Term!.Name.Should().Be("Beta");
        review.NoteCount.Should().Be(0);
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

    [Fact]
    public void Inline_markdown_copes_with_escapes_and_markers_that_dont_close()
    {
        Inline.PlainText(@"\\\`\*\_\[\]\(\)\#\- \a\").Should().Be(@"\`*_[]()#- \a\");
        Inline.Parse("`code`").Should().Equal(new InlineSpan(SpanKind.Code, "code"));
        Inline.PlainText("A `code` span").Should().Be("A code span");
        foreach (var text in new[] { "****", "2 * 3 * 4", "a `", "see [a]", "[a] (b)", "[a(b)", "[a](b" })
            Inline.PlainText(text).Should().Be(text);
    }

    [Fact]
    public void A_link_ends_at_its_own_bracket_and_paren()
    {
        Inline.Parse("[f(x)](#f) and [b](#y)").Should().Equal(
            new InlineSpan(SpanKind.Link, "f(x)", "#f"),
            new InlineSpan(SpanKind.Text, " and "),
            new InlineSpan(SpanKind.Link, "b", "#y"));
    }
}
