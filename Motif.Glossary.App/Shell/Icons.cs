using Microsoft.AspNetCore.Components;

namespace Motif.Glossary.App;

/// <summary>The app's few icons, drawn as inline SVG in Motif Sketch's style.</summary>
public static class Icons
{
    private static MarkupString I(string paths, string extra = "") =>
        new($"<svg class=\"i\" viewBox=\"0 0 24 24\" aria-hidden=\"true\"{extra}>{paths}</svg>");

    /// <summary>The Motif mark, drawn in the theme's colours: the suite's brand/mark.svg, as in index.html's splash.</summary>
    public static readonly MarkupString Mark = new(
        "<svg class=\"mark\" viewBox=\"0 0 24 24\" aria-hidden=\"true\">" +
        "<rect x=\"2.9\" y=\"15.74\" width=\"5.36\" height=\"5.36\" rx=\"0.96\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\"/>" +
        "<rect x=\"8.42\" y=\"8.42\" width=\"7.16\" height=\"7.16\" rx=\"1.86\" fill=\"currentColor\"/>" +
        "<rect x=\"14.84\" y=\"2\" width=\"7.16\" height=\"7.16\" rx=\"1.86\" fill=\"var(--accent)\"/></svg>");

    public static readonly MarkupString Chevron = I("<path d=\"M6 9l6 6 6-6\"/>", " style=\"width:12px\"");
    public static readonly MarkupString Open = I("<path d=\"M3 7a2 2 0 012-2h4l2 2h8a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2z\"/>");
    public static readonly MarkupString Paste = I("<rect x=\"6\" y=\"4\" width=\"12\" height=\"17\" rx=\"2\"/><path d=\"M9 4V3h6v1M9 10h6M9 14h6M9 18h3\"/>");
    public static readonly MarkupString Repo = I("<path d=\"M5 4a2 2 0 012-2h12v16H7a2 2 0 00-2 2zM5 20a2 2 0 002 2h12v-4\"/><path d=\"M9 6h6\"/>");
    public static readonly MarkupString Example = I("<path d=\"M4 5a2 2 0 012-2h5v18H6a2 2 0 01-2-2zM13 3h5a2 2 0 012 2v14a2 2 0 01-2 2h-5z\"/>");
    public static readonly MarkupString Link = I("<path d=\"M10 14a5 5 0 007 0l3-3a5 5 0 00-7-7l-1 1\"/><path d=\"M14 10a5 5 0 00-7 0l-3 3a5 5 0 007 7l1-1\"/>");
    public static readonly MarkupString Copy = I("<rect x=\"8\" y=\"8\" width=\"13\" height=\"13\" rx=\"2\"/><path d=\"M4 16V5a2 2 0 012-2h11\"/>");
    public static readonly MarkupString Sun = I("<circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 2v2M12 20v2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M2 12h2M20 12h2M4.9 19.1l1.4-1.4M17.7 6.3l1.4-1.4\"/>");
    public static readonly MarkupString Moon = I("<path d=\"M20 14.5A8 8 0 019.5 4a8 8 0 1010.5 10.5z\"/>");
    public static readonly MarkupString Monitor = I("<rect x=\"3\" y=\"4\" width=\"18\" height=\"12\" rx=\"2\"/><path d=\"M8 20h8M12 16v4\"/>");
    public static readonly MarkupString Note = I("<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 8v5M12 16h.01\"/>");
    public static readonly MarkupString Close = new("<span aria-hidden=\"true\">✕</span>");
}
