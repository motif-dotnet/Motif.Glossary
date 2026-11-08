namespace Motif.Glossary.Specs;

/// <summary>Format notes: gentle, per term, never blocking (decision 7).</summary>
public class FormatNoteSpecs
{
    private static IEnumerable<FormatNoteKind> NotesOn(Review review, string term) =>
        Fixture.Term(review, term).FormatNotes.Select(n => n.Kind);

    [Fact]
    public void A_term_with_no_kind_gets_a_note_and_its_own_group()
    {
        var review = Fixture.Pasted("## Alpha\n\nThe first.\n");

        NotesOn(review, "Alpha").Should().Equal(FormatNoteKind.NoKind);
        review.Groups.Single().Kind.Should().BeNull();
        review.Groups.Single().Name.Should().Be("No kind");
    }

    [Fact]
    public void A_kind_outside_the_seven_gets_a_note()
    {
        var review = Fixture.Pasted("## Alpha\n\n- **Kind:** widget\n");

        var note = Fixture.Term(review, "Alpha").FormatNotes.Single();
        note.Kind.Should().Be(FormatNoteKind.UnknownKind);
        note.Subject.Should().Be("widget");
    }

    [Fact]
    public void More_than_two_kinds_gets_a_note()
    {
        var review = Fixture.Pasted("## Alpha\n\n- **Kind:** storage, port, adapter\n");

        NotesOn(review, "Alpha").Should().Equal(FormatNoteKind.TooManyKinds);
        review.Groups.Should().HaveCount(3);
    }

    [Fact]
    public void Two_kinds_are_fine()
    {
        var review = Fixture.Pasted("## Alpha\n\n- **Kind:** storage, port\n");

        NotesOn(review, "Alpha").Should().BeEmpty();
    }

    [Fact]
    public void A_see_also_that_doesnt_resolve_gets_a_note()
    {
        var review = Fixture.Pasted("## Alpha\n\n- **Kind:** actor\n- **See also:** [Beta](#beta), [Gamma](#gamma)\n\n## Beta\n\n- **Kind:** actor\n");

        var note = Fixture.Term(review, "Alpha").FormatNotes.Single();
        note.Kind.Should().Be(FormatNoteKind.UnresolvedSeeAlso);
        note.Subject.Should().Be("Gamma");
        Fixture.Term(review, "Alpha").SeeAlso[0].Term!.Name.Should().Be("Beta");
    }

    [Fact]
    public void An_avoid_word_that_is_itself_a_term_gets_a_note()
    {
        var review = Fixture.Pasted("## Alpha\n\n- **Kind:** actor\n- **Avoid:** beta, delta\n\n## Beta\n\n- **Kind:** actor\n");

        var note = Fixture.Term(review, "Alpha").FormatNotes.Single();
        note.Kind.Should().Be(FormatNoteKind.AvoidWordIsTerm);
        note.Subject.Should().Be("beta");
    }

    [Fact]
    public void Headings_out_of_alphabetical_order_get_a_note()
    {
        var review = Fixture.Pasted("## Beta\n\n- **Kind:** actor\n\n## Alpha\n\n- **Kind:** actor\n\n## Gamma\n\n- **Kind:** actor\n");

        NotesOn(review, "Beta").Should().BeEmpty();
        Fixture.Term(review, "Alpha").FormatNotes.Single().Should().Be(
            new FormatNote(FormatNoteKind.OutOfOrder, "Beta", "Out of alphabetical order: it comes after “Beta”."));
        NotesOn(review, "Gamma").Should().BeEmpty();
    }

    [Fact]
    public void Notes_never_stop_a_glossary_being_reviewed()
    {
        var f = new Fixture();
        f.Tasks.Paste("## Zed\n\n## Alpha\n\n- **Kind:** thing, other, more\n- **See also:** [Nowhere](#nowhere)\n");

        f.Queries.Problem.Should().BeNull();
        f.Queries.Review!.NoteCount.Should().Be(7);
    }
}
