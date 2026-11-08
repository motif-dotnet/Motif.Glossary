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

## Later (not v1)

- **What changed:** compare two versions (a branch or PR against main) and show added, renamed and removed terms. Agents mark new terms *proposed*, so the proposed filter covers most of this for now.
- **Editing:** agree terms, add Avoid words, save back to the file.
- **Private repos**, through GitHub sign-in.

## Still open

- [design/ideas.md](https://github.com/motif-dotnet/.github/blob/main/design/ideas.md) links #10 from the **Glossary in context** row (IDE hover text, Sketch highlighting), but this viewer is closer to the **Glossary editor** row. One of them needs rewording.
