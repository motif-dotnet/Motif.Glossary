// Browser Files: the file picker, and files dropped onto the page. Each file comes back as [name, text].

const accept = ".md,.markdown,.txt,.json,text/markdown,application/json";
let dropped = Promise.resolve([]);
let listening = false;

function read(fileList) {
  return Promise.all(Array.from(fileList ?? []).map(async f => [f.name, await f.text()]));
}

// Catches drops in the capture phase, so the files are already being read when the page hears about the drop.
export function listen() {
  if (listening) return;
  listening = true;
  const hasFiles = e => Array.from(e.dataTransfer?.types ?? []).includes("Files");
  window.addEventListener("dragover", e => { if (hasFiles(e)) e.preventDefault(); }, true);
  window.addEventListener("drop", e => {
    if (!hasFiles(e)) return;
    e.preventDefault();
    dropped = read(e.dataTransfer.files);
  }, true);
}

export async function takeDropped() {
  const files = await dropped;
  dropped = Promise.resolve([]);
  return files;
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
