using Motif.Glossary.Adapters;
using Motif.Glossary.Fakes;

namespace Motif.Glossary.Specs;

/// <summary>
/// The application over its real adapters, with only their Fittings faked,
/// and the fixtures: this repo's motif/ folder and the Example (Motif
/// Sketch's).
/// </summary>
public sealed class Fixture
{
    public FakeGitHub GitHub { get; } = new();
    public FakeLocalFiles LocalFiles { get; } = new();
    public MotifGlossaryApplication App { get; }

    public Fixture() => App = new MotifGlossaryApplication(
        new ForOpeningReposViaGitHub(GitHub),
        new ForOpeningFilesViaPickerAndDrop(LocalFiles));

    public ForDoing Tasks => App;
    public ForSeeing Queries => App;

    public static string OwnGlossary => Read("glossary.md");
    public static string OwnSketchFile => Read("sketch.motifsketch.json");

    /// <summary>
    /// This repo's own glossary, as a file on the Reviewer's computer.
    /// </summary>
    public static LocalFile LocalGlossary() => LocalGlossary(OwnGlossary);

    public static LocalFile LocalGlossary(string markdown) =>
        new(Glossary.FileName, markdown);

    /// <summary>
    /// This repo's own sketch file, as a file on the Reviewer's computer.
    /// </summary>
    public static LocalFile LocalSketchFile() => LocalSketchFile(OwnSketchFile);

    public static LocalFile LocalSketchFile(string json) =>
        new(SketchFile.FileName, json);

    /// <summary>
    /// The review of some pasted Markdown.
    /// </summary>
    public static Review Pasted(string markdown)
    {
        var f = new Fixture();
        f.Tasks.Paste(markdown);
        return f.Queries.Review
            ?? throw new InvalidOperationException(f.Queries.Problem.Text);
    }

    public static TermReview Term(Review review, string name) =>
        review.Terms.Single(t => t.Term.Name == name);

    private static string Read(string fixture) =>
        File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "Fixtures", fixture));
}
