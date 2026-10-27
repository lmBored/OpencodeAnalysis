import fs from "node:fs";

const results = JSON.parse(fs.readFileSync("complexity.json", "utf8"));
const rows = [];

for (const file of results) {
  for (const m of file.messages) {
    if (m.ruleId !== "complexity") continue;
    const value = Number(m.message.match(/complexity of (\d+)/)?.[1]);
    rows.push({ file: file.filePath, line: m.line, value });
  }
}

rows.sort((a, b) => b.value - a.value);
console.log("Functions measured:", rows.length);
console.log("Max complexity:", rows[0]?.value);
console.table(rows.slice(0, 10));