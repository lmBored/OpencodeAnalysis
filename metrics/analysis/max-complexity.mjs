import fs from "node:fs"

// Reads the ESLint JSON produced by:
//   npx eslint . --format json --output-file analysis/complexity.json
// (run from the metrics/ workspace). The complexity rule is configured with a
// threshold of 0, so every function is reported together with its complexity.

const results = JSON.parse(fs.readFileSync(new URL("./complexity.json", import.meta.url), "utf8"))

const rows = []
for (const file of results) {
  for (const message of file.messages) {
    if (message.ruleId !== "complexity") continue
    const value = Number(/complexity of (\d+)/.exec(message.message)?.[1])
    if (!Number.isFinite(value)) continue
    rows.push({
      complexity: value,
      file: file.filePath.replace(/.*[\\/]opencode[\\/]/, ""),
      line: message.line,
      function: /Function '(.+?)'/.exec(message.message)?.[1] ?? "(anonymous)",
    })
  }
}

if (rows.length === 0) {
  console.error("No complexity warnings found in complexity.json.")
  console.error("Generate it first from metrics/: npx eslint . --format json --output-file analysis/complexity.json")
  process.exit(1)
}

const byFile = new Map()
for (const row of rows) byFile.set(row.file, Math.max(byFile.get(row.file) ?? 0, row.complexity))

const top = [...rows].sort((a, b) => b.complexity - a.complexity)
const files = [...byFile]
  .map(([file, complexity]) => ({ file, complexity }))
  .sort((a, b) => b.complexity - a.complexity)

console.log("Linted files:", results.length)
console.log("Functions reported:", rows.length)
console.log(`Max complexity: ${top[0].complexity} (${top[0].file}:${top[0].line}, ${top[0].function})`)
console.log("\nTop 10 functions:")
console.table(top.slice(0, 10).map(({ file, line, function: name, complexity }) => ({ file, line, name, complexity })))
console.log("\nTop 10 files by max function complexity:")
console.table(files.slice(0, 10))
