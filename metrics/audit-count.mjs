const repo = process.argv[2];
const p = Bun.spawnSync(["bun", "audit", "--json"], { cwd: repo });
const audit = JSON.parse(p.stdout.toString()); // bun audit exits non-zero when it finds vulnerabilities, which is fine

const names = Object.keys(audit);
const advisories = names.flatMap((n) => audit[n]);
const bySeverity = {};
for (const a of advisories) bySeverity[a.severity] = (bySeverity[a.severity] ?? 0) + 1;

console.log("Vulnerable packages:", names.length);
console.log("Advisories:", advisories.length);
console.log("Advisories by severity:", bySeverity);
console.log("Unique advisory IDs:", new Set(advisories.map((a) => a.id ?? a.url)).size);