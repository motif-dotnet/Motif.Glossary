using Motif.Glossary.Fakes;

namespace Motif.Glossary.Specs;

/// <summary>The application wired to fakes, and the fixtures: this repo's motif/ folder and the Example (Motif Sketch's).</summary>
public sealed class Fixture
{
    public FakeRepos Repos { get; } = new();
    public FakeFiles Files { get; } = new();
    public MotifGlossaryApplication App { get; }

    public Fixture() => App = new MotifGlossaryApplication(Repos, Files);

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
        return f.Queries.Review ?? throw new InvalidOperationException(f.Queries.Problem);
    }

    public static TermReview Term(Review review, string name) => review.Terms.Single(t => t.Term.Name == name);
}
