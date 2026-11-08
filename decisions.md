# Motif Glossary: decisions so far

Motif Glossary shows a project's `motif/glossary.md` in a form that's easy to look at and review. It starts as a viewer ([motif-dotnet/.github#10](https://github.com/motif-dotnet/.github/issues/10)) and may grow into the Glossary editor. The file format is in the [glossary design note](https://github.com/motif-dotnet/.github/blob/main/design/glossary.md).

| # | Topic | Decision |
|---|---|---|
| 1 | Name | Shown to people as "Motif Glossary". The repo, solution and namespaces use `Motif.Glossary`, as the design note proposed. |
| 2 | Form | A standalone Blazor WebAssembly app on GitHub Pages, like Motif Sketch: no server and no accounts. Not a view inside Sketch, because the glossary doesn't depend on a sketch. |
| 3 | Parser | Reading `glossary.md` lives in a plain .NET library, so the `dotnet motif` CLI's glossary check ([#4](https://github.com/motif-dotnet/.github/issues/4)) can reuse it later. |
| 4 | Read-only | v1 views and reviews only. The file is the product: changes are made in the file and reviewed in pull requests. Writing the file back, without disturbing what people wrote, is the editor's job, later. |
| 5 | Sources | Four ways in: open or drop a local `glossary.md`; paste Markdown; a public GitHub repo (`owner/name`), fetching `motif/glossary.md` from its default branch, with a shareable `#repo=owner/name` link that opens straight into the review; and a bundled example, Motif Sketch's glossary. Private repos would need GitHub sign-in, so not v1. |
| 6 | Reviewing | The whole glossary at a glance: terms grouped by the seven kinds, each showing its Avoid words, status, In code and Sketch ids. *Proposed* terms are picked out, with a "Proposed only" filter. |
| 7 | Format notes | Gentle notes per term, never blocking: no kind, a kind outside the seven, more than two kinds, a See also that doesn't resolve, an Avoid word that is itself a term, headings out of alphabetical order. |
| 8 | Sketch | Optional. When `motif/sketch.motifsketch.json` is there (fetched alongside from a repo, or dropped in with the glossary), Sketch ids show as the element or port's name, ids no longer in the sketch get a note, and sketch elements and ports with no entry are listed. It reads only ids, names and types. Without a sketch, ids show as plain text. |
| 9 | Built the Motif way | The repo gets its own `motif/` folder first: a sketch drafted for Motif Sketch, the plan and diagrams exported from it, and its own glossary. The solution is stubbed from the plan, as Motif.Sketch was. |
| 10 | Reader | The glossary reader is hand-written, not Markdig. The format is a fixed, line-based shape (a `#` title, `##` per term, a bold-labelled field list), so a small reader is enough; the core keeps no package references for the CLI to inherit, and the download stays smaller. It never fails: whatever doesn't fit is kept as definition text, and the notes point it out. It lives in `Motif.Glossary.Domain`, with the sketch-file reader, the notes and the Review. |
| 11 | Avoid words | An Avoid word can carry a note: in brackets ("user (that's the port)") or after a comma ("project, on its own"; "word, for the whole entry"). "primary and secondary" is two words. An Avoid word that is itself a term gets a format note only when it has no note: a note means the glossary already explains the clash, as Motif Sketch's "Checks adapter" does for "fake". |
| 12 | Sketch items | Ids, names and types are read for elements, ports and adapters, so a Sketch id pointing at an adapter resolves too. Only elements and ports are listed when no term has their id: most adapters are named after a technology and don't need a term. |
| 13 | Composition root | The Web App references the two adapter projects as well as UseCases, because it's the composition root (as in Motif Sketch). The plan's stub stage listed UseCases only, with no composition root yet. |
| 14 | Shared look | Motif Glossary first copied Motif Sketch's look by hand, and the two drifted ([motif-dotnet/.github#12](https://github.com/motif-dotnet/.github/issues/12)). Both now use the suite's web kit ([brand/web](https://github.com/motif-dotnet/.github/tree/main/brand/web)), copied in by `scripts/sync-suite.sh`: `css/motif.css`, `wwwroot/fonts/` and `Suite/` (namespace `Motif.Suite`). Don't edit those copies; change the kit and sync. A workflow reports drift without blocking a deploy. So, as in Sketch: the mark and name open the tool switcher, and Start again is its own command; the theme menu has the "this browser only" note; there's a Help dialog; a problem opening a glossary is a toast, while a sketch file that couldn't be read stays a banner because it lasts as long as the review; code is monospace in the text colour, not orange-brown; labels hide below 900 px with `.wide`; and the page has link-preview tags and a social preview card. |

## Later (not v1)

- **What changed:** compare two versions (a branch or PR against main) and show added, renamed and removed terms. Agents mark new terms *proposed*, so the proposed filter covers most of this for now.
- **Editing:** agree terms, add Avoid words, save back to the file.
- **Private repos**, through GitHub sign-in.

## Still open

- [design/ideas.md](https://github.com/motif-dotnet/.github/blob/main/design/ideas.md) links #10 from the **Glossary in context** row (IDE hover text, Sketch highlighting), but this viewer is closer to the **Glossary editor** row. One of them needs rewording.
