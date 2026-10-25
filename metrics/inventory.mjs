import { Project, SyntaxKind } from "ts-morph";
import fs from "node:fs";

const repo = process.argv[2].replace(/\\/g, "/");
const project = new Project({ skipAddingFilesFromTsConfig: true });
project.addSourceFilesAtPaths([
  `${repo}/**/*.ts`, `!${repo}/**/node_modules/**`,
  `!${repo}/**/*.d.ts`, `!${repo}/**/*.test.ts`,
]);

const rows = [];
for (const sf of project.getSourceFiles()) {
  for (const call of sf.getDescendantsOfKind(SyntaxKind.CallExpression)) {
    if (!/Table$/.test(call.getExpression().getText())) continue;
    const args = call.getArguments();
    const cols = args[1];
    if (!cols || cols.getKind() !== SyntaxKind.ObjectLiteralExpression) continue;
    for (const p of cols.getProperties()) {
      rows.push({
        file: sf.getFilePath().replace(/.*packages\//, ""),
        table: args[0].getText(),
        column: p.getName?.() ?? p.getText(),
        definition: p.getText().replace(/\s+/g, " ").slice(0, 100),
      });
    }
  }
}

const esc = (s) => `"${String(s).replace(/"/g, '""')}"`;
const csv = ["file,table,column,definition", ...rows.map((r) => [r.file, r.table, r.column, r.definition].map(esc).join(","))].join("\n");
fs.writeFileSync("columns.csv", "\uFEFF" + csv, "utf8");
console.log("Columns found:", rows.length, "| tables:", new Set(rows.map((r) => r.table)).size);