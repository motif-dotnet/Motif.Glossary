// The Web App's own browser plumbing. Everything here is thin: the logic stays in C#.

export function fragment() { return location.hash; }
export function pageUrl() { return location.href; }

// Replaces the fragment without a history entry or a hashchange event.
export function setFragment(value) {
  const url = location.pathname + location.search + (value ? "#" + value : "");
  if (url !== location.pathname + location.search + location.hash) history.replaceState(null, "", url);
}

// Someone edited the address bar, or followed a repo link into this tab.
export function listenForFragment(dotnet) {
  window.addEventListener("hashchange", () => dotnet.invokeMethodAsync("OnFragment", location.hash));
}

export function scrollToTerm(id) {
  const el = document.getElementById(id);
  if (!el) return;
  el.scrollIntoView({ behavior: matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth", block: "start" });
  el.classList.remove("flash");
  void el.offsetWidth; // restart the animation
  el.classList.add("flash");
}

export async function copy(text) {
  try { await navigator.clipboard.writeText(text); return true; } catch { return false; }
}

export function focus(selector) {
  const el = document.querySelector(selector);
  if (el) { el.focus(); el.select?.(); }
}

// Theme: follows the system, with an override kept in this browser only. Shared with the other Motif tools.
export function theme() {
  try { return localStorage.getItem("motif-theme") || "system"; } catch { return "system"; }
}

export function setTheme(value) {
  try { localStorage.setItem("motif-theme", value); } catch { /* storage may be blocked */ }
  if (value === "light" || value === "dark") document.documentElement.setAttribute("data-theme", value);
  else document.documentElement.removeAttribute("data-theme");
}

export function scrollToTop(selector) {
  const el = document.querySelector(selector);
  if (el) el.scrollTop = 0;
}
