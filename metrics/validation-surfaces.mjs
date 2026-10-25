import { Project, SyntaxKind, Node } from "ts-morph";
import fs from "node:fs";

const repo = process.argv[2].replace(/\\/g, "/");
const SCHEMA = /(?<!JSON)\.(parse|safeParse|parseAsync|safeParseAsync)\(|zValidator|[sS]chema|\.check\(/;
const CLI_STRONG = /\b(choices|coerce|check|demandOption|conflicts|implies|requiresArg)\b/;
const TUI_SIGNAL = new RegExp(SCHEMA.source + "|\\.trim\\(|\\.test\\(|\\.match\\(|\\.startsWith\\(|\\.length\\b");
const FN = new Set([SyntaxKind.FunctionDeclaration, SyntaxKind.FunctionExpression, SyntaxKind.ArrowFunction, SyntaxKind.MethodDeclaration]);

const project = new Project({ skipAddingFilesFromTsConfig: true });
project.addSourceFilesAtPaths([
  `${repo}/**/*.ts`, `${repo}/**/*.tsx`,
  `!${repo}/**/node_modules/**`, `!${repo}/**/*.d.ts`, `!${repo}/**/*.test.ts`,
]);

const scopeOf = (n) => n.getFirstAncestor((a) => FN.has(a.getKind())) ?? n.getSourceFile();
const handlerText = (expr) => {
  if (!expr) return null;
  if (expr.getKind() === SyntaxKind.ArrowFunction || expr.getKind() === SyntaxKind.FunctionExpression) return expr.getText();
  if (expr.getKind() === SyntaxKind.Identifier) {
    try { const d = expr.getDefinitionNodes(); if (d.length) return d.map((x) => x.getText()).join("\n"); } catch {}
  }
  return null;
};

const rows = [];
const seen = new Set();
const add = (surface, node, sf, status) =>
  rows.push({ surface, file: sf.getFilePath().replace(/.*packages\//, ""), line: node.getStartLineNumber(), status });

for (const sf of project.getSourceFiles()) {
  for (const c of sf.getDescendantsOfKind(SyntaxKind.CallExpression)) {
    const t = c.getExpression().getText();
    const args = c.getArguments();
    if (/\.(values|set)$/.test(t) && /\b(insert|update)\(/.test(t)) {
      add("db-write", c, sf, SCHEMA.test(scopeOf(c).getText()) ? "yes" : "no");
    } else if (/(^|\.)(readFile|readFileSync)$|^Bun\.file$|^JSON\.parse$/.test(t)) {
      const s = scopeOf(c), key = sf.getFilePath() + ":" + s.getStart();
      if (seen.has(key)) continue;
      seen.add(key);
      add("file-config", c, sf, SCHEMA.test(s.getText()) ? "yes" : "no");
    } else if (/\.(option|positional)$/.test(t) && args.length >= 2) {
      const def = args[1].getText();
      add("cli", c, sf, CLI_STRONG.test(def) ? "yes" : /\btype\b/.test(def) ? "typed-only" : "no");
    } else if (/createInterface$|^(useKeyboard|useInput)$/.test(t) && args.length) {
      const h = handlerText(args[args.length - 1]);
      add("tui", c, sf, h === null ? "review" : TUI_SIGNAL.test(h) ? "yes" : "no");
    }
  }
  for (const p of sf.getDescendantsOfKind(SyntaxKind.PropertyAccessExpression)) {
    if (/^(process|Bun)\.argv$/.test(p.getText()))
      add("cli", p, sf, /parseArgs|yargs/.test(scopeOf(p).getText()) || SCHEMA.test(scopeOf(p).getText()) ? "yes" : "no");
  }
  for (const a of sf.getDescendantsOfKind(SyntaxKind.JsxAttribute)) {
    if (!/^(onInput|onSubmit|onPaste|onChange)$/.test(a.getNameNode().getText())) continue;
    const init = a.getInitializer();
    const h = init && Node.isJsxExpression(init) ? handlerText(init.getExpression()) : null;
    add("tui", a, sf, h === null ? "review" : TUI_SIGNAL.test(h) ? "yes" : "no");
  }
}

const surfaces = [...new Set(rows.map((r) => r.surface))];
const count = (rs, s) => rs.filter((r) => r.status === s).length;
const summary = [...surfaces, "ALL"].map((s) => {
  const rs = s === "ALL" ? rows : rows.filter((r) => r.surface === s);
  return { surface: s, sites: rs.length, yes: count(rs, "yes"), "typed-only": count(rs, "typed-only"),
           review: count(rs, "review"), no: count(rs, "no"), coverage: `${((100 * count(rs, "yes")) / rs.length).toFixed(1)}%` };
});
console.table(summary);

const esc = (s) => `"${String(s).replace(/"/g, '""')}"`;
fs.writeFileSync("surface-sites.csv", "\uFEFF" + ["surface,file,line,status", ...rows.map((r) => [r.surface, r.file, r.line, r.status].map(esc).join(","))].join("\n"), "utf8");