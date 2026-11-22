# Manual checks

Every spec runs the real adapters over fake Fittings, so the real Fittings are checked by hand. The browser specs (`Checks/Motif.Glossary.BrowserSpecs`, decision 18) cover the page's layout, the theme and repo links, in Edge, but not the file picker, dropping, or the real GitHub. Run a Fitting's checks before a release when it has changed, in Chrome, and in Firefox when the browser's side of it has changed. If a fake Fitting turns out to behave differently from the real one, tighten the contract in its Seam's doc comments, and fix the fake.

## Local Files (`JsInteropLocalFiles`, `wwwroot/files.js`)

- [ ] **Open** shows the file picker, offering Markdown and JSON files. Picking a `glossary.md` and its `sketch.motifsketch.json` together reviews both.
- [ ] Closing the picker changes nothing.
- [ ] Dropping a `glossary.md` anywhere on the page reviews it. Dropping its sketch file on its own then joins it to the review.
- [ ] Dropping text, such as a selection from another page, does nothing, and the page doesn't navigate away.

## GitHub (the browser's `HttpClientHandler`)

- [ ] **Repo** with `motif-dotnet/Motif.Sketch` reviews its glossary with its sketch file, and the address carries `#repo=motif-dotnet/Motif.Sketch`.
- [ ] A repo with no `motif/glossary.md` says so in a toast.
- [ ] With the network off (DevTools, Network, Offline), a repo says GitHub couldn't be reached.
