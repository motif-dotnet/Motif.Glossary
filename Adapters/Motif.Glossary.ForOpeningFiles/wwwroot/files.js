// The Local Files Fitting: the file picker, and files dropped onto the page.
// Each file comes back as { name, text }. The rules are in the C# adapter.

const accept = ".md,.markdown,.txt,.json,text/markdown,application/json";
let latest = { number: 0, files: Promise.resolve([]) };

function read(fileList) {
  return Promise.all(Array.from(fileList ?? []).map(async f => ({ name: f.name, text: await f.text() })));
}

// Catches drops in the capture phase, so the files are already being read when the page hears about the drop.
// The browser only lets a drop be taken while its event runs, so telling files from other drops stays here.
export function listen() {
  const hasFiles = e => Array.from(e.dataTransfer?.types ?? []).includes("Files");
  window.addEventListener("dragover", e => { if (hasFiles(e)) e.preventDefault(); }, true);
  window.addEventListener("drop", e => {
    if (!hasFiles(e)) return;
    e.preventDefault();
    latest = { number: latest.number + 1, files: read(e.dataTransfer.files) };
  }, true);
}

export async function latestDrop() {
  const { number, files } = latest;
  return { number, files: await files };
}

// Several files can be picked at once: a glossary.md and the sketch file beside it.
export function pick() {
  return new Promise((resolve, reject) => {
    const input = document.createElement("input");
    input.type = "file";
    input.multiple = true;
    input.accept = accept;
    input.addEventListener("change", () => read(input.files).then(resolve, reject));
    input.addEventListener("cancel", () => resolve([]));
    input.click();
  });
}
