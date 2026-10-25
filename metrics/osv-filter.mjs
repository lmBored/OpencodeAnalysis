import fs from "node:fs";
const CA = /^(bcrypt|bcryptjs|argon2|@node-rs\/(argon2|bcrypt)|jsonwebtoken|jose|jws|jwa|node-jose|passport.*|oauth.*|openid-client|crypto-js|node-forge|tweetnacl|@noble\/.+|better-auth|next-auth|@auth\/.+|lucia|oslo|express-session|cookie|cookie-signature)$/i;
const res = JSON.parse(fs.readFileSync("osv.json", "utf8")).results ?? [];
const hits = new Map();
for (const r of res) for (const p of r.packages ?? []) {
  if (!CA.test(p.package.name)) continue;
  hits.set(`${p.package.name}@${p.package.version}`, (p.vulnerabilities ?? []).map((v) => v.id));
}
console.log("Vulnerable crypto/auth packages:", hits.size);
for (const [k, ids] of hits) console.log(k, ids.join(", "));