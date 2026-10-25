## kitlangton (1084 commits; random sample of 30, seed 42)
types: refactor (314); fix (231); test (177); feat (141)
scopes: server (66); httpapi (64); core (59); session (47); tui (44); opencode (35)
packages (sampled merged PRs): packages/opencode (75); packages/core (52); packages/tui (22); packages/client (5); specs/ (5); (root) (4)

- 2026-01-11 [a457828](https://github.com/anomalyco/opencode/commit/a457828a67548288276f120de2fd5bf4e60baed9) fix(opencode): command palette mouse hover highlights wrong item (#7721)
- 2026-03-27 [7b44918](https://github.com/anomalyco/opencode/commit/7b449181498252f64eff7410c39aeebe7d2f1a88) refactor(tool-registry): yield Config/Plugin services, use Effect.forEach (#19363)
- 2026-04-01 [5fd833a](https://github.com/anomalyco/opencode/commit/5fd833aa18d2cc71c977925c0646392a7f78ece2) refactor: standardize InstanceState variable name to state (#20267)
- 2026-04-09 [3199383](https://github.com/anomalyco/opencode/commit/3199383eef4cc2ac4ca086f9485b071061dcff70) fix: finalize interrupted bash via tool result path (#21724)
- 2026-04-09 [10441ef](https://github.com/anomalyco/opencode/commit/10441efad1895cbaa77be3ba5277026b489894a2) refactor(effect): extract session run state service (#21744)
- 2026-04-09 [b2f621b](https://github.com/anomalyco/opencode/commit/b2f621b897ca636dc720b035072555e03c0da30a) refactor(session): inline init route orchestration (#21754)
- 2026-04-11 [f38f415](https://github.com/anomalyco/opencode/commit/f38f415bf0af3fb8baf211b83996d3a58a1fd010) refactor: collapse Format facade (#21980)
- 2026-04-14 [a2cb490](https://github.com/anomalyco/opencode/commit/a2cb4909daac81a297e80c3a5059b9bcff8f6b3e) refactor(plugin): remove async facade exports (#22367)
- 2026-04-16 [bf4c107](https://github.com/anomalyco/opencode/commit/bf4c1078290a5bf7e580141b17e7b37d905de311) fix: remove 7 unnecessary `as any` casts in opencode core (#22840)
- 2026-04-16 [715786b](https://github.com/anomalyco/opencode/commit/715786bbf96304f617c5f2a48ed49fe6101c90ef) refactor: unwrap FileTime namespace + self-reexport (#22966)
- 2026-04-17 [19d15d9](https://github.com/anomalyco/opencode/commit/19d15d9ff7826db276219bccce278f78b654a431) refactor: unwrap ConfigProvider namespace + self-reexport (#22949)
- 2026-04-17 [f83ceca](https://github.com/anomalyco/opencode/commit/f83cecaaf6ee29da70109575595901884cdbc312) fix(opencode): untrace streaming event hot paths (#23156)
- 2026-04-21 [2da6d86](https://github.com/anomalyco/opencode/commit/2da6d860e0e2c5309a0030a5591c8218fffd6a99) refactor(core): derive provider schema .zod via effect-zod walker (#23753)
- 2026-04-30 [fef7981](https://github.com/anomalyco/opencode/commit/fef79819425714c1cbd969dd2139b8c4b96e0d16) test: use Effect runtime in runner deadlock case (#25045)
- 2026-05-01 [ce3b098](https://github.com/anomalyco/opencode/commit/ce3b0988c416d4505309dbad7e23e9439645aafa) refactor(project): yield instance context in bootstrap (#25204)
- 2026-05-03 [40dc2fa](https://github.com/anomalyco/opencode/commit/40dc2fa3c1d6217d0f4fd21d813160e41f438a55) refactor(cli/providers): flatten — Effect-native handlers end-to-end (#25537)
- 2026-05-09 [b24a4e8](https://github.com/anomalyco/opencode/commit/b24a4e897e59d7cfdcdac364e49488acba39774b) chore(server): clean up post-Hono-deletion scar tissue (#26542)
- 2026-05-09 [805af01](https://github.com/anomalyco/opencode/commit/805af011c9d74668a58160399d38e8fb446ebdaf) test(session): regression test for #26574 + mirror loosening on Vcs.FileDiff (#26578)
- 2026-05-10 [d28b5ad](https://github.com/anomalyco/opencode/commit/d28b5ad2f484861c7ee0e70ee8f6e7f8b4826075) refactor(http-recorder): Redactor + Recorder seams, README (#26636)
- 2026-05-10 [049502f](https://github.com/anomalyco/opencode/commit/049502fac6136f4c44eb73f929130c8fb4deeb62) fix(server): return diagnosable body for schema rejections (#26631)
- 2026-05-11 [effd967](https://github.com/anomalyco/opencode/commit/effd96755e70ddb9e0955bc444860d5cc8829248) Use SyncEvent service at event call sites (#26782)
- 2026-05-12 [3974520](https://github.com/anomalyco/opencode/commit/3974520742e1f2a64209429b4e8c6c845f34f6b5) Migrate UI cancel error to tagged error (#27112)
- 2026-05-12 [1d46130](https://github.com/anomalyco/opencode/commit/1d4613006aa2dc25ec2a8050c87763b74f27c46d) test(project): migrate instance tests to Effect runner (#27130)
- 2026-05-13 [913ea36](https://github.com/anomalyco/opencode/commit/913ea36ae61617152ae28a092e0c4f9446dc09a2) test(server): scope MCP HttpApi handler (#27226)
- 2026-05-14 [5e41dbb](https://github.com/anomalyco/opencode/commit/5e41dbbcbf7fb437a877806ecbb48ebcf094d72e) test(effect): use Effect sleep in instance state tests (#27404)
- 2026-05-14 [8e35358](https://github.com/anomalyco/opencode/commit/8e353584c7fdf14e65cdc6f547a9570a801ae956) test(format): remove formatter check sleeps (#27407)
- 2026-05-19 [18b9cec](https://github.com/anomalyco/opencode/commit/18b9cec50d6b058cf991e6c29d643a7717d07196) test(cli): help-text snapshots for every CLI command (#28267)
- 2026-05-19 [55baa16](https://github.com/anomalyco/opencode/commit/55baa16fbc740d943423b8e145d1d791c184a6c4) test(lib): extract snapshot normalizer utility for cross-OS stability (#28356)
- 2026-05-19 [512e34a](https://github.com/anomalyco/opencode/commit/512e34af83dca3b27f4abdc353bb1b427ad56403) Migrate MCP config tests to instance fixtures (#28338)
- 2026-06-22 [fe840d4](https://github.com/anomalyco/opencode/commit/fe840d42b859f6439be8374774cdddbb6f93468b) refactor(core): simplify session run coordination (#33388)

## rekram1-node (1258 commits; random sample of 30, seed 42)
types: fix (415); tweak (157); ci (141); ignore (88)
scopes: opencode (81); mcp (30); tui (27); provider (18); core (17); codemode (6)
packages (sampled merged PRs): packages/opencode (63); packages/core (38); packages/ai (28); packages/codemode (20); (root) (15); packages/plugin (8)

- 2025-10-13 [5885b69](https://github.com/anomalyco/opencode/commit/5885b691b9a9ccee08a798fdbbfb7edd7a2d7ee7) docs: update recommended models list (#3121)
- 2025-10-19 [5d3a88f](https://github.com/anomalyco/opencode/commit/5d3a88f34fcc1b4c1f0abe91226bbf2619c2b7a4) fix: snapshot undo bug (#3290)
- 2025-10-22 [593e89b](https://github.com/anomalyco/opencode/commit/593e89b4f4caf196f05112c956d2b67d3bc5a634) vscode: fix script
- 2025-11-01 [cb4401e](https://github.com/anomalyco/opencode/commit/cb4401ec92efa046e3c3ba06fc221590b435703f) ignore: update contributing md
- 2025-11-04 [f501501](https://github.com/anomalyco/opencode/commit/f5015017911c81c6d89d0a3dcdbe475b948c6829) fix: piping
- 2025-11-07 [79bb22a](https://github.com/anomalyco/opencode/commit/79bb22a5735610e7d7691741865c22808436343c) ci: auto update zed extension
- 2025-11-20 [40ac254](https://github.com/anomalyco/opencode/commit/40ac2549ffaea94d47d619962d865a1fe683b5c4) fix: aur build
- 2025-11-21 [d16c8c9](https://github.com/anomalyco/opencode/commit/d16c8c9f0fda4bdd3b41c3cf9fca84daa3262e9a) ignore: update sdk
- 2025-12-11 [f7e29a1](https://github.com/anomalyco/opencode/commit/f7e29a1acf98531af0099cbfc90f1a983560af6d) downgrade bun
- 2025-12-22 [345f480](https://github.com/anomalyco/opencode/commit/345f4801e8fb67bf8a27b053187989aff47979b2) feat: add experimental lsp tool (#5886)
- 2025-12-31 [b9ef09a](https://github.com/anomalyco/opencode/commit/b9ef09a0f4e8c96b4549352b53a64cee4be32aa7) tweak: read plurals too and stop erroring on them
- 2026-01-05 [8b3ae08](https://github.com/anomalyco/opencode/commit/8b3ae08a5592f7244beb2f705d4a61fac88cbbaa) acp: handle case where big-pickle is unavailable as a fallback
- 2026-01-08 [c74bc32](https://github.com/anomalyco/opencode/commit/c74bc323b68493f6e94839903f13489974319edc) ci: tweak enforcement of titles
- 2026-01-10 [8b8a358](https://github.com/anomalyco/opencode/commit/8b8a358de1005970c3316c0a9ff0088ad1474232) update docs and auth methods for openai
- 2026-01-15 [9293143](https://github.com/anomalyco/opencode/commit/92931437c4ce48d2c4fdcad14067bff9a6f5d3ef) fix: codex id issue (#8605)
- 2026-01-29 [75166a1](https://github.com/anomalyco/opencode/commit/75166a1961f1963927b838c729bcd17f8acd0858) fix: use ?? to prevent args being undefined for mcp server in some cases (#11203)
- 2026-02-03 [b7b734f](https://github.com/anomalyco/opencode/commit/b7b734f51f8369ac34a5edb12d26d11309f1fc07) fix: ensure mcp tools are sanitized (#11984)
- 2026-02-05 [2f78705](https://github.com/anomalyco/opencode/commit/2f78705f6e91b6a775d544460246cea59b2a4068) tweak: update transforms for gpt-5.3 (#12325)
- 2026-02-20 [1a329ba](https://github.com/anomalyco/opencode/commit/1a329ba47d2f4c060a518f2396215467953cb8cb) fix: issue from structuredClone addition by using unwrap (#14359)
- 2026-04-07 [3c31d04](https://github.com/anomalyco/opencode/commit/3c31d046669ca8df09798f690ef5c9cf17021ddd) chore: bump anthropic ai sdk pkg, delete patch (#21247)
- 2026-04-11 [96c1c03](https://github.com/anomalyco/opencode/commit/96c1c0363d5c3ab5d5ef23bdd880b533f42fba14) chore: rm unnecessary test (now we use effect) and the test is flaky (#21959)
- 2026-04-12 [fc01cad](https://github.com/anomalyco/opencode/commit/fc01cad2b842a4025e7a04dc79130e843b1d56b7) fix: ensure logger cleanup properly orders list before deleting files (#22101)
- 2026-04-12 [8b9b9ad](https://github.com/anomalyco/opencode/commit/8b9b9ad31ee715301613f7254424590f0cc8805b) fix: ensure images read by agent dont count against quota (#22168)
- 2026-04-16 [143817d](https://github.com/anomalyco/opencode/commit/143817d44e1ededeccbaff3e61690dfea4fe4109) chore: bump ai sdk deps for opus 4.7 (#22869)
- 2026-04-16 [ae58433](https://github.com/anomalyco/opencode/commit/ae584332b36668ddfe4faa1b65157f5d276d34ad) fix: uncomment import (#22923)
- 2026-05-04 [c1f607d](https://github.com/anomalyco/opencode/commit/c1f607d206e7d723d8093650559fffb8a144738e) fix: ensure anthropic sdk properly resolves when using azure (#25721)
- 2026-06-11 [ca8db31](https://github.com/anomalyco/opencode/commit/ca8db315a998f84fbd9d5cfa5e06bb4fe86d3c03) fix(tui): show prompt submission errors (#31949)
- 2026-06-26 [ded29f0](https://github.com/anomalyco/opencode/commit/ded29f03f0d575086f11eede64b4f13f92756d08) fix(core): refine small model defaults (#33926)
- 2026-07-08 [ccb6b7c](https://github.com/anomalyco/opencode/commit/ccb6b7c3eac7146c6ed0f319baeb1772023a0a14) fix: improve xai cache hit rate (#35970)
- 2026-07-24 [2b2aacc](https://github.com/anomalyco/opencode/commit/2b2aacc93975330f9fd045d4306f698b0c6a8f8f) fix(provider): generalize Claude adaptive thinking (#38757)

## adamdotdevin (1580 commits; random sample of 30, seed 42)
types: fix (816); chore (279); wip (213); feat (173)
scopes: app (550); desktop (284); stats (128); data (20); core (18); share (15)
packages (sampled merged PRs): packages/app (91); packages/ui (48); packages/opencode (21); (root) (15); packages/stats/app (14); packages/stats/core (12)

- 2025-10-28 [1da24f6](https://github.com/anomalyco/opencode/commit/1da24f6adb7fce37af79678dd053090831fda2f2) wip: desktop work
- 2025-10-29 [cdeb82e](https://github.com/anomalyco/opencode/commit/cdeb82e9ca0213960202447896493c8d18cb867b) wip: desktop work
- 2025-10-30 [e944ff0](https://github.com/anomalyco/opencode/commit/e944ff028639de0e3263aa57cc0c6bafea64e292) wip: desktop work
- 2025-11-24 [8e1c7cf](https://github.com/anomalyco/opencode/commit/8e1c7cfe891a249c4c8dcc6fb3b588edb6879c3b) wip(share): enterprise favicon
- 2025-12-01 [bf8c866](https://github.com/anomalyco/opencode/commit/bf8c866bf79f1db3de6597f62d9f9694dafe7759) chore: otf fonts
- 2025-12-12 [ccdd770](https://github.com/anomalyco/opencode/commit/ccdd77032aedf9ce9fdf48ad74e79a9e308a370c) fix: desktop layout
- 2025-12-15 [d318243](https://github.com/anomalyco/opencode/commit/d31824320e9632cf7671210ce4d3c190a5bf1187) Revert "wip(desktop): session turn state consolidation"
- 2025-12-15 [4fd9a19](https://github.com/anomalyco/opencode/commit/4fd9a19fbbb815b54f10187c830e1979a4facf02) fix: keybinds for agent and model selection
- 2025-12-17 [204e3bf](https://github.com/anomalyco/opencode/commit/204e3bf3827670359d5c0bfaefc062ce8fd51b33) feat(desktop): inter and ibm plex mono
- 2025-12-18 [b787525](https://github.com/anomalyco/opencode/commit/b7875256f362d85e9644b376562e7525ee7191ae) feat(desktop): shell mode
- 2025-12-19 [26cf5e0](https://github.com/anomalyco/opencode/commit/26cf5e003ebd402b3343504dd6726a4c8cc9ffe3) fix(desktop): perf stuff
- 2025-12-25 [2178dee](https://github.com/anomalyco/opencode/commit/2178deef914fa35db957811076559907806dd054) fix(desktop): override agent model
- 2025-12-28 [ba3a1cf](https://github.com/anomalyco/opencode/commit/ba3a1cfa0bf3c08423a3b1974b5ad17f50797bb1) chore: cleanup
- 2025-12-30 [a7beba5](https://github.com/anomalyco/opencode/commit/a7beba5aa921c767470cdf20f7fefb5524b66f7a) chore(desktop): disable sourcemap
- 2025-12-31 [2ec6a21](https://github.com/anomalyco/opencode/commit/2ec6a21cc0018be6677e4cbad6bf48dbf8b37786) feat(desktop): unified diff toggle
- 2026-01-01 [2da71e0](https://github.com/anomalyco/opencode/commit/2da71e0a506f5094c90f8f69a72283588b8eec83) feat(desktop): lose the summaries
- 2026-01-01 [d1a4295](https://github.com/anomalyco/opencode/commit/d1a4295a3284e13ca715359c06e1245d926313d3) fix(util): checksum defensiveness
- 2026-01-21 [259b2a3](https://github.com/anomalyco/opencode/commit/259b2a3c2dbb785dd46c83e243f20436a05e3021) fix(app): japanese language support
- 2026-01-24 [fa51016](https://github.com/anomalyco/opencode/commit/fa510161f6851a316e087d4a1bbe041a4673dd3c) fix(app): missing translations
- 2026-01-26 [c1e840b](https://github.com/anomalyco/opencode/commit/c1e840b9b229889c1a85b479b3cd601d6f313c87) chore: cleanup
- 2026-01-27 [2180be2](https://github.com/anomalyco/opencode/commit/2180be2f3f6f414e0b40f983a0727e8d610f05ea) chore: cleanup
- 2026-01-27 [743e83d](https://github.com/anomalyco/opencode/commit/743e83d9bfc050183be5bfb0f241ebe5faac6b35) fix(app): agent fallback colors
- 2026-02-02 [2f76b49](https://github.com/anomalyco/opencode/commit/2f76b49df3cfd316069a2b5c292fed369acadbde) Revert "feat(ui): Smooth fading out on scroll, style fixes (#11683)"
- 2026-02-09 [ba740ea](https://github.com/anomalyco/opencode/commit/ba740eaefd42988deea68957cc881c6913431fae) fix: locale routing
- 2026-02-23 [3b5b21a](https://github.com/anomalyco/opencode/commit/3b5b21a91e2a8f084ee8ed85aca81246880a9384) fix(app): duplicate markdown
- 2026-06-05 [b36b859](https://github.com/anomalyco/opencode/commit/b36b85936d1276d13f80721313c292ad28178591) fix(stats): filter market share to go
- 2026-06-08 [f116a55](https://github.com/anomalyco/opencode/commit/f116a55e4a69407629ce1069b8e1220201442aa3) fix(stats): show new for leaderboard deltas
- 2026-07-08 [ccd8676](https://github.com/anomalyco/opencode/commit/ccd8676beeba33f3ec099b5121b42166f422144d) feat(stats): add model comparison home
- 2026-08-05 [146720e](https://github.com/anomalyco/opencode/commit/146720e197866afc2e799462374c42b51e12857b) fix(stats): improve page speed
- 2026-08-20 [ad192a5](https://github.com/anomalyco/opencode/commit/ad192a59b5517fb432bc5f4d27f131d605a22beb) fix(stats): clarify market share providers (#43647)

## thdxr (1293 commits; random sample of 30, seed 42)
types: core (147); ci (144); tui (123); fix (113)
scopes: core (58); tui (23); opencode (19); console (11); server (9); cli (8)
packages (sampled merged PRs): packages/opencode (85); packages/core (50); packages/tui (32); packages/cli (22); (root) (20); packages/sdk/js (18)

- 2025-10-09 [f3b7100](https://github.com/anomalyco/opencode/commit/f3b71007d2d05529a755dddf5e73a69507f8ada7) core: replace chokidar with @parcel/watcher for better performance and cross-platform support
- 2025-10-31 [d4cb47e](https://github.com/anomalyco/opencode/commit/d4cb47eadc515646f9f42679100a075ff9c9d458) tui: add keyboard shortcuts to cycle through recently used models
- 2025-11-01 [65d0b3e](https://github.com/anomalyco/opencode/commit/65d0b3ed6dc7873287cc103e88c51ff70ad7d24e) sync
- 2025-11-03 [07bb75f](https://github.com/anomalyco/opencode/commit/07bb75f086e3fd08b862feb94b360ba8141b6e66) core: add optional dirs parameter to file search API
- 2025-11-09 [c9adbc7](https://github.com/anomalyco/opencode/commit/c9adbc7c212313c2115720ab59e2b94614afb6d2) tui: add logging when creating project instances to help users debug startup issues
- 2025-11-11 [85f1589](https://github.com/anomalyco/opencode/commit/85f15893bcd528d3a1bf33681f42abe5210e7ebd) core: prevent crash when starting in repositories without any commits yet
- 2025-11-12 [d81dce6](https://github.com/anomalyco/opencode/commit/d81dce6a82d670d69e813a1bba67b9f06698d30b) fix: add support for loading custom themes from .opencode/themes directory (#4229)
- 2025-11-22 [e103fb1](https://github.com/anomalyco/opencode/commit/e103fb1f9363c796b51d2d009db5bd6c907ea53b) ci: add Node.js setup to deploy workflow for consistent runtime environment
- 2025-11-22 [78a6325](https://github.com/anomalyco/opencode/commit/78a6325b64951cbe4498ae48bf9e82d94c7d71f0) improve model footer
- 2025-11-22 [e03a411](https://github.com/anomalyco/opencode/commit/e03a41144a5b726438c0fd67cd27ae151acfb7b8) tui: keep assistant footer from crashing after compaction
- 2025-11-23 [bf81e31](https://github.com/anomalyco/opencode/commit/bf81e3108cfa87921875f292b39b801ec65a8466) ci: ignore
- 2025-11-29 [d808803](https://github.com/anomalyco/opencode/commit/d80880350d47e208a5d3739e5ec37a064f3aad66) core: improve explore agent description to clarify tool availability
- 2025-12-01 [993422c](https://github.com/anomalyco/opencode/commit/993422c6afc820471142cb39fb1a24ba7b64dfa0) core: prevent share tests from cleaning up storage used by other tests
- 2025-12-07 [0ecccbf](https://github.com/anomalyco/opencode/commit/0ecccbfd17a1098091a1a28d8b90940d6e487bb7) enable zoom hotkeys
- 2025-12-08 [bf0f85e](https://github.com/anomalyco/opencode/commit/bf0f85e37f8ccdf1a749a279f67161b63f5d3bc8) playing with sdk docs
- 2025-12-09 [edffcc3](https://github.com/anomalyco/opencode/commit/edffcc32cf30e92c3375d268eafe1831e551fdc2) core: make project updated timestamp optional to support legacy project data
- 2025-12-11 [d4dc142](https://github.com/anomalyco/opencode/commit/d4dc142cc26ffc0082efc85b093b405774f0aa56) core: add client identification to user agent and request headers for better tracking
- 2025-12-14 [c8fc910](https://github.com/anomalyco/opencode/commit/c8fc9105334bdfc80b9f773e4e8ad808b20b03af) ignore: simplify download page to use GitHub latest redirect URLs
- 2026-01-04 [cdd6ea5](https://github.com/anomalyco/opencode/commit/cdd6ea514b94d8ce71770bb02f33ab979e9ee0f6) core: improve Rust formatter detection and add cargo fmt support
- 2026-01-12 [1954c12](https://github.com/anomalyco/opencode/commit/1954c1255e71ab86283e6a99ac82598268fc308d) core: add password authentication and improve server security
- 2026-01-30 [97a428c](https://github.com/anomalyco/opencode/commit/97a428cf69ebc4a00201475f9437f97079779f64) ci
- 2026-02-02 [372dcc0](https://github.com/anomalyco/opencode/commit/372dcc033c71d0789db25dc7059a5dc9ea3f4cf6) ci: change trigger from scheduled cron to PR labeled events with beta label condition
- 2026-02-19 [568eccb](https://github.com/anomalyco/opencode/commit/568eccb4c654e83382253eb0c1478d24585288aa) Revert: all refactor commits migrating from Bun.file() to Filesystem module
- 2026-03-28 [81eb6e6](https://github.com/anomalyco/opencode/commit/81eb6e670be1baef2413ea18c0e60f92eedbe09e) refactor(prompt): remove variant cycle display from footer (#19489)
- 2026-04-16 [9db4099](https://github.com/anomalyco/opencode/commit/9db40996cc5ce5877565d99e4199656345e8b80f) fix build script
- 2026-04-17 [65b2a10](https://github.com/anomalyco/opencode/commit/65b2a10e97d34e6b8a832a69b5a75f90f21e076c) fade in prompt metadata transitions (#23037)
- 2026-04-19 [e60a6e3](https://github.com/anomalyco/opencode/commit/e60a6e3a829f3598aba3707efc843a5364f0dc69) fix: change Free download button text to Download (#23388)
- 2026-05-15 [e11e089](https://github.com/anomalyco/opencode/commit/e11e089e42200fee8399fdcf15946032411868ae) Add Effect-native core event system (#27415)
- 2026-06-10 [8a2cfc0](https://github.com/anomalyco/opencode/commit/8a2cfc00c93afc32a79979b7a928bc55d6483934) feat(core): add project reference guidance (#31601)
- 2026-06-26 [1ac6b4b](https://github.com/anomalyco/opencode/commit/1ac6b4bec46f876eca69310d9371488e497ae5c6) fix(core): authorize external read paths

## Hona (271 commits; random sample of 30, seed 42)
types: fix (184); feat (30); chore (11); test (11)
scopes: app (90); desktop (28); win32 (15); opencode (11); tui (7); test (6)
packages (sampled merged PRs): packages/app (77); packages/desktop (31); packages/opencode (25); packages/session-ui (25); (root) (22); packages/ui (19)

- 2025-12-16 [27e826e](https://github.com/anomalyco/opencode/commit/27e826eba63a3d8d1111f13dc4934d3f54d90cf8) fix(win32): Normalise LSP paths on windows (fixes lua) (#5597)
- 2025-12-19 [7f8e799](https://github.com/anomalyco/opencode/commit/7f8e79939217c504effa9ff20bb113941645d581) tweak: DevEx to run changelog independently (#5774)
- 2026-01-09 [eb5c113](https://github.com/anomalyco/opencode/commit/eb5c113cff63f2f2cad0174db6b05408caf115da) ignore: add PR template (#7391)
- 2026-02-09 [3d6fb29](https://github.com/anomalyco/opencode/commit/3d6fb29f0c14bfd924ad2e91e398bfa41adfc67e) fix(desktop): correct module name for linux_display in main.rs (#12862)
- 2026-02-24 [1af3e9e](https://github.com/anomalyco/opencode/commit/1af3e9e557a6df4f933a01d0dad2e52e418ebd52) fix(win32): fix plugin resolution with createRequire fallback (#14898)
- 2026-03-07 [4c7fe60](https://github.com/anomalyco/opencode/commit/4c7fe6049346def59e06c676c6bdec1a72c72470) fix(opencode): sanitize preview database filenames (#16430)
- 2026-03-12 [d481f64](https://github.com/anomalyco/opencode/commit/d481f64bdeaca91226e66c0e7888c7a10ba631f7) fix(electron): theme Windows titlebar overlay (#16843)
- 2026-03-13 [3998df8](https://github.com/anomalyco/opencode/commit/3998df8112d398c2c5cd6253d352e4660589927e) fix(app): increase CI e2e workers (#17263)
- 2026-03-16 [4d7cbdc](https://github.com/anomalyco/opencode/commit/4d7cbdcbef92bb69613fe98ba64e832b5adddd79) fix(ci): workaround by using hoisted Bun linker on Windows (#17751)
- 2026-03-20 [d460614](https://github.com/anomalyco/opencode/commit/d460614cd7ad9e047a2792139ea67e16caa82ea7) fix: lots of desktop stability, better e2e error logging (#18300)
- 2026-03-23 [afe9b97](https://github.com/anomalyco/opencode/commit/afe9b9727415ea046dc08990f981e00e27ec4a43) fix(app): restore keyboard project switching in open sidebar (#18682)
- 2026-03-25 [9a64bdb](https://github.com/anomalyco/opencode/commit/9a64bdb5397dc7c75eeb7053f0024e2c89636a2c) fix: beta resolver typecheck + build smoke check (#19060)
- 2026-04-01 [fa96cb9](https://github.com/anomalyco/opencode/commit/fa96cb9c6e7d0cafad065066c00c2119b94b68d9) Fix selection expansion by retaining focused input selections during global key events (#20205)
- 2026-04-06 [a4a9ea4](https://github.com/anomalyco/opencode/commit/a4a9ea4ab00fe58d35ea6fc4971bd47cb9e5b0ac) fix(tui): revert kitty keyboard events workaround on windows (#20180)
- 2026-04-10 [b16ee08](https://github.com/anomalyco/opencode/commit/b16ee08fd5c867fa1402237d53bb43d62a61cff2) ci use node 24 in test workflow fixing random ECONNRESET (#21782)
- 2026-04-14 [f9d99f0](https://github.com/anomalyco/opencode/commit/f9d99f044df4d506aac897a1c27d1a0b1f894ae9) fix(session): keep GitHub Copilot compaction requests valid (#22371)
- 2026-04-22 [69b8ea0](https://github.com/anomalyco/opencode/commit/69b8ea0d66ce9a57e4692278853fb67cc163a67b) chore: bump Bun to 1.3.13 (#23791)
- 2026-04-22 [20756e0](https://github.com/anomalyco/opencode/commit/20756e0ee45885db5614dab1867f16cea70ca1ec) test: fix cross-spawn stderr race on Windows CI (#23808)
- 2026-04-23 [ac26394](https://github.com/anomalyco/opencode/commit/ac26394fcb280592a8ecddf903a3a7116c841f39) fix(beta): PR resolvers/smoke check should typecheck all pacakges (#23913)
- 2026-05-06 [2dffdff](https://github.com/anomalyco/opencode/commit/2dffdfff4aa02d5c4df128035d0bfce2fd309ebd) fix(server): apply cors before legacy auth (#26092)
- 2026-05-12 [caf1151](https://github.com/anomalyco/opencode/commit/caf1151cb5d574d2aac2ed6ccb20a9121880c18a) refactor(app): centralize sync query options (#25941)
- 2026-05-28 [e16bfd7](https://github.com/anomalyco/opencode/commit/e16bfd745d7d5b3ea949abec79cf95f37e71cc20) fix(app): start MCP servers only for open directories (#28937)
- 2026-06-09 [5372c63](https://github.com/anomalyco/opencode/commit/5372c63c7c4aab159c96325167c0678fb7b02bfc) fix(app): clip rounded session panels (#31462)
- 2026-06-10 [e0449c0](https://github.com/anomalyco/opencode/commit/e0449c0b9647c60810ccfa72a6367051a37ac359) fix(desktop): restore macOS auto-updates (#31621)
- 2026-06-25 [cfd75d6](https://github.com/anomalyco/opencode/commit/cfd75d62fe91f8953c815a0e4432c12850b9fa2a) fix(app): separate provider lifetimes and reactive ownership (#33739)
- 2026-06-27 [3d07211](https://github.com/anomalyco/opencode/commit/3d072112ceff73d7f767dec9f05bc3c1e77bc3b1) fix(app): reconcile session pages with concurrent events (#34042)
- 2026-07-14 [7304bb2](https://github.com/anomalyco/opencode/commit/7304bb2751ae69e0011ebc51f5a85bdc53eb52e9) fix(app): resync timeline after route reconnect (#36643)
- 2026-07-20 [43e472b](https://github.com/anomalyco/opencode/commit/43e472bba70728d79ad9a2003067b08104e24f7a) feat(app): sync embedded terminal theme (#37931)
- 2026-07-31 [2039c90](https://github.com/anomalyco/opencode/commit/2039c90c06f8431cc32dca1adc716eada02af057) fix(desktop): open external links in system browser (#39820)
- 2026-08-05 [66fdd51](https://github.com/anomalyco/opencode/commit/66fdd51f0d6db8e47e876721c855ea155043b74c) docs: add RTL development skill (#40543)

## Brendonovich (418 commits; random sample of 30, seed 42)
types: fix (157); feat (50); desktop (43); refactor (34)
scopes: app (161); desktop (45); ui (8); desktop-electron (3); electron (2); session-ui (2)
packages (sampled merged PRs): packages/app (107); packages/desktop (27); packages/session-ui (18); packages/ui (17); (root) (13); packages/opencode (6)

- 2025-12-19 [d03fac5](https://github.com/anomalyco/opencode/commit/d03fac52e72babf7a83042cc05383ca2ca2d8fa2) Update SolidStart and bring back HttpHeader usage (#5355)
- 2025-12-22 [cb1a1fb](https://github.com/anomalyco/opencode/commit/cb1a1fb26c0f496165cde0991a8bab1a038f0f93) try uploading artifacts in workflow
- 2025-12-22 [11a92b2](https://github.com/anomalyco/opencode/commit/11a92b24c2a6b3dba1938039d44b343a1fe492e9) ci: run prepare step for tauri build
- 2026-01-07 [a41c850](https://github.com/anomalyco/opencode/commit/a41c8508daa315e4ce136f63244edc7cdaa895b2) desktop: go back to regular tauri cli
- 2026-01-13 [b01eec3](https://github.com/anomalyco/opencode/commit/b01eec38d1b0bec375f46a562f8a39f9c72e53ff) fix(desktop): set serverPassword
- 2026-01-21 [d00b8df](https://github.com/anomalyco/opencode/commit/d00b8df7707c0a4ad94ce7a3488780fe5764ae6c) feat(desktop): properly integrate window controls on windows (#9835)
- 2026-02-03 [b7bd561](https://github.com/anomalyco/opencode/commit/b7bd561eaafa05d5176bdc177236d1e1bfecb990) ci: use numeric release id instead of gql one
- 2026-02-06 [e0e32ed](https://github.com/anomalyco/opencode/commit/e0e32ed3a846be03f66d2f48e2b2659a9e658a11) desktop: add more basic menu bar items
- 2026-02-18 [6cd3a59](https://github.com/anomalyco/opencode/commit/6cd3a5902260764899a566b33d7f76123b9c9800) desktop: cleanup
- 2026-03-18 [4ba7d3b](https://github.com/anomalyco/opencode/commit/4ba7d3b4062b090ea41c2674a3141c2a66def561) app: replace autoselect effects with single resource
- 2026-03-25 [3ea72ae](https://github.com/anomalyco/opencode/commit/3ea72aec21e6266a69cda23b5705c6e8e7e19186) app: pre-warm project globalSync state when navigate project via keybind (#19088)
- 2026-04-15 [d7718d4](https://github.com/anomalyco/opencode/commit/d7718d41d465cc1e84bc4d6c2e81af8baf46a23e) refactor(electron): update store configuration (#22597)
- 2026-04-21 [e5687d6](https://github.com/anomalyco/opencode/commit/e5687d646ce33b5c05bb007bf14cf5362676733b) electron: use custom oc:// protocol for renderer windows (#23516)
- 2026-04-24 [3bfe6a1](https://github.com/anomalyco/opencode/commit/3bfe6a1ef6cf41bc7f05339d63ab8d6032c6e8e1) ci: add platform-specific bun install flags (#23822)
- 2026-04-29 [f6b4f54](https://github.com/anomalyco/opencode/commit/f6b4f542162a6db7a630db359926a3a82a566159) refactor(app): convert getProjectAvatarSource to early returns (#24896)
- 2026-05-06 [efd8024](https://github.com/anomalyco/opencode/commit/efd8024430f8ec6d90086688bf6bb259a1b5af4c) feat(desktop): add OPENCODE_TEST_ONBOARDING env (#25968)
- 2026-05-12 [ff38bbe](https://github.com/anomalyco/opencode/commit/ff38bbeeeb64c7f2faaae54430b5bfa3a2f5435f) refactor(desktop): remove configureEnv callback from spawnLocalServer (#27022)
- 2026-05-13 [ad6a8a1](https://github.com/anomalyco/opencode/commit/ad6a8a185045d691b3f293b6a02629bcb9e61740) fix: use htmlrewriter2 instead of HTMLRewriter for node compat (#26309)
- 2026-05-22 [1f0390c](https://github.com/anomalyco/opencode/commit/1f0390cfbbd931297faa4dbc6d297ef2665a4a84) app: wrap provider data in Map to avoid store (#28765)
- 2026-05-25 [9495ecd](https://github.com/anomalyco/opencode/commit/9495ecd536740dcf310440859fafb52dc3bf9382) refactor(app): extract refcount utility and clean up server sdk context (#29155)
- 2026-06-23 [b006370](https://github.com/anomalyco/opencode/commit/b0063709e6107be835569c103024cec3d8ce6dcf) feat(app): add mobile bottom navigation (#32797)
- 2026-06-23 [a21e747](https://github.com/anomalyco/opencode/commit/a21e74773f281b1f414a543a5584b36585d28f30) fix(app): improve iOS PWA shell (#32798)
- 2026-06-25 [fed8f01](https://github.com/anomalyco/opencode/commit/fed8f0101696d8728b99cbd562dd24e13ea02ffe) refactor(app): lift parent session navigation (#33782)
- 2026-06-30 [c8fde60](https://github.com/anomalyco/opencode/commit/c8fde60ad4e977b6498a40a1c5dde24e0df5d6af) fix(app): hide missing workspace branch (#34649)
- 2026-07-02 [5fecf7a](https://github.com/anomalyco/opencode/commit/5fecf7ae9e6c23ca7410bf23e4438b74387c0fd2) fix(app): only allow \(...\) syntax for inline latex (#34850)
- 2026-07-24 [ce9a875](https://github.com/anomalyco/opencode/commit/ce9a875181b8ac7507e7eb84245b28ed31d75477) feat(app): render current session timeline (#38466)
- 2026-07-24 [29af2e3](https://github.com/anomalyco/opencode/commit/29af2e39ff7e35e24ea6ece72dbdafabbaaaf15d) feat(app): migrate session interactions (#38461)
- 2026-07-24 [55f4a26](https://github.com/anomalyco/opencode/commit/55f4a2691ae9e72a84c821d789f0912353197cbe) fix(app): preserve paginated timeline order (#38641)
- 2026-07-28 [c39ad38](https://github.com/anomalyco/opencode/commit/c39ad38424325423b195e794e63fd5b7cf314ec7) fix(app): refresh global provider state (#39220)
- 2026-08-04 [c75e3b5](https://github.com/anomalyco/opencode/commit/c75e3b5bae57206eac3f3e3986568d7f7a87acb5) fix(app): supply fallback session titles (#40385)

## nexxeln (257 commits; random sample of 30, seed 42)
types: fix (124); refactor (51); feat (47); test (14)
scopes: acp (27); flags (21); app (16); tui (16); core (14); httpapi (14)
packages (sampled merged PRs): packages/opencode (70); packages/core (43); packages/ai (31); packages/app (12); packages/client (10); packages/ui (10)

- 2026-03-08 [a139e92](https://github.com/anomalyco/opencode/commit/a139e9297d2a269308c66efbc7ed2b7a53a59a16) fix: prune and evict stale app session caches (#16584)
- 2026-03-19 [e6f5214](https://github.com/anomalyco/opencode/commit/e6f521477959b1009153d8310fb90ac41d766b8b) feat: add git-backed session review modes (#17961)
- 2026-03-27 [a93374c](https://github.com/anomalyco/opencode/commit/a93374c48f724ebbc99886ac5d607a3990090b75) fix(ui): make streamed markdown feel more continuous (#19404)
- 2026-04-07 [3a1ec27](https://github.com/anomalyco/opencode/commit/3a1ec27feba5129623f2c916f6de31ddb43e535e) feat(app): show full names on composer attachment chips (#21306)
- 2026-04-14 [5b60e51](https://github.com/anomalyco/opencode/commit/5b60e51c9f36fd21036cff3012d90dd52cfa299e) fix(opencode): resolve ripgrep worker path in builds (#22436)
- 2026-05-10 [5cf9abe](https://github.com/anomalyco/opencode/commit/5cf9abe7437b643e1b6e83edf42dd795a81768a3) feat(scout): materialize configured reference repos (#26692)
- 2026-05-12 [3dc2c1d](https://github.com/anomalyco/opencode/commit/3dc2c1d81c3ac6a0e508ddf3cf918e76888faf08) fix(session): preserve usage update timestamps (#27094)
- 2026-05-12 [45de497](https://github.com/anomalyco/opencode/commit/45de4975dec676e398a308149ae62bac5e017ce8) refactor(core): resolve default agent info (#27125)
- 2026-05-13 [fed043a](https://github.com/anomalyco/opencode/commit/fed043a1ade7e51c5e8ac367bf56661f812becb5) fix(session): add typed message lookup wrappers (#27269)
- 2026-05-13 [b0dc8e4](https://github.com/anomalyco/opencode/commit/b0dc8e4638fef6c96665d68d2268a14443e7f05b) fix(session): use typed message reads in tools (#27280)
- 2026-05-13 [2e7cf92](https://github.com/anomalyco/opencode/commit/2e7cf92c8b04f16c899eb0daf693c092c3698c7a) fix(worktree): type expected errors (#27296)
- 2026-05-13 [a4ebb07](https://github.com/anomalyco/opencode/commit/a4ebb07c25ce58ecc5599555267ea196ea8f21a1) refactor(flags): route llm client through runtime flags (#27368)
- 2026-05-13 [9ee1f6c](https://github.com/anomalyco/opencode/commit/9ee1f6ceba8964c7db34fb9429533f67269ef089) fix(server): map busy sessions in http handlers (#27375)
- 2026-05-14 [e26abd8](https://github.com/anomalyco/opencode/commit/e26abd8da9d28d0e79a481a1e0c0d6b0a70f05ee) fix(tool): close shell truncation stream (#27517)
- 2026-05-14 [e22cfa4](https://github.com/anomalyco/opencode/commit/e22cfa435a7ce3ba62417944ec58cd5ab1f405c4) refactor(lsp): move ty flag to runtime flags (#27610)
- 2026-05-14 [d35e09f](https://github.com/anomalyco/opencode/commit/d35e09f1fc77372932458547cf28fa701b1950e2) test(workspace): use runtime flags in workspace tests (#27612)
- 2026-05-15 [22cb039](https://github.com/anomalyco/opencode/commit/22cb0395e2f601923a44a6d2093493de7eb1cfce) refactor(flags): migrate external skills flag (#27685)
- 2026-05-15 [e653838](https://github.com/anomalyco/opencode/commit/e65383810a3734024895e41944f6ae261366fe38) refactor(tool): read repo overview directory from instance state (#27717)
- 2026-05-15 [fa9a2cb](https://github.com/anomalyco/opencode/commit/fa9a2cb24d9759d1830a5e87e9bc781d33120362) refactor(instance): remove remaining bind call sites (#27731)
- 2026-05-15 [a2392ca](https://github.com/anomalyco/opencode/commit/a2392ca60d974e5d4de907ec04f79e1668141225) refactor(worktree): provide runtime reentry refs (#27754)
- 2026-05-20 [4308dd7](https://github.com/anomalyco/opencode/commit/4308dd75fb303e2f3dee12d8a75f863c4ac884ad) fix(httpapi): expose v2 catalog errors (#28498)
- 2026-05-21 [2697cb8](https://github.com/anomalyco/opencode/commit/2697cb8001cdcf0a6df888e4478c519244a32585) fix(httpapi): remove config error middleware special case (#28631)
- 2026-05-22 [4ce247e](https://github.com/anomalyco/opencode/commit/4ce247eabaaf2bb3dbcbd45bf416e3b535dd018a) fix(httpapi): return request not found errors (#28693)
- 2026-05-26 [717e74f](https://github.com/anomalyco/opencode/commit/717e74f3e51ccf64851433b256f9fec7a124bd87) feat(acp): stream acp-next tool updates (#29333)
- 2026-05-29 [8f8b161](https://github.com/anomalyco/opencode/commit/8f8b161cae248687810e80477ef3baa561e099bb) feat(tui): add session switcher plugin (#29861)
- 2026-06-04 [6d4f3b4](https://github.com/anomalyco/opencode/commit/6d4f3b4ab26094f0a6f8b1678f2aada3e5a36c41) feat(tui): improve experimental session switcher (#30738)
- 2026-06-14 [3e523d5](https://github.com/anomalyco/opencode/commit/3e523d506c93aa276e129c48e5ee953478fbdddd) fix(tui): match @ mention items by name, not description or uri (#32309)
- 2026-06-26 [971518c](https://github.com/anomalyco/opencode/commit/971518c6d93d191a713df05d79f6aa0afdd3093b) fix(core): refresh cached remote skills (#34059)
- 2026-06-26 [6e8e772](https://github.com/anomalyco/opencode/commit/6e8e772582360f8069408d6aea7b4b179c825d22) fix(acp): surface prompt errors (#34061)
- 2026-06-29 [f7eeb08](https://github.com/anomalyco/opencode/commit/f7eeb089427a609d701353414701c0db80af3ead) fix(llm): narrow raw overlays (#34448)

## iamdavidhill (568 commits; random sample of 30, seed 42)
types: fix (206); tweak (73); tui (39); feat (26)
scopes: ui (97); app (94); console (15); tui (4); desktop (1); util (1)
packages (sampled merged PRs): packages/app (63); packages/ui (25); packages/opencode (9); packages/tui (6); packages/session-ui (5); packages/desktop (4)

- 2025-10-02 [f922988](https://github.com/anomalyco/opencode/commit/f9229889a16d0817d64c55e9e1dc031d52fc4d05) Mobile nav fix
- 2025-10-02 [b35c6b9](https://github.com/anomalyco/opencode/commit/b35c6b9fffbe3d32252ee5e4845c927b18d6225a) Merge branch 'dev' of https://github.com/sst/opencode into dev
- 2025-10-02 [6022d12](https://github.com/anomalyco/opencode/commit/6022d12ea215c040a273018c9b3a37860d0a018d) Update dock.png
- 2025-12-02 [58b30d6](https://github.com/anomalyco/opencode/commit/58b30d678a910378a554b2ab580da4a2e17796cd) Merge branch 'dev' of https://github.com/sst/opencode into dev
- 2025-12-02 [39d5bdf](https://github.com/anomalyco/opencode/commit/39d5bdff4bc5600e65debeadc94a27b025dfd9bb) fix: add docs button
- 2025-12-11 [4e02704](https://github.com/anomalyco/opencode/commit/4e02704f17be406a7cec8b5a815582a08a2043cf) Merge branch 'dev' of https://github.com/sst/opencode into dev
- 2025-12-16 [6c1a1a7](https://github.com/anomalyco/opencode/commit/6c1a1a77b762dbde875f0325ca43903c2c0840b6) fix: strip parentheses from file paths generated by llm
- 2025-12-16 [9f3bc0e](https://github.com/anomalyco/opencode/commit/9f3bc0e35204dd1e8c24de5f46b9bae4b9f5974a) Merge branch 'dev' of https://github.com/sst/opencode into dev
- 2025-12-16 [a190eda](https://github.com/anomalyco/opencode/commit/a190eda2c897491cea569c533f4947db9db67b40) Merge branch 'dev' of https://github.com/sst/opencode into dev
- 2025-12-23 [59b87f6](https://github.com/anomalyco/opencode/commit/59b87f60f72e547d35a16a37bb93ea3719eaa67b) Add animated braille spinner to terminal title when agent is running (#5984)
- 2026-01-15 [fe2cc0c](https://github.com/anomalyco/opencode/commit/fe2cc0cff158a844f0ec84ede7b9f95753ef9301) fix: archive icon replaces diff count on hover
- 2026-01-16 [7042767](https://github.com/anomalyco/opencode/commit/704276753ba4a6f1ecd6d3044aefee452914e399) bug: moved createMemo down
- 2026-01-17 [dfa2a9f](https://github.com/anomalyco/opencode/commit/dfa2a9f22556148390551b6581b1aaa6c4ded2e5) fix: reduce command item left padding in search modal
- 2026-01-19 [b079417](https://github.com/anomalyco/opencode/commit/b0794172bffd58bc7e1d1fef6b999259f798edf9) update: tighten edit project color spacing
- 2026-01-19 [2dbdd18](https://github.com/anomalyco/opencode/commit/2dbdd18483a6cfbda7c965e8f7b4df89d39d5432) add hover overlay with upload/trash icons to project icon in edit dialog
- 2026-01-20 [ecae24f](https://github.com/anomalyco/opencode/commit/ecae24f426d61d4e221cb6eae91debf4f9480548) use medium font weight for settings tab labels
- 2026-01-20 [9ffb714](https://github.com/anomalyco/opencode/commit/9ffb7141e5de14d38ce4d9e2e427caac7437416b) set select dropdown border-radius to 8px
- 2026-01-20 [0d9ce6a](https://github.com/anomalyco/opencode/commit/0d9ce6ad7bc3093e78f945124fcdf2e8c18c3481) set settings sidebar width to 200px
- 2026-01-20 [2111473](https://github.com/anomalyco/opencode/commit/2111473746fc82b4ace93ea88a141edd6ba7efd3) fix: remove close delay on hover cards to stop overlapping
- 2026-01-20 [80dc74a](https://github.com/anomalyco/opencode/commit/80dc74a0ec7c5bd3a03f95a8fd658993d302da2a) add keyboard shortcut (mod+,) to open settings dialog
- 2026-01-20 [3b46f90](https://github.com/anomalyco/opencode/commit/3b46f90124d362aaa553d6cfb0ff3811302a5921) fix: icon size in sidbar
- 2026-01-23 [d3490cf](https://github.com/anomalyco/opencode/commit/d3490cfd29c82d6d12a9b89d5568e79e226b8e9c) feat(ui): add close-small icon and use it for comment card dismiss button
- 2026-01-24 [af6bd9d](https://github.com/anomalyco/opencode/commit/af6bd9d3b1fd05ce089a56d3cf6a6bcb1e7badd8) fix(ui): style comment popovers - 14px radius, move label below, use text-weak for label, text-strong 14px for...
- 2026-02-18 [db4ff89](https://github.com/anomalyco/opencode/commit/db4ff895793d61e7a99e1c6c86f6d50bf4a854c6) Update oc-2.json
- 2026-02-19 [40f00cc](https://github.com/anomalyco/opencode/commit/40f00ccc1c269a31a761617d42f47330eb6ade8d) tweak(ui): use chevron icons for review diff rows
- 2026-03-04 [de6a6af](https://github.com/anomalyco/opencode/commit/de6a6af5ab6986d05971728848f1ac802ad75f34) tweak(ui): remove section
- 2026-03-04 [40fc406](https://github.com/anomalyco/opencode/commit/40fc406424c27a00f0841af46f6ec96572c973f7) ci: make tsgo available for pre-push typechecks
- 2026-03-09 [399b8f0](https://github.com/anomalyco/opencode/commit/399b8f0701f04ded3114e81cd74e6024bef1a50d) fix(app): session title turn spinner (#16764)
- 2026-03-10 [f77e5cf](https://github.com/anomalyco/opencode/commit/f77e5cf8fb9d9babcfc1b3ba046c0ba571489647) feat(ui): restyle Card and improve tool error cards (#16888)
- 2026-03-13 [536abea](https://github.com/anomalyco/opencode/commit/536abea2e2ed3bee160cf16a12b23558ed6e3fa3) fix(app): restore sidebar dash and sync session spinner colors (#17384)

## jlongster (145 commits; random sample of 30, seed 42)
types: fix (53); refactor (39); feat (37); test (9)
scopes: core (74); tui (34); opencode (22); config (1); core,tui (1); plugin (1)
packages (sampled merged PRs): packages/opencode (57); packages/tui (57); packages/core (34); packages/server (14); packages/client (11); packages/schema (10)

- 2026-02-24 [2c00eb6](https://github.com/anomalyco/opencode/commit/2c00eb60bdc6e6ff0362e792e731eaa39204bf72) feat(core): add workspace-serve command (experimental) (#14960)
- 2026-03-10 [4c4aed5](https://github.com/anomalyco/opencode/commit/4c4aed5a875e44aec2856c117fa05e861fa62bb5) fix(core): make worktrees read the project id from local workspace (#16795)
- 2026-03-14 [8c53b2b](https://github.com/anomalyco/opencode/commit/8c53b2b47033c579b46b02b1ba9638004de0154f) fix(core): increase default chunk timeout from 2 min to 5 min (#17490)
- 2026-03-19 [d69962b](https://github.com/anomalyco/opencode/commit/d69962b0f7ca54494452dd902053088f8113809d) fix(core): disable chunk timeout by default (#18264)
- 2026-03-27 [4b9660b](https://github.com/anomalyco/opencode/commit/4b9660b211aa57477b6baa1848e582d3279f4db7) refactor(core): move more responsibility to workspace routing (#19455)
- 2026-04-10 [42206da](https://github.com/anomalyco/opencode/commit/42206da1f8f42add18b6a107f5df9b96a3bd59f9) refactor(tui): switch to global events and start passing workspace param (#21719)
- 2026-04-16 [074ef03](https://github.com/anomalyco/opencode/commit/074ef032eef2cb6a9a9b8dde5626ad5c0080d808) feat(core): add fence to make all methods strongly consistent when syncing (#22679)
- 2026-04-16 [305460b](https://github.com/anomalyco/opencode/commit/305460b25fc673f707a238f180d93e58d80f1ee9) fix: add a few more tests for sync and session restore (#22837)
- 2026-04-16 [a8d8a35](https://github.com/anomalyco/opencode/commit/a8d8a35cd3033602befc6648d00ed6be37aed826) feat(core): pass auth data to workspace (#22897)
- 2026-04-17 [0bedea5](https://github.com/anomalyco/opencode/commit/0bedea52b19515c69057866ec958769004147f66) fix(tui): tui resiliency when workspace is dead, disable directory filter in session list (#23013)
- 2026-04-17 [0f80c82](https://github.com/anomalyco/opencode/commit/0f80c827ed2f0ee32f8dfb8812605ed64ab47254) feat(core): exponential backoff of workspace reconnect (#23083)
- 2026-04-17 [aa05b9a](https://github.com/anomalyco/opencode/commit/aa05b9abe5ff3a4b29b72fb878be969567eda5bb) fix(core): pass OTEL config to workspace env (#23154)
- 2026-04-23 [bbf67d0](https://github.com/anomalyco/opencode/commit/bbf67d0fff28b8d26d7ffb11c347f519308944b0) fix(tui): render all non-synthetic text parts of a user message (#24009)
- 2026-04-23 [98ea5b6](https://github.com/anomalyco/opencode/commit/98ea5b6e7e907b6d4eb52b294b9a7770bbd0f18e) feat(tui): support builtin protocol for handling context from editors (#24034)
- 2026-04-30 [53e9cac](https://github.com/anomalyco/opencode/commit/53e9cac383859f7bb33f771db3ac6967cf4a98a7) refactor(core): convert control-plane workspace to Effect (#25018)
- 2026-05-15 [af06e52](https://github.com/anomalyco/opencode/commit/af06e5270889b6f10bc2c32210429d3723e452dd) fix(session): ignore instruction lookup errors (#27656)
- 2026-05-18 [ff9d7ca](https://github.com/anomalyco/opencode/commit/ff9d7cab5c6abb8535bd2095abd1f869be968244) fix(core): fix file references in workspaces (#28209)
- 2026-05-18 [12ae223](https://github.com/anomalyco/opencode/commit/12ae22378f962270cb20c2283d98d0c9781520c4) fix(plugin): `ask` in tools from plugins returns promise instead of effect (#28217)
- 2026-05-20 [17d66ee](https://github.com/anomalyco/opencode/commit/17d66ee4fed78643c093b1ffeb19dccd12614c65) feat(tui): initial impl of diff viewer (#28476)
- 2026-05-22 [69e4f52](https://github.com/anomalyco/opencode/commit/69e4f5227232df49211baa6ace143a1b9b86416c) fix(tui): interaction improvements to diff viewer (#28851)
- 2026-06-03 [147c6c4](https://github.com/anomalyco/opencode/commit/147c6c4d5137f5539a08fb98e53ba72aa00b06c2) feat(core): project copying and tracking directories (#30139)
- 2026-06-05 [d5b2056](https://github.com/anomalyco/opencode/commit/d5b205657fcd990abaaf9a8d578f1f99d404c92b) fix(tui): inject reminder after moving session (#31027)
- 2026-06-12 [c2e6b18](https://github.com/anomalyco/opencode/commit/c2e6b18076676dce554f4765eca3ebf8fcc398c1) feat(core): refactor project copies for v2 (#31943)
- 2026-06-21 [d3bbfff](https://github.com/anomalyco/opencode/commit/d3bbfff826c58708bb55ef11737943436305da7b) test(opencode): simplify message pagination layer wiring (#33157)
- 2026-06-29 [be14739](https://github.com/anomalyco/opencode/commit/be14739cce49f057c35e540cdaef05caa20256ab) refactor(core): convert config tests to nodes (#34474)
- 2026-06-30 [5b62211](https://github.com/anomalyco/opencode/commit/5b6221116789b461f3b49a8077bb8dcf1b7bdee1) refactor(opencode): build runtimes from layer nodes (#34515)
- 2026-06-30 [1f37c26](https://github.com/anomalyco/opencode/commit/1f37c265747ce233a9690c6e664dde3a60e8086a) refactor(opencode): use layer nodes in remaining harnesses (#34516)
- 2026-06-30 [451876b](https://github.com/anomalyco/opencode/commit/451876b0bddae9c48ea2c14651d9741e364870d7) refactor(opencode): remove core service layer exports (#34518)
- 2026-06-30 [c86066e](https://github.com/anomalyco/opencode/commit/c86066efef4f6853d632c1887bb87084f76ead2f) test(opencode): update help snapshots (#34639)
- 2026-07-07 [254a481](https://github.com/anomalyco/opencode/commit/254a481e5d06914a1ec590cbfaa07e65ebf9d20c) fix(config): handle unavailable config directories (#35632)

## fwang (481 commits; random sample of 30, seed 42)
types: zen (188); wip (93); go (28); docs (10)
scopes: console (8); web (3); zen (1)
packages (sampled merged PRs): packages/console/app (3); packages/console/core (3); packages/web (2); (root) (1); infra/ (1); packages/console/function (1)

- 2026-01-05 [9c55cb7](https://github.com/anomalyco/opencode/commit/9c55cb729bc28c86ca03f0e30e5ff19b4a0a8ea4) zen: add index
- 2026-01-07 [947b864](https://github.com/anomalyco/opencode/commit/947b864d96cbdeb4206920f1a7ed29305602dbc8) wip: zen
- 2026-01-09 [52fbd16](https://github.com/anomalyco/opencode/commit/52fbd16e08fe59d2d0a7a588dfa565a8ff2d2c95) wip: zen
- 2026-01-22 [5f3ab93](https://github.com/anomalyco/opencode/commit/5f3ab9395fc8f8543724dcda914d38fba0809049) wip: zen black
- 2026-02-10 [eb25878](https://github.com/anomalyco/opencode/commit/eb2587844b1ac89d6b888467215f3a5420fc7037) zen: retry on 429
- 2026-02-22 [5712cff](https://github.com/anomalyco/opencode/commit/5712cff5c453a185ac75a160f76ca06135d6ab2d) zen: track session in usage
- 2026-02-24 [fb6d201](https://github.com/anomalyco/opencode/commit/fb6d201ee03d73967c554394742be360e2ff782d) wip: zen lite
- 2026-03-07 [0654f28](https://github.com/anomalyco/opencode/commit/0654f28c7296cf31de10a6399bf82d43ef626202) zen: fix graph legend
- 2026-03-11 [4a81df1](https://github.com/anomalyco/opencode/commit/4a81df190c58c29418d8c32e9402cf71afa61bc8) zen: add alipay for go sub
- 2026-03-18 [3558deb](https://github.com/anomalyco/opencode/commit/3558deba4a619082cf3e2cae42da33f0b50ba68e) zen: minimax m2.7
- 2026-03-18 [1b0096b](https://github.com/anomalyco/opencode/commit/1b0096bf61e47d54b8afecb965e553fdc1d1e2ed) docs: update go models
- 2026-03-19 [bd44489](https://github.com/anomalyco/opencode/commit/bd44489ada70cf908b69466f623ca74e800b3fc7) go: upi payment
- 2026-03-24 [a03a2b6](https://github.com/anomalyco/opencode/commit/a03a2b6eab82725ab380547c08847f016bbf1d8a) Zen: adjust cache tokens
- 2026-03-26 [f7c2ef8](https://github.com/anomalyco/opencode/commit/f7c2ef876f3ba261380ae37bac7ad5805b61d80b) wip: zen
- 2026-03-29 [963dad7](https://github.com/anomalyco/opencode/commit/963dad75ef0b76bac08376e2cc3741e7f7d06f02) ci: fix
- 2026-03-30 [e7ff0f1](https://github.com/anomalyco/opencode/commit/e7ff0f17c88566222bcb7a2114cec9a51566753a) zen: qwen3.6 plus
- 2026-04-20 [ae7a351](https://github.com/anomalyco/opencode/commit/ae7a3518f789caf9d1f39dfb7848fa44005e36a0) zen: tpm based routing
- 2026-04-24 [4dab2a8](https://github.com/anomalyco/opencode/commit/4dab2a8555dabdc40109cc79d7528fca452bbdab) zen: gpt-5.5
- 2026-05-08 [6869186](https://github.com/anomalyco/opencode/commit/6869186fc69983becd55f2a9ec6f9c623037d3fc) zen: update tpm rate limit algo
- 2026-05-13 [655b25b](https://github.com/anomalyco/opencode/commit/655b25bccf3f31dfd14a8626bf8119b6905275ea) sync
- 2026-05-21 [39e7ff9](https://github.com/anomalyco/opencode/commit/39e7ff932d5652049f12fd7be5f813e9c42175ea) sync
- 2026-05-21 [5671432](https://github.com/anomalyco/opencode/commit/56714327f4c99de11fdc14348f6bdd18314e6ccf) sync
- 2026-06-22 [a0a5003](https://github.com/anomalyco/opencode/commit/a0a500316ee009b5f84f593ec6b304e737224ad7) zen: new inference
- 2026-06-26 [7a17925](https://github.com/anomalyco/opencode/commit/7a17925495ca61b76169737b226b834b61942586) zen: new inference
- 2026-07-16 [5121352](https://github.com/anomalyco/opencode/commit/51213520f5f69ce2c6c741adcb2785e017488ade) go: grok 4.5 and kimi k3
- 2026-07-30 [f720490](https://github.com/anomalyco/opencode/commit/f7204902192624481e526b1852cdedaadb914e24) fix(console): add public fetch compatibility flag
- 2026-08-07 [3255297](https://github.com/anomalyco/opencode/commit/325529761beb79a004de6d86e48b8db69cf4eba3) sync
- 2026-08-19 [3477d28](https://github.com/anomalyco/opencode/commit/3477d28a87d1df1a0c07399d10b1b4970daa4d09) Merge branch 'muse-spark' of github.com:anomalyco/opencode into muse-spark
- 2026-08-19 [0a63972](https://github.com/anomalyco/opencode/commit/0a6397272c67b1d47ceb072616e55408b7680882) Merge branch 'muse-spark' into dev
- 2026-08-19 [18fc3ee](https://github.com/anomalyco/opencode/commit/18fc3eea7cb18bcc99867ddfe8090d6a3f432823) update zen model name

