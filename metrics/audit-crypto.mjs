const repo = process.argv[2];
const CA = /^(bcrypt|bcryptjs|argon2|@node-rs\/(argon2|bcrypt)|jsonwebtoken|jose|jws|jwa|node-jose|passport.*|oauth.*|openid-client|crypto-js|node-forge|tweetnacl|@noble\/.+|better-auth|next-auth|@auth\/.+|lucia|oslo|express-session|cookie|cookie-signature)$/i;

const run = (args) => Bun.spawnSync(["bun", ...args], { cwd: repo }).stdout.toString();

const audit = JSON.parse(run(["audit", "--json"]));
const vulnerable = Object.keys(audit).filter((n) => CA.test(n));

const installed = new Set();
for (const m of run(["pm", "ls", "--all"]).matchAll(/((?:@[\w.-]+\/)?[\w.-]+)@\d[^\s]*/g))
  if (CA.test(m[1])) installed.add(m[1]);

console.log("Installed crypto/auth packages:", installed.size, [...installed].join(", "));
console.log("Vulnerable crypto/auth packages:", vulnerable.length, vulnerable.join(", "));
console.log("Vulnerable packages overall:", Object.keys(audit).length);