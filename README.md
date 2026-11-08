# Motif Glossary

A tool in the **[Motif.NET](https://github.com/motif-dotnet)** suite. It shows a project's glossary, `motif/glossary.md`, in a form that's easy to look at and review: terms grouped by kind, with their Avoid words, status and sketch ids visible at a glance, *proposed* terms picked out, and gentle notes where a term doesn't follow the format or no longer matches the sketch.

It's a Blazor WebAssembly app for GitHub Pages: no server and no accounts. Open or drop a glossary, paste one, or give a public GitHub repo.

The glossary format is in the [Motif Glossary design note](https://github.com/motif-dotnet/.github/blob/main/design/glossary.md). This app started from [motif-dotnet/.github#10](https://github.com/motif-dotnet/.github/issues/10).

## Design

### Context diagram

![Motif Glossary context diagram](motif/diagrams/context-diagram.svg)

### Ports and adapters

![Motif Glossary ports-and-adapters sketch](motif/diagrams/ports-and-adapters.svg)

These pictures are exported by [Motif Sketch](https://motif-dotnet.github.io/Motif.Sketch/) from [motif/sketch.motifsketch.json](motif/sketch.motifsketch.json). The [motif/](motif/) folder holds the design, the [solution plan](motif/plan.md) and the [glossary](motif/glossary.md). The decisions so far are in [decisions.md](decisions.md).

## Status

**Stubbed.** The solution follows [motif/plan.md](motif/plan.md): every project, port and adapter exists, with no functionality yet.

## Build and run

You need the .NET 10 SDK.

```
dotnet run --project Motif.Glossary.App   # the app
dotnet test                                # the specs
```
