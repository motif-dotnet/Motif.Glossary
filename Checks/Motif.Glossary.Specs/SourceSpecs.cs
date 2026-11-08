namespace Motif.Glossary.Specs;

/// <summary>The four ways in: files opened or dropped, pasted Markdown, a public repo, and the Example (decision 5).</summary>
public class SourceSpecs
{
    [Fact]
    public async Task Opening_a_glossary_and_its_sketch_file()
    {
        var f = new Fixture();
        f.Files.WillPick(Fixture.SketchFileFile(), Fixture.GlossaryFile());

        await f.Tasks.OpenFiles();

        f.Queries.Source.Should().Be(new Source(SourceKind.Files, "glossary.md", "sketch.motifsketch.json"));
        f.Queries.Review!.SketchFile.Should().NotBeNull();
    }

    [Fact]
    public async Task Cancelling_the_file_picker_changes_nothing()
    {
        var f = new Fixture();
        f.Tasks.OpenExample();

        await f.Tasks.OpenFiles();

        f.Queries.Source!.Kind.Should().Be(SourceKind.Example);
        f.Queries.Problem.Should().BeNull();
    }

    [Fact]
    public async Task Dropping_a_glossary()
    {
        var f = new Fixture();
        f.Files.Drop(Fixture.GlossaryFile());

        await f.Tasks.OpenDroppedFiles();

        f.Queries.Review!.Terms.Should().HaveCount(22);
        f.Queries.Review.SketchFile.Should().BeNull();
    }

    [Fact]
    public async Task A_sketch_file_dropped_on_its_own_joins_the_open_glossary()
    {
        var f = new Fixture();
        f.Tasks.Paste(Fixture.OwnGlossary);
        f.Files.Drop(Fixture.SketchFileFile());

        await f.Tasks.OpenDroppedFiles();

        f.Queries.Source.Should().Be(new Source(SourceKind.Pasted, "Pasted Markdown", "sketch.motifsketch.json"));
        Fixture.Term(f.Queries.Review!, "Reviewer").Sketch.Single().Item!.Name.Should().Be("Reviewer");
    }

    [Fact]
    public async Task Dropping_files_with_no_glossary_says_so()
    {
        var f = new Fixture();
        f.Files.Drop(new LocalFile("picture.png", ""));

        await f.Tasks.OpenDroppedFiles();

        f.Queries.Review.Should().BeNull();
        f.Queries.Problem.Should().StartWith("Drop a glossary.md");
    }

    [Fact]
    public void Pasting_a_glossary()
    {
        var f = new Fixture();

        f.Tasks.Paste(Fixture.OwnGlossary);

        f.Queries.Source!.Kind.Should().Be(SourceKind.Pasted);
        f.Queries.Review!.Terms.Should().HaveCount(22);
    }

    [Fact]
    public void Pasting_nothing_or_no_terms_says_so_and_keeps_the_review()
    {
        var f = new Fixture();
        f.Tasks.OpenExample();

        f.Tasks.Paste("   ");
        f.Queries.Problem.Should().Contain("nothing to review");

        f.Tasks.Paste("# Just a title\n\nNo terms.");
        f.Queries.Problem.Should().Contain("No terms");
        f.Queries.Source!.Kind.Should().Be(SourceKind.Example);

        f.Tasks.DismissProblem();
        f.Queries.Problem.Should().BeNull();
    }

    [Fact]
    public async Task Opening_a_public_repo_gives_a_repo_link()
    {
        var f = new Fixture();
        f.Repos.Add("motif-dotnet/Motif.Glossary", Fixture.OwnGlossary, Fixture.OwnSketchFile);

        await f.Tasks.OpenRepo("motif-dotnet/Motif.Glossary");

        f.Queries.Source!.Name.Should().Be("motif-dotnet/Motif.Glossary");
        f.Queries.Review!.SketchFile.Should().NotBeNull();
        f.Queries.RepoLinkFragment.Should().Be("repo=motif-dotnet/Motif.Glossary");
        f.Queries.Loading.Should().BeFalse();
    }

    [Theory]
    [InlineData("https://github.com/motif-dotnet/Motif.Glossary")]
    [InlineData("github.com/motif-dotnet/Motif.Glossary/blob/main/motif/glossary.md")]
    [InlineData(" motif-dotnet/Motif.Glossary.git ")]
    public async Task A_repo_can_be_given_as_a_GitHub_URL(string given)
    {
        var f = new Fixture();
        f.Repos.Add("motif-dotnet/Motif.Glossary", Fixture.OwnGlossary);

        await f.Tasks.OpenRepo(given);

        f.Repos.Reads.Should().Equal(new RepoName("motif-dotnet", "Motif.Glossary"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("motif-dotnet")]
    [InlineData("a/b/c")]
    [InlineData("-bad/name")]
    public async Task Something_that_isnt_a_repo_is_not_read(string given)
    {
        var f = new Fixture();

        await f.Tasks.OpenRepo(given);

        f.Repos.Reads.Should().BeEmpty();
        f.Queries.Problem.Should().Contain("isn't a GitHub repo");
    }

    [Fact]
    public async Task A_repo_with_no_glossary_says_so()
    {
        var f = new Fixture();

        await f.Tasks.OpenRepo("someone/elsewhere");

        f.Queries.Review.Should().BeNull();
        f.Queries.Problem.Should().Be("someone/elsewhere has no motif/glossary.md on its default branch, or it isn't a public repo.");
    }

    [Fact]
    public async Task GitHub_out_of_reach_says_so()
    {
        var f = new Fixture { Repos = { Unreachable = true } };

        await f.Tasks.OpenRepo("motif-dotnet/Motif.Sketch");

        f.Queries.Problem.Should().StartWith("GitHub couldn't be reached");
        f.Queries.Loading.Should().BeFalse();
    }

    [Fact]
    public async Task A_repo_link_opens_straight_into_the_review()
    {
        var f = new Fixture();
        f.Repos.Add("motif-dotnet/Motif.Glossary", Fixture.OwnGlossary);

        (await f.Tasks.OpenLink("#repo=motif-dotnet/Motif.Glossary")).Should().BeTrue();

        f.Queries.Review!.Terms.Should().HaveCount(22);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("#example")]
    [InlineData("#repo=")]
    public async Task Other_fragments_are_not_repo_links(string? fragment)
    {
        var f = new Fixture();

        (await f.Tasks.OpenLink(fragment)).Should().BeFalse();

        f.Repos.Reads.Should().BeEmpty();
    }

    [Fact]
    public async Task Only_a_repo_review_has_a_repo_link()
    {
        var f = new Fixture();
        f.Repos.Add("motif-dotnet/Motif.Glossary", Fixture.OwnGlossary);
        await f.Tasks.OpenRepo("motif-dotnet/Motif.Glossary");

        f.Tasks.OpenExample();

        f.Queries.RepoLinkFragment.Should().BeNull();
    }
}
