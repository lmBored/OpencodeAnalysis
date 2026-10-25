import skott from "skott";

const repo = process.argv[2]; // path to the repo root, e.g. ..\opencode

const { getStructure } = await skott({
  cwd: repo,
  includeBaseDir: false,
  fileExtensions: [".ts", ".tsx"],
  ignorePattern: "**/node_modules/**",
  dependencyTracking: { thirdParty: false, builtin: false, typeOnly: true },
});

const graph = getStructure().graph; // { fileId: { adjacentTo: [...], ... } }
const isTest = (f) => /\.(test|spec)\.tsx?$/.test(f);

const ids = Object.keys(graph).filter((f) => !isTest(f));
const ca = {};
const rows = ids.map((f) => {
  const deps = new Set(graph[f].adjacentTo.filter((d) => !isTest(d)));
  for (const d of deps) ca[d] = (ca[d] ?? 0) + 1;
  return { file: f, Ce: deps.size };
});
for (const r of rows) r.Ca = ca[r.file] ?? 0;

console.log("Files analyzed:", rows.length);
for (const key of ["Ce", "Ca"]) {
  const top = [...rows].sort((a, b) => b[key] - a[key]);
  console.log(`\nMax ${key} (file level): ${top[0]?.[key]}`);
  console.table(top.slice(0, 5));
}