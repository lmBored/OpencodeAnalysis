# Candidate threads (targeted sample; window 2025-09-27..2026-09-27)
Frame: PRs created in the window by either member; filter: >=3 comments by the two members together and >=1 by the non-author.

## kitlangton <-> rekram1-node (weaker direction 8, total 32; 3 threads)
### #18433 by rekram1-node (2026-03-20) feat: AI SDK v6 support
https://github.com/anomalyco/opencode/pull/18433 | comments kitlangton=14, rekram1-node=8, formal review between them: yes
- 2026-03-24 22:40 **rekram1-node** (issue): Marking as non draft so that it can be included in beta
- 2026-03-24 22:46 **rekram1-node** (issue): Test failure is due to changes in ai sdk that prevent u from setting both temp and topp when using anthropic sdk, this makes sense for anthropic but may break things for other models like minimax.
- 2026-03-25 22:37 **rekram1-node** (issue): @avarayr thanks for testing the branch! There are a few minor things that are incompat (for example u see the failing unit tests, these failures make sense when using anthropic but other providers also use that same sdk ...
- 2026-03-26 00:57 **rekram1-node** (issue): @avarayr what providers will error from that?
- 2026-03-26 05:07 **rekram1-node** (issue): Thats extra annoying cause the header isnt even needed anymore hmm
- 2026-03-26 05:07 **rekram1-node** (issue): This is technically easy to solve, we just need to override the header for provider...
- 2026-03-27 13:46 **kitlangton** (review packages/opencode/src/provider/transform.ts): Do you need to push the message still?
- 2026-03-27 13:46 **kitlangton** (review packages/opencode/src/provider/transform.ts): Looks like this may have been accidentally deleted? Unless I'm missing something :)

### #25034 by kitlangton (2026-04-30) feat: default HTTP API backend to on for dev/beta channels
https://github.com/anomalyco/opencode/pull/25034 | comments kitlangton=3, rekram1-node=1, formal review between them: yes
- 2026-05-01 03:05 **kitlangton** (review packages/core/src/flag/flag.ts): helo
- 2026-05-01 11:14 **kitlangton** (review packages/core/src/flag/flag.ts): test comment.
- 2026-05-01 19:51 **rekram1-node** (review packages/core/src/flag/flag.ts): helo oheu
- 2026-05-02 23:34 **kitlangton** (review packages/core/src/flag/flag.ts): OH HELLO OH OH OH

### #18971 by rekram1-node (2026-03-24) chore: effectify agent.ts
https://github.com/anomalyco/opencode/pull/18971 | comments kitlangton=3, rekram1-node=0, formal review between them: yes
- 2026-03-24 18:02 **kitlangton** (review packages/opencode/src/agent/agent.ts): ```suggestion
- 2026-03-24 18:03 **kitlangton** (review packages/opencode/src/agent/agent.ts): Should use the `config` helper above?
- 2026-03-24 18:03 **kitlangton** (review packages/opencode/src/agent/agent.ts): I think Auth has been Effectified so we can depend on and use that service.

## Hona <-> Brendonovich (weaker direction 7, total 20; 1 threads)
### #25962 by Brendonovich (2026-05-06) feat(desktop): move server to utilityProcess
https://github.com/anomalyco/opencode/pull/25962 | comments Hona=5, Brendonovich=0, formal review between them: yes
- 2026-05-06 03:51 **Hona** (review packages/desktop/src/main/server.ts): This startup wait needs a timeout. As written, the desktop can hang forever if the sidecar stalls before posting `ready`/`error` (module import, sqlite migration, native binding load, `Server.listen`). The 30s health tim...
- 2026-05-06 03:51 **Hona** (review packages/desktop/src/main/sidecar.ts): This startup error path posts the error but leaves the utility process alive. The parent rejects, but nothing guarantees this sidecar exits or gets killed. VS Code treats utility-process exit/crash as lifecycle state and...
- 2026-05-06 03:51 **Hona** (review packages/desktop/src/main/server.ts): Shutdown is fire-and-forget here. Callers like relaunch/update/quit immediately continue, so this 2s force-kill timer may never fire if Electron main exits first. VS Code has `waitForExit(maxWaitTimeMs)` that waits for e...
- 2026-05-06 03:51 **Hona** (review packages/desktop/src/main/server.ts): This passes the live mutable `process.env` object directly. VS Code's utility-process wrapper clones env, applies explicit process metadata, removes dangerous env vars, handles platform-specific env adjustments, and stri...
- 2026-05-06 03:51 **Hona** (review packages/desktop/src/main/server.ts): We should also wire the utility-process crash/error path, not just a plain `exit` callback. VS Code registers stdout, stderr, message, spawn, exit, and `app.child-process-gone` filtered by service name so it can distingu...

## rekram1-node <-> Hona (weaker direction 5, total 35; 7 threads)
### #16069 by Hona (2026-03-05) feat(windows): add first-class pwsh/powershell support
https://github.com/anomalyco/opencode/pull/16069 | comments rekram1-node=3, Hona=5, formal review between them: yes
- 2026-03-09 16:45 **rekram1-node** (review packages/app/e2e/terminal/terminal-tabs.spec.ts): flaky!
- 2026-03-29 23:21 **Hona** (issue): /review
- 2026-03-30 02:51 **rekram1-node** (review packages/opencode/src/pty/index.ts): is this change necessary?
- 2026-03-30 02:52 **rekram1-node** (review packages/opencode/src/pty/index.ts): ig i see what it does just wondering why the behavior change?
- 2026-03-30 02:52 **Hona** (review packages/opencode/src/pty/index.ts): yeah - breaks now with 'pwsh' before 'ends with sh' just happened to work.
- 2026-03-30 07:03 **Hona** (issue): @caozhiyuan what automation? I would love to hear more about the cross team use case?
- 2026-03-30 07:04 **Hona** (issue): but yeah set an env var if you want git bash again, e.g.
- 2026-03-31 05:19 **Hona** (issue): yup i'm further refactoring pwsh/powershell into separate tools

### #5600 by Hona (2025-12-16) fix: debounce LSP diagnostics to get complete results
https://github.com/anomalyco/opencode/pull/5600 | comments rekram1-node=3, Hona=2, formal review between them: no
- 2025-12-16 00:13 **Hona** (issue): /review
- 2025-12-16 00:14 **Hona** (issue): he doesn't listen to me </3
- 2025-12-16 01:40 **rekram1-node** (issue): /review
- 2025-12-16 01:40 **rekram1-node** (issue): only i can do it hehe
- 2025-12-16 01:41 **rekram1-node** (issue): prollyy should create const DIAGNOSTICS_DEBOUNCE_MS near top of file

### #9305 by Hona (2026-01-18) feat(tui): use mouse for permission buttons
https://github.com/anomalyco/opencode/pull/9305 | comments rekram1-node=2, Hona=2, formal review between them: yes
- 2026-01-18 23:24 **rekram1-node** (review packages/opencode/src/cli/cmd/tui/routes/session/permission.tsx): why?
- 2026-01-18 23:34 **Hona** (review packages/opencode/src/cli/cmd/tui/routes/session/permission.tsx): it would fall thru and send allow once as well.
- 2026-01-18 23:39 **rekram1-node** (review packages/opencode/src/cli/cmd/tui/routes/session/permission.tsx): ahh
- 2026-01-18 23:42 **Hona** (review packages/opencode/src/cli/cmd/tui/routes/session/permission.tsx): Yeah all good.

### #4234 by Hona (2025-11-12) fix: Tool calling on windows
https://github.com/anomalyco/opencode/pull/4234 | comments rekram1-node=1, Hona=13, formal review between them: no
- 2025-11-12 22:18 **Hona** (issue): Weird - it fixed it for me. I'll clean rebuild and try your prompt
- 2025-11-12 22:23 **Hona** (issue): <img width="1116" height="631" alt="image" src="https://github.com/user-attachments/assets/bed9f3e6-20f2-4075-8000-c25c8e43d0b8" />
- 2025-11-12 22:35 **Hona** (issue): Thanks for that extra info @Sewer56
- 2025-11-12 22:42 **Hona** (issue): Yeah in theory that is why I made the changes to try and call fsync and so forth for the race conditions. I'll dig into it further
- 2025-11-12 22:45 **Hona** (issue): Yup I am trying similar, tmp -> rename. I'll also see if I can revert to bun apis for the file write. Thanks for that
- 2025-11-12 22:58 **Hona** (issue): Weirdly online threads seem to think that `fs.rename` does use `MoveFileEx("temp.txt", "original.txt", MOVEFILE_REPLACE_EXISTING);` under the hood on windows. But we see errors + the 0 byte issues, so I guess not - maybe...
- 2025-11-12 23:08 **Hona** (issue): I'm not a JS guy, but it seems that
- 2025-11-12 23:12 **Hona** (issue): Your codes nice too - I'll remove all my extra error handling etc and just keep with bun APIs + that proper await. I'll see how small I can make it

### #5768 by Hona (2025-12-19) tweak: better release notes (grouped changelog)
https://github.com/anomalyco/opencode/pull/5768 | comments rekram1-node=6, Hona=1, formal review between them: yes
- 2025-12-19 00:20 **rekram1-node** (review script/publish.ts): these 3 should be exlcuded
- 2025-12-19 00:20 **rekram1-node** (review script/publish.ts): these should be excluded
- 2025-12-19 00:20 **rekram1-node** (review script/publish.ts): can we keep the commits they made? Or why remove it?
- 2025-12-19 00:21 **rekram1-node** (review script/publish.ts): nix, infra, script
- 2025-12-19 00:21 **rekram1-node** (review script/publish.ts): /zed
- 2025-12-19 00:23 **Hona** (review script/publish.ts): <img width="455" height="763" alt="image" src="https://github.com/user-attachments/assets/cf40ef71-e90d-48c8-8771-55651032cb0a" />
- 2025-12-19 00:31 **rekram1-node** (review script/publish.ts): Yeah that's fine then gotcha

### #6629 by Hona (2026-01-02) feat(telemetry): add OpenTelemetry instrumentation with Aspire Dashboard support
https://github.com/anomalyco/opencode/pull/6629 | comments rekram1-node=1, Hona=3, formal review between them: no
- 2026-01-02 07:29 **Hona** (issue): /review locally
- 2026-01-04 22:44 **Hona** (review packages/opencode/src/telemetry/index.ts): agreed on the running application chooses the exporter.
- 2026-01-06 03:07 **Hona** (review packages/opencode/src/telemetry/index.ts): I've got some more work to make it perfect - but agree on your points.
- 2026-05-15 03:10 **rekram1-node** (issue): Automated PR Cleanup

### #4273 by Hona (2025-11-13) fix: Enable Windows builds and fix bun+pnpm install on Windows
https://github.com/anomalyco/opencode/pull/4273 | comments rekram1-node=2, Hona=1, formal review between them: yes
- 2025-11-13 03:37 **rekram1-node** (review packages/opencode/script/postinstall.mjs): Is this return necessary? It does change the pre-existing behavior, like what would happen if someone tried yarn?
- 2025-11-13 03:54 **Hona** (review packages/opencode/script/postinstall.mjs): Yeah return here is a no op, I'll remove it.
- 2025-11-13 03:56 **rekram1-node** (review packages/opencode/script/postinstall.mjs): oh nvm i think this isn't an issue actually

## adamdotdevin <-> Hona (weaker direction 4, total 10; 0 threads)
## rekram1-node <-> adamdotdevin (weaker direction 3, total 7; 0 threads)
