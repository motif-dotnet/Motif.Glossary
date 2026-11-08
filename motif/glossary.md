# Glossary

The words this project uses, and what they mean. People and agents use these terms in code, docs, issues and discussion, and avoid the words listed under **Avoid**. A new concept gets an entry, marked *proposed*, in the same change that introduces it. An entry with no status is agreed.

The format is described in the [Motif Glossary design note](https://github.com/motif-dotnet/.github/blob/main/design/glossary.md). **Sketch** gives the id of the element or port in Motif Glossary's own design ([sketch.motifsketch.json](sketch.motifsketch.json)) that the term comes from.

This app is about glossaries, so several terms here name parts of the glossary format itself.

## Avoid word

A word listed under a term's **Avoid** field: a synonym not to use for that term. The part of a glossary that stops names drifting. It can carry a note, in brackets or after a comma ("project, on its own"), saying why or when it's avoided.

- **Kind:** domain concept
- **Avoid:** synonym, banned word
- **In code:** `AvoidWord`
- **See also:** [Term](#term)

## Example

The bundled glossary: Motif Sketch's own, with its sketch file. Shown when the Reviewer picks it, so there's something to look at straight away.

- **Kind:** domain concept
- **Avoid:** sample, demo
- **In code:** `Example`, `SourceKind.Example`

## Field

One bold-labelled line under a term: **Kind**, **Avoid**, **In code**, **Status**, **Sketch** or **See also**. Every field is optional.

- **Kind:** domain concept
- **Avoid:** property, attribute
- **See also:** [Term](#term)

## Files

The port that reads a glossary file the Reviewer opens or drops in, and a sketch file with it if there is one.

- **Kind:** port
- **In code:** `Files`; real adapter `BrowserFiles`
- **Sketch:** `9e1a0c03`
- **See also:** [Glossary Files](#glossary-files)

## Format note

A gentle note on a term that doesn't follow the glossary format: no kind, a kind outside the seven, more than two kinds, a See also that doesn't resolve, an Avoid word that is itself a term (unless its note explains the clash), or a heading out of alphabetical order. Notes never block anything.

- **Kind:** domain concept
- **Avoid:** error, warning, lint (notes report; they don't fail)
- **In code:** `FormatNote`, `FormatNotes`
- **Status:** proposed
- **See also:** [Sketch note](#sketch-note)

## GitHub

Where public repos are read from: a repo's `motif/glossary.md` and, if there is one, its sketch file, from the default branch. Also the name of the real adapter on the Repos port.

- **Kind:** external system
- **In code:** `GitHubRepos`
- **Sketch:** `9e1a0a03`
- **See also:** [Repos](#repos)

## Glossary

A project's agreed words, kept in `motif/glossary.md`: one term per `##` heading, in alphabetical order.

- **Kind:** domain concept
- **Avoid:** dictionary, language and ubiquitous language (ambiguous in .NET)
- **In code:** `Glossary`; read by `GlossaryReader`
- **See also:** [Term](#term)

## Glossary Files

Where a glossary on the Reviewer's computer comes from: a local `glossary.md`, opened or dropped in, with the sketch file beside it if there is one.

- **Kind:** storage
- **Sketch:** `9e1a0a04`
- **See also:** [Files](#files)

## Kind

Where a term sits. One of seven: *system*, *actor*, *external system*, *storage*, *port* and *adapter* come from the sketch; *domain concept* covers everything else. A term can have two kinds, never more. The review groups terms by kind.

- **Kind:** domain concept
- **Avoid:** category, type (that's an element's type in Motif Sketch)
- **In code:** `Kind`, `Kinds`

## Motif Glossary

This app: it shows a project's glossary at a glance for review. Also the system on its own context diagram. It starts as a viewer and may grow into an editor, keeping the same name.

- **Kind:** system
- **Avoid:** glossary viewer, Glossary app
- **In code:** the root namespace `Motif.Glossary`
- **Sketch:** `9e1a0a01`

## Repo link

A link ending `#repo=owner/name` that opens straight into a review of that public repo's glossary. It carries only the repo's name, so it always shows the latest glossary on the default branch.

- **Kind:** domain concept
- **Avoid:** snapshot link (Motif Sketch's word for a link that carries the whole project), share link
- **In code:** `RepoLink`, `RepoName`
- **Status:** proposed

## Repos

The port that reads a public repo's `motif/glossary.md` and, if there is one, its sketch file, from the default branch.

- **Kind:** port
- **In code:** `Repos`
- **Sketch:** `9e1a0c02`
- **See also:** [GitHub](#github)

## Review

The main view: every term at a glance, grouped by kind, each showing its Avoid words, status, In code and Sketch ids. Proposed terms are picked out, and can be shown on their own.

- **Kind:** domain concept
- **Avoid:** report, dashboard
- **In code:** `Review`, `TermReview`, `KindGroup`
- **Status:** proposed

## Reviewer

The person who opens, drops or pastes a glossary, or follows a repo link, and reviews its terms.

- **Kind:** actor
- **Avoid:** user (that's the port), viewer (Motif Sketch's read-only actor), reader
- **Sketch:** `9e1a0a02`

## Sketch file

A Motif Sketch project, kept beside the glossary as `motif/sketch.motifsketch.json`. Motif Glossary reads only its sketch items: their ids, names and types.

- **Kind:** domain concept
- **Avoid:** sketch, on its own (in Motif Sketch that's the ports-and-adapters view), project (Motif Sketch's word for the file; here, "project" means the app a glossary belongs to)
- **In code:** `SketchFile`; read by `SketchFileReader`
- **Status:** proposed
- **See also:** [Sketch id](#sketch-id), [Sketch item](#sketch-item)

## Sketch id

The id in a term's **Sketch** field: the element or port in the sketch file that the term came from. A term can have two. When there's a sketch file, the review shows the name the id points to.

- **Kind:** domain concept
- **In code:** `Term.SketchIds`, `SketchRef`
- **See also:** [Sketch file](#sketch-file), [Sketch note](#sketch-note)

## Sketch item

An element, port or adapter in a sketch file, as Motif Glossary reads it: its id, name and type, and the kind a term for it would have. Elements and ports are expected to have a term; adapters usually aren't, since most are named after a technology.

- **Kind:** domain concept
- **Avoid:** node, element (an element is only one sort of sketch item)
- **In code:** `SketchItem`
- **Status:** proposed
- **See also:** [Sketch file](#sketch-file)

## Sketch note

A gentle note from comparing the glossary with its sketch file: a sketch id that isn't in the sketch any more, or an element or port in the sketch with no term.

- **Kind:** domain concept
- **Avoid:** error, warning
- **In code:** `SketchNote`, `SketchNotes`
- **Status:** proposed
- **See also:** [Format note](#format-note)

## Source

Where the glossary under review came from: one of the four ways in. A file opened or dropped in, pasted Markdown, a public repo, or the Example.

- **Kind:** domain concept
- **Avoid:** input, origin
- **In code:** `Source`, `SourceKind`
- **Status:** proposed
- **See also:** [Example](#example), [Repo link](#repo-link)

## Status

Whether a term is *proposed* or *agreed*. An agent can propose a term; people agree it. A term with no status counts as agreed.

- **Kind:** domain concept
- **Avoid:** draft, approved
- **In code:** `Status`

## Term

One entry in a glossary: a `##` heading naming it, a short definition, then its fields.

- **Kind:** domain concept
- **Avoid:** word, for the whole entry (the heading is the term's name); definition (that's one part of a term)
- **In code:** `Term`
- **See also:** [Field](#field), [Glossary](#glossary)

## User port

The driving port people use. Its real adapter is the Web App, the review page; pasted glossaries come in through it too.

- **Kind:** port
- **Avoid:** user, for a person (say Reviewer)
- **In code:** `UserTasks`, `UserQueries`
- **Sketch:** `9e1a0c01`
