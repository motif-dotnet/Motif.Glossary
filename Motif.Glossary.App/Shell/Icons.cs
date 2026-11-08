using Microsoft.AspNetCore.Components;
using Motif.Suite;

namespace Motif.Glossary.App;

/// <summary>The app's few icons, drawn with the suite's <see cref="SuiteIcons.Icon"/> so they match its own (in Suite/SuiteIcons.cs).</summary>
public static class Icons
{
    private static MarkupString I(string paths, string extra = "") => SuiteIcons.Icon(paths, extra);

    public static readonly MarkupString Open = I("<path d=\"M3 7a2 2 0 012-2h4l2 2h8a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2z\"/>");
    public static readonly MarkupString Paste = I("<rect x=\"6\" y=\"4\" width=\"12\" height=\"17\" rx=\"2\"/><path d=\"M9 4V3h6v1M9 10h6M9 14h6M9 18h3\"/>");
    public static readonly MarkupString Repo = I("<path d=\"M5 4a2 2 0 012-2h12v16H7a2 2 0 00-2 2zM5 20a2 2 0 002 2h12v-4\"/><path d=\"M9 6h6\"/>");
    public static readonly MarkupString Example = I("<path d=\"M4 5a2 2 0 012-2h5v18H6a2 2 0 01-2-2zM13 3h5a2 2 0 012 2v14a2 2 0 01-2 2h-5z\"/>");
    public static readonly MarkupString Note = I("<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 8v5M12 16h.01\"/>");
    public static readonly MarkupString Back = I("<path d=\"M9 14L4 9l5-5\"/><path d=\"M4 9h11a5 5 0 010 10h-4\"/>");
}
