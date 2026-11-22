namespace Motif.Glossary.BrowserSpecs;

/// <summary>Filled in by Playwright, so it needs setters.</summary>
public sealed class Strip
{
    public double Width { get; set; }
    public double ContentWidth { get; set; }
    public string[] Overlapping { get; set; } = [];
}

/// <summary>Measures a strip of items laid out along the page.</summary>
public static class Measuring
{
    /// <summary>
    /// The element's widths, and each pair of its visible children that
    /// overlap, named by their text or tooltip. A child squeezed narrower
    /// than its content reaches as far as its content does, since that's
    /// what shows.
    /// </summary>
    public static Task<Strip> Measure(IPage page, string selector) =>
        page.Locator(selector).EvaluateAsync<Strip>("""
            bar => {
              const reach = e => {
                const r = e.getBoundingClientRect();
                const right = Math.max(r.right, r.left + e.scrollWidth);
                return { left: r.left, right, top: r.top, bottom: r.bottom,
                         width: r.width, height: r.height };
              };
              const items = [...bar.children]
                .filter(e => !e.classList.contains("spacer"))
                .map(e => ({ e, r: reach(e) }))
                .filter(i => i.r.width > 0 && i.r.height > 0);
              const name = e => [e.title, e.textContent,
                  e.getAttribute("aria-label"), e.className]
                .map(s => (s ?? "").trim()).find(s => s).slice(0, 30);
              const overlapping = [];
              for (let a = 0; a < items.length; a++)
                for (let b = a + 1; b < items.length; b++) {
                  const [p, q] = [items[a].r, items[b].r];
                  if (p.right - 0.5 > q.left && q.right - 0.5 > p.left
                      && p.bottom - 0.5 > q.top && q.bottom - 0.5 > p.top)
                    overlapping.push(
                      name(items[a].e) + " / " + name(items[b].e));
                }
              return {
                width: bar.clientWidth,
                contentWidth: bar.scrollWidth,
                overlapping,
              };
            }
            """);
}
