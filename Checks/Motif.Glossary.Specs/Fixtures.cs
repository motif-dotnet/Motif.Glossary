using Motif.Glossary.Adapters;
using Motif.Glossary.Fakes;

namespace Motif.Glossary.Specs;

/// <summary>The application over its real adapters, with only their Fittings faked, and the fixtures: this repo's motif/ folder and the Example (Motif Sketch's).</summary>
public sealed class Fixture
{
    public FakeGitHub GitHub { get; } = new();
    public FakeLocalFiles LocalFiles { get; } = new();
    public MotifGlossaryApplication App { get; }

    public Fixture() => App = new MotifGlossaryApplication(
        new ForOpeningReposViaGitHub(GitHub),
        new ForOpeningFilesViaPickerAndDrop(LocalFiles));

    public UserTasks Tasks => App;
    public UserQueries Queries => App;

    public static string OwnGlossary => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "glossary.md"));
    public static string OwnSketchFile => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "sketch.motifsketch.json"));

    public static LocalFile GlossaryFile(string text = "") => new("glossary.md", text.Length == 0 ? OwnGlossary : text);
    public static LocalFile SketchFileFile(string? json = null) => new("sketch.motifsketch.json", json ?? OwnSketchFile);

    /// <summary>The review of some pasted Markdown.</summary>
    public static Review Pasted(string markdown)
    {
        var f = new Fixture();
        f.Tasks.Paste(markdown);
        return f.Queries.Review ?? throw new InvalidOperationException(f.Queries.Problem.Text);
    }

    public static TermReview Term(Review review, string name) => review.Terms.Single(t => t.Term.Name == name);
}
