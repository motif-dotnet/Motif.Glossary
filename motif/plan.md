# Motif.Glossary: solution plan

*Exported by Motif Sketch from the project "Motif Glossary". Stubs only: no functionality.*

## 1. Overview

- **System**: Motif.Glossary (Blazor WebAssembly (GitHub Pages))
- **Purpose**: Shows a project's glossary at a glance for review: terms grouped by kind, proposed terms picked out, gentle notes on the format and the sketch
- **Solution**: `Motif.Glossary.sln`, targeting `net10.0`
- **Architecture**: ports and adapters. The application core (Domain and UseCases) knows nothing about the outside world; every external concern sits behind a port and is reached through an adapter.
- **Ports**: 1 driving, 2 driven

## 2. Solution and project layout

Projects are grouped by role, and the solution folders in the `.sln` mirror the folders on disk. There are no `src/` or `tests/` folders.

```
Motif.Glossary.sln
  Motif.Glossary.App/
  Motif.Glossary/
    Motif.Glossary.Domain/
    Motif.Glossary.UseCases/
  Adapters/
    Motif.Glossary.ForOpeningRepos/
    Motif.Glossary.ForOpeningFiles/
  Checks/
    Motif.Glossary.Fakes/
    Motif.Glossary.Specs/
```

| Project | Folder | Template | Namespace | Role |
|---|---|---|---|---|
| `Motif.Glossary.App` | `Motif.Glossary.App/` | `blazorwasm` | `Motif.Glossary.App` | The app: runnable adapter on the User port |
| `Motif.Glossary.Domain` | `Motif.Glossary/Motif.Glossary.Domain/` | `classlib` | `Motif.Glossary` | Domain-specific business rules |
| `Motif.Glossary.UseCases` | `Motif.Glossary/Motif.Glossary.UseCases/` | `classlib` | `Motif.Glossary` | Application rules (use cases) and all port interfaces |
| `Motif.Glossary.ForOpeningRepos` | `Adapters/Motif.Glossary.ForOpeningRepos/` | `classlib` | `Motif.Glossary.Adapters` | Adapters on the For opening repos port |
| `Motif.Glossary.ForOpeningFiles` | `Adapters/Motif.Glossary.ForOpeningFiles/` | `razorclasslib` | `Motif.Glossary.Adapters` | Adapters on the For opening files port |
| `Motif.Glossary.Fakes` | `Checks/Motif.Glossary.Fakes/` | `classlib` | `Motif.Glossary.Fakes` | Fake driven adapters, for specs |
| `Motif.Glossary.Specs` | `Checks/Motif.Glossary.Specs/` | `xunit` | `Motif.Glossary.Specs` | Specs: the checks on every driving port |

## 3. Project references (dependency rules)

| Project | References |
|---|---|
| `Motif.Glossary.App` | `Motif.Glossary.UseCases` |
| `Motif.Glossary.Domain` | nothing |
| `Motif.Glossary.UseCases` | `Motif.Glossary.Domain` |
| `Motif.Glossary.ForOpeningRepos` | `Motif.Glossary.UseCases` |
| `Motif.Glossary.ForOpeningFiles` | `Motif.Glossary.UseCases` |
| `Motif.Glossary.Fakes` | `Motif.Glossary.UseCases`, `Motif.Glossary.ForOpeningFiles` |
| `Motif.Glossary.Specs` | `Motif.Glossary.UseCases`, `Motif.Glossary.Fakes`, `Motif.Glossary.ForOpeningFiles` |

Rules:

- `Motif.Glossary.Domain` references nothing.
- `Motif.Glossary.UseCases` references `Motif.Glossary.Domain` only.
- Every adapter project references `Motif.Glossary.UseCases` only. **Adapters never reference each other.**
- The fakes project also references the adapter projects whose Seams it fakes, and the specs project references them too, so specs run the real adapters over fake Fittings.
- These references are the dependency rule. Don't add others.

## 4. Namespaces

- `Motif.Glossary.Domain` and `Motif.Glossary.UseCases` both use the plain `Motif.Glossary` namespace, so the port interfaces are `Motif.Glossary.ForDoing` and so on.
- Every other project uses the namespace in section 2. Real and simulated adapters share one namespace, and so do the fakes.
- Folders inside a project don't change its namespace.
- Set `RootNamespace` in each `.csproj` to its namespace, so new files created in an IDE get it.
- Every namespace sits inside `Motif.Glossary`, so adapters, fakes and specs see the port interfaces without a `using`.

## 5. Ports

All port interfaces live in `Motif.Glossary.UseCases`, namespace `Motif.Glossary`, one file each. Interfaces take the plain name of the port, **with no `I` prefix**. Leave them empty: their members are design work still to do.

```
Motif.Glossary.UseCases/
  ForDoing.cs
  ForSeeing.cs
  MotifGlossaryApplication.cs
  Ports/
    ForOpeningRepos.cs
    ForOpeningFiles.cs
```

### User (driving)

The User port: how people (and anything acting for them) use the application. It has two halves: `ForDoing`, the commands, and `ForSeeing`, the queries.

`ForDoing.cs`:

```csharp
namespace Motif.Glossary;

public interface ForDoing
{
}
```

`ForSeeing.cs`:

```csharp
namespace Motif.Glossary;

public interface ForSeeing
{
}
```

### For opening repos (driven)

Reads a public repo's motif/glossary.md and, when it's there, its sketch, from the default branch

`Ports/ForOpeningRepos.cs`:

```csharp
namespace Motif.Glossary;

public interface ForOpeningRepos
{
}
```

### For opening files (driven)

Reads a glossary.md the Reviewer opens or drops in, and a sketch file with it if there is one

`Ports/ForOpeningFiles.cs`:

```csharp
namespace Motif.Glossary;

public interface ForOpeningFiles
{
}
```

Also add one stub class, `MotifGlossaryApplication` in `Motif.Glossary.UseCases` (namespace `Motif.Glossary`), implementing the driving interfaces (`ForDoing`, `ForSeeing`). It is the application itself; its constructor will later take the driven ports.

## 6. Adapters

Kinds: **Real** is the production adapter; **Fake** is a simple in-process stand-in for fast specs; **Simulated** is a higher-fidelity stand-in for people running the whole app locally; **Checks** drive the application through a driving port and check what it does.

| Port | Adapter | Kind | Technology | Project | Class | Source |
|---|---|---|---|---|---|---|
| User | Web App | Real | Blazor WebAssembly UI | `Motif.Glossary.App` | - | Context diagram: Reviewer |
| User | Specs | Checks | xUnit | `Motif.Glossary.Specs` | - | Added in the sketch |
| For opening repos | GitHub | Real | HttpClient (raw.githubusercontent.com) | `Motif.Glossary.ForOpeningRepos` | `ForOpeningReposViaGitHub` | Context diagram: GitHub |
| For opening repos | Fake GitHub | Fake | - | `Motif.Glossary.Fakes` (`ForOpeningRepos/`) | `FakeGitHubForOpeningRepos` | Added in the sketch |
| For opening files | Picker and Drop | Real | File picker and drag and drop (JS interop) | `Motif.Glossary.ForOpeningFiles` | `ForOpeningFilesViaPickerAndDrop` | Context diagram: Glossary Files |

For each driven adapter, add the stub class named above implementing its port's interface, in its project's namespace and in the folder shown. For example:

```csharp
namespace Motif.Glossary.Adapters;

public sealed class ForOpeningReposViaGitHub : ForOpeningRepos
{
}
```

Deep adapters: an adapter with a Seam keeps its own rules, and reaches the outside world through the Seam, an interface it owns. The Seam's Fitting makes one outside call per member, with no branches worth testing. Specs run the real adapter over its fake Fitting, so its port has no fake of its own. The Seam and its real Fitting go in the adapter's project and namespace; the fake Fitting goes in the fakes project, in the port's folder.

| Port | Adapters | Seam | Real Fitting | Fake Fitting |
|---|---|---|---|---|
| For opening files | Picker and Drop | `LocalFiles` | `JsInteropLocalFiles` | `FakeLocalFiles` (`ForOpeningFiles/`) |

For each Seam, add the empty interface, its real Fitting and its fake Fitting. Each adapter with a Seam takes it in its one constructor. For example:

```csharp
namespace Motif.Glossary.Adapters;

public interface LocalFiles
{
}

public sealed class JsInteropLocalFiles : LocalFiles
{
}

public sealed class ForOpeningFilesViaPickerAndDrop(LocalFiles localFiles) : ForOpeningFiles
{
}
```

```csharp
using Motif.Glossary.Adapters;

namespace Motif.Glossary.Fakes;

public sealed class FakeLocalFiles : LocalFiles
{
}
```

Driving adapters (the app among them) keep their project template's default files. Checks and fakes on driving ports are the specs (section 7).

Adapter notes:

- **Web App** (`Motif.Glossary.App`): The review page. Pasted glossaries reach the application through it
- **Specs** (`Motif.Glossary.Specs`): Checks that drive the application through the User port, using the fake driven adapters
- **GitHub** (`Motif.Glossary.ForOpeningRepos`): Public repos whose motif/ folder holds the glossary and, often, the sketch
- **Picker and Drop** (`Motif.Glossary.ForOpeningFiles`): A glossary.md on the Reviewer's computer, opened or dropped in, with the sketch beside it if there is one

## 7. Fakes and specs

- `Motif.Glossary.Fakes` (`Checks/Motif.Glossary.Fakes/`): every fake Fitting, and every fake of a port whose adapters are their own Fittings, with a folder per port (`ForOpeningRepos/`, `ForOpeningFiles/`). The namespace is flat: `Motif.Glossary.Fakes`.
- `Motif.Glossary.Specs` (`Checks/Motif.Glossary.Specs/`): xUnit. Packages: `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `coverlet.collector`, `AwesomeAssertions`. Keep the template's packages and add `AwesomeAssertions`.
  Add one placeholder spec class with a single passing `[Fact]`, so `dotnet test` runs. Specs wire the real adapters over their fake Fittings, and the other fakes, up by hand; there is no composition root yet.
- The browser adapters (`Motif.Glossary.ForOpeningFiles`) also take `Microsoft.JSInterop`: each reaches the browser through a small JS module it ships as a static web asset.
- Use the latest stable package versions. Use AwesomeAssertions (`.Should()` style) in specs.

## 8. Not placed

Every element of the context diagram is on the sketch.

## 9. Build this

Instructions for the coding agent:

1. Create `Motif.Glossary.sln` (classic `.sln` format: `dotnet new sln --format sln`).
2. Create every project in section 2 with its template, in its folder, and add it to the solution in the matching solution folder (`dotnet sln add <project> --solution-folder <folder>`, or `--in-root` for top-level projects). Set each project's `RootNamespace`.
3. Delete template placeholder files (`Class1.cs`, `UnitTest1.cs`, and the `razorclasslib` samples (`Component1.razor`, `ExampleJsInterop.cs`, the files in `wwwroot/`)), except `Class1.cs` in `Motif.Glossary.Domain`: keep it, with its namespace changed to `Motif.Glossary`, to show the namespace choice.
4. Add exactly the project references in section 3, and the packages in section 7.
5. Add a root `.editorconfig` with a naming rule that says interfaces are plain PascalCase, so the IDE doesn't ask for an `I` prefix:

   ```ini
   root = true

   [*.cs]
   dotnet_naming_rule.interfaces_no_prefix.symbols = interfaces
   dotnet_naming_rule.interfaces_no_prefix.style = pascal_case
   dotnet_naming_rule.interfaces_no_prefix.severity = warning

   dotnet_naming_symbols.interfaces.applicable_kinds = interface
   dotnet_naming_symbols.interfaces.applicable_accessibilities = *

   dotnet_naming_style.pascal_case.capitalization = pascal_case
   ```

6. Add the port interfaces and the application stub from section 5, and the adapter stub classes, and each Seam with its Fittings, from section 6.
7. Add the placeholder spec from section 7.
8. Check that `dotnet build` and `dotnet test` both succeed.

Ground rules:

- **Stubs only.** No functionality, no interface members, no wiring.
- **No composition root** (no host project, no dependency injection setup) in this version.
- **No `I` prefix** on interfaces. An adapter is named for its port, then its way (`ForStoringViaFileSystem`); other implementations carry the meaningful extra (`FileSystemFolder`), never `Impl`.
- **Two-letter acronyms stay in capitals** in code names (`IndexedDB`, `UI`).
- Don't add projects, references or packages the plan doesn't list.
