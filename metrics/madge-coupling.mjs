import madge from "madge";
import path from "node:path";

const repo = process.argv[2]; // e.g. "C:\full\path\to\opencode"

const res = await madge(repo, {
  fileExtensions: ["ts", "tsx"],
  excludeRegExp: [/node_modules/, /[\\/]dist[\\/]/, /\.(test|spec)\.tsx?$/],
  detectiveOptions: { ts: { skipTypeImports: false }, tsx: { skipTypeImports: false } },
  // tsConfig: path.join(repo, "tsconfig.json"),  // uncomment to resolve path aliases
});

const graph = res.obj(); // { file: [imported files] }
const ca = {};
const rows = Object.entries(graph).map(([file, deps]) => {
  const uniq = new Set(deps);
  for (const d of uniq) ca[d] = (ca[d] ?? 0) + 1;
  return { file, Ce: uniq.size };
});
for (const r of rows) r.Ca = ca[r.file] ?? 0;

console.log("Files analyzed:", rows.length);
for (const key of ["Ce", "Ca"]) {
  const top = [...rows].sort((a, b) => b[key] - a[key]);
  console.log(`\nMax ${key} (file level): ${top[0]?.[key]}`);
  console.table(top.slice(0, 5));
}

for (const r of rows) {
  const total = r.Ca + r.Ce;
  r.I = total === 0 ? null : r.Ce / total; // null = isolated file, undefined by definition
}

const defined = rows.filter((r) => r.I !== null);
console.log("\nFiles with defined instability:", defined.length, "of", rows.length);

const vals = defined.map((r) => r.I).sort((a, b) => a - b);
const mean = vals.reduce((s, v) => s + v, 0) / vals.length;
console.log(`I: mean ${mean.toFixed(2)}, median ${vals[Math.floor(vals.length / 2)].toFixed(2)}`);

console.log("\nMost unstable (I = 1, depends on others, nothing depends on it):");
console.table(defined.filter((r) => r.I === 1).sort((a, b) => b.Ce - a.Ce).slice(0, 5));

console.log("\nMost stable, highest Ca (I near 0):");
console.table([...defined].sort((a, b) => a.I - b.I || b.Ca - a.Ca).slice(0, 5));

const pct = (arr, p) => arr[Math.min(arr.length - 1, Math.floor(p * arr.length))];
const edges = [["0", 0, 0], ["1-2", 1, 2], ["3-5", 3, 5], ["6-10", 6, 10], ["11+", 11, Infinity]];

for (const [key, name] of [["Ca", "fan-in"], ["Ce", "fan-out"]]) {
  const vals = rows.map((r) => r[key]).sort((a, b) => a - b);
  const mean = vals.reduce((s, v) => s + v, 0) / vals.length;
  console.log(`\n${name} (${key}): mean ${mean.toFixed(1)}, median ${pct(vals, 0.5)}, p90 ${pct(vals, 0.9)}, p99 ${pct(vals, 0.99)}, max ${vals.at(-1)}`);

  const hist = {};
  for (const [label, lo, hi] of edges)
    hist[label] = vals.filter((v) => v >= lo && v <= hi).length;
  console.table(hist);
}

import fs from "node:fs";

const P = [0.5, 0.75, 0.9, 0.95, 0.99, 0.999, 1];
const at = (arr, p) => arr[Math.min(arr.length - 1, Math.ceil(p * arr.length) - 1)]; // nearest-rank

const table = P.map((p) => ({ percentile: p === 1 ? "max" : `p${p * 100}` }));
for (const key of ["Ca", "Ce"]) {
  const vals = rows.map((r) => r[key]).sort((a, b) => a - b);
  P.forEach((p, i) => {
    const v = at(vals, p);
    const prev = i === 0 ? null : table[i - 1][key];
    table[i][key] = v;
    table[i][`${key} growth`] = prev ? `x${(v / prev).toFixed(1)}` : "-";
  });
}
console.table(table);

fs.writeFileSync(
  "percentiles.csv",
  ["percentile,Ca,Ce", ...table.map((r) => `${r.percentile},${r.Ca},${r.Ce}`)].join("\n"),
  "utf8"
);