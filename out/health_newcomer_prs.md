# Stratified sample of newcomer PRs (calendar quarter x team, 6 per stratum, seed 42)
Frame: 16824 PRs by newcomers (4570 by team members). Sample: 54.

| group | PRs | merged | auto-closed | closed (no bot note) | open | first reply: team | other human | bots only | nobody |
|---|---|---|---|---|---|---|---|---|---|
| outside newcomer | 30 | 1 | 16 | 11 | 2 | 2 | 6 | 16 | 6 |
| team newcomer | 24 | 18 | 0 | 6 | 0 | 1 | 0 | 3 | 20 |

## #2824 2025Q3 non-team mlshv (occasional) -> closed; first reply: other human; first human response 0.6 h
fix: add "src" to "files" in SDK package.json | https://github.com/anomalyco/opencode/pull/2824
- 2025-09-27 12:27 remorses: There is already https://github.com/sst/opencode/pull/2567

## #2832 2025Q3 non-team SimplyYourAverageDev (occasional) -> closed; first reply: other human; first human response 13.0 h
Adding smart edit | https://github.com/anomalyco/opencode/pull/2832
- 2025-09-28 10:44 jenslys: Does this supersede https://github.com/sst/opencode/pull/1916 ?
- 2025-10-07 17:23 zenyr: Thanks for implementing the smart_edit tool—it seems great for handling complex code edits! Just a concern: since it uses OpenRouter's morph/morph-v3-large mode...
- 2025-10-11 11:19 bhaktatejas922: https://github.com/sst/opencode/pull/1916
- 2025-10-14 22:25 bhaktatejas922: @thdxr ?

## #2835 2025Q3 non-team paulbettner (regular) -> closed; first reply: nobody; first human response none
fix(smartypants): sanitize history to avoid provider errors on reasoning and empty content | https://github.com/anomalyco/opencode/pull/2835

## #2850 2025Q3 non-team paulbettner (regular) -> closed; first reply: nobody; first human response none
test: normalize macOS /private tmp path in snapshot tests | https://github.com/anomalyco/opencode/pull/2850

## #2861 2025Q3 non-team CarlosGtrz (regular) -> pr-stale; first reply: other human; first human response 558.9 h
fix: validate DIBv5 format before fallback to DIB in Windows clipboard | https://github.com/anomalyco/opencode/pull/2861
- 2025-10-23 03:37 itsrainingmani: I'm testing this out on a few different windows based terminals. Can I ask where specifically you find improvements in providing the DIB fallback?
- 2026-02-02 16:37 [bot] github-actions[bot]: Closing this pull request because it has had no updates for more than 60 days. If you plan to continue working on it, feel free to reopen or open a new PR.

## #2869 2025Q3 non-team paulbettner (regular) -> closed; first reply: nobody; first human response none
fix: sanitize message history to prevent API errors | https://github.com/anomalyco/opencode/pull/2869

## #3430 2025Q4 non-team JosXa (regular) -> merged; first reply: team member; first human response 31.5 h
fix: make build script work cross-platform | https://github.com/anomalyco/opencode/pull/3430
- 2025-10-26 06:40 rekram1-node: thanks!

## #3880 2025Q4 non-team PranshuRaj1 (occasional) -> closed; first reply: nobody; first human response none
[FIX]: Refocus prompt after session delete | https://github.com/anomalyco/opencode/pull/3880

## #3965 2025Q4 non-team riatzukiza (regular) -> pr-stale; first reply: bots only; first human response none
Differentiate Shift+Enter from Enter for prompt submission | https://github.com/anomalyco/opencode/pull/3965
- 2026-02-02 16:40 [bot] github-actions[bot]: Closing this pull request because it has had no updates for more than 60 days. If you plan to continue working on it, feel free to reopen or open a new PR.

## #4275 2025Q4 team Hona (key) -> merged; first reply: bots only; first human response none
fix: resolve bun/pnpm global install failures on Windows | https://github.com/anomalyco/opencode/pull/4275
- 2025-11-13 06:08 [bot] Copilot: The package name uses "windows" but the build now creates packages with "win32" in the name. This line should use `win32` instead of `windows` to match the pack...
- 2025-11-13 06:08 [bot] Copilot: The Windows .exe copying and bin field configuration are conditional on `process.platform === "win32"`, meaning they only execute when the publish script runs o...
- 2025-11-13 06:08 [bot] Copilot: Multiple `process.chdir()` calls without proper error handling could leave the process in an unexpected directory if an error occurs between changes. Consider w...
- 2025-11-13 06:08 [bot] Copilot: Using `chmod 777` grants read, write, and execute permissions to all users, which is overly permissive and a security concern. Consider using more restrictive p...

## #5486 2025Q4 team R44VC0RP (key) -> closed; first reply: nobody; first human response none
fix(tui): normalize animated terminal output in bash tool | https://github.com/anomalyco/opencode/pull/5486

## #5536 2025Q4 non-team shamil2 (occasional) -> pr-stale; first reply: bots only; first human response none
Fix race condition in concurrent session deletion | https://github.com/anomalyco/opencode/pull/5536
- 2026-02-13 06:18 [bot] github-actions[bot]: Closing this pull request because it has had no updates for more than 60 days. If you plan to continue working on it, feel free to reopen or open a new PR.

## #5590 2025Q4 team Hona (key) -> merged; first reply: bots only; first human response none
fix(win32): correct ElixirLS extension typo | https://github.com/anomalyco/opencode/pull/5590

## #5684 2025Q4 non-team tedin7 (occasional) -> pr-stale; first reply: team member; first human response 4.7 h
fix(desktop): detect system dark mode on Linux | https://github.com/anomalyco/opencode/pull/5684
- 2025-12-17 16:33 rekram1-node: A few things in flux atm, desktop guys requested I didn't merge PRs quite yet, they should be ready for contributions soon tho
- 2025-12-18 16:37 rekram1-node: Ill have them review
- 2026-02-17 06:17 [bot] github-actions[bot]: Closing this pull request because it has had no updates for more than 60 days. If you plan to continue working on it, feel free to reopen or open a new PR.

## #5823 2025Q4 team R44VC0RP (key) -> merged; first reply: team member; first human response 18.1 h
feat(docs): adding .md to docs pages shows raw markdown | https://github.com/anomalyco/opencode/pull/5823
- 2025-12-20 18:01 rekram1-node: /review
- 2025-12-20 18:02 [bot] github-actions[bot]: lgtm

## #5854 2025Q4 team R44VC0RP (key) -> merged; first reply: nobody; first human response none
docs: clarify model ID format for OpenCode provider | https://github.com/anomalyco/opencode/pull/5854

## #6353 2025Q4 non-team brettheap (occasional) -> closed; first reply: nobody; first human response none
fix(tui): improve model selection dialog | https://github.com/anomalyco/opencode/pull/6353

## #6399 2025Q4 team nexxeln (key) -> merged; first reply: nobody; first human response none
add supermemory plugin to ecosystem | https://github.com/anomalyco/opencode/pull/6399

## #8279 2026Q1 non-team lqmanh (occasional) -> closed; first reply: other human; first human response 1011.9 h
feat(plugin): show both configured & installed version of plugins in `DialogStatus` | https://github.com/anomalyco/opencode/pull/8279
- 2026-01-13 20:38 [bot] github-actions[bot]: <!-- pr-standards:issue -->
- 2026-01-13 20:39 [bot] github-actions[bot]: _The following comment was made by an LLM, it may be inaccurate:_
- 2026-02-25 00:35 LukeCarrier: I just duped this in #14975, but for both the TUI and desktop app. Sorry, GitHub search is rough with this many open issues and PRs 🙃

## #8627 2026Q1 non-team ryanwyler (regular) -> closed; first reply: other human; first human response 0.7 h
feat(tui): load conversation and session history | https://github.com/anomalyco/opencode/pull/8627
- 2026-01-15 08:16 [bot] github-actions[bot]: _The following comment was made by an LLM, it may be inaccurate:_
- 2026-01-15 08:58 ariane-emory: Have not yet tested, but I like the idea, if it works I would totally be in favour.
- 2026-01-15 09:18 ariane-emory: I haven't looked at the failures too deeply yet. If it's just the CI system spazzing out and throwing a fit - which does occur sometimes - then it could be safe...

## #11206 2026Q1 non-team deiviuds (casual) -> pr-stale; first reply: bots only; first human response none
docs: add opencode-brain plugin to ecosystem | https://github.com/anomalyco/opencode/pull/11206
- 2026-01-29 21:49 [bot] github-actions[bot]: _The following comment was made by an LLM, it may be inaccurate:_
- 2026-04-01 06:30 [bot] github-actions[bot]: Closing this pull request because it has had no updates for more than 60 days. If you plan to continue working on it, feel free to reopen or open a new PR.

## #12587 2026Q1 non-team faywong (occasional) -> pr-stale; first reply: bots only; first human response none
feat: add opencode-dingtalk-notify plugin to web ecosystem | https://github.com/anomalyco/opencode/pull/12587
- 2026-02-07 07:36 [bot] github-actions[bot]: <!-- pr-standards:title -->
- 2026-02-07 07:36 [bot] github-actions[bot]: _The following comment was made by an LLM, it may be inaccurate:_
- 2026-02-07 22:21 [bot] github-actions[bot]: <!-- pr-standards:issue -->
- 2026-04-10 06:32 [bot] github-actions[bot]: Closing this pull request because it has had no updates for more than 60 days. If you plan to continue working on it, feel free to reopen or open a new PR.

## #13019 2026Q1 non-team themavik (occasional) -> closed; first reply: bots only; first human response none
fix: prevent rate limit errors from being misclassified as context overflow | https://github.com/anomalyco/opencode/pull/13019
- 2026-02-10 20:15 [bot] github-actions[bot]: _The following comment was made by an LLM, it may be inaccurate:_

## #16682 2026Q1 non-team paarthd00 (occasional) -> pr-stale; first reply: bots only; first human response none
fix(app): save provider list in store and prevent multiple provider calls | https://github.com/anomalyco/opencode/pull/16682
- 2026-03-09 03:24 [bot] github-actions[bot]: <!-- pr-standards:title -->
- 2026-05-12 06:52 [bot] github-actions[bot]: Closing this pull request because it has had no updates for more than 60 days. If you plan to continue working on it, feel free to reopen or open a new PR.

## #17857 2026Q1 team Hona (key) -> merged; first reply: bots only; first human response none
fix(app): avoid prompt tooltip Switch on startup | https://github.com/anomalyco/opencode/pull/17857

## #17952 2026Q1 team nexxeln (key) -> closed; first reply: nobody; first human response none
docs: add GitHub releases link to README | https://github.com/anomalyco/opencode/pull/17952

## #18127 2026Q1 team kitlangton (key) -> closed; first reply: nobody; first human response none
fix(zen): read nested usage in non-stream responses | https://github.com/anomalyco/opencode/pull/18127

## #18448 2026Q1 team kitlangton (key) -> closed; first reply: nobody; first human response none
fix: lazy runtime imports in facades to break bundle cycles | https://github.com/anomalyco/opencode/pull/18448

## #18731 2026Q1 team nexxeln (key) -> merged; first reply: nobody; first human response 0.0 h
fix(app): prefer cmd+k for command palette | https://github.com/anomalyco/opencode/pull/18731

## #19431 2026Q1 team jlongster (key) -> closed; first reply: nobody; first human response none
fix(tui): fix flaky plugin test | https://github.com/anomalyco/opencode/pull/19431

## #23283 2026Q2 non-team flexdinesh (regular) -> pr-cleanup; first reply: bots only; first human response none
feat(opencode): use regex to whitelist/blacklist provider models | https://github.com/anomalyco/opencode/pull/23283
- 2026-04-18 11:30 [bot] github-actions[bot]: <!-- pr-standards:title -->
- 2026-04-18 11:31 [bot] github-actions[bot]: _The following comment was made by an LLM, it may be inaccurate:_
- 2026-04-18 11:41 [bot] github-actions[bot]: Thanks for updating your PR! It now meets our contributing guidelines. :+1:
- 2026-05-18 22:23 [bot] github-actions[bot]: Automated PR Cleanup

## #25589 2026Q2 non-team dk363 (casual) -> pr-cleanup; first reply: bots only; first human response none
docs: fix markdown table rendering in experimental features. (zh-CN)  | https://github.com/anomalyco/opencode/pull/25589
- 2026-05-03 12:54 [bot] github-actions[bot]: _The following comment was made by an LLM, it may be inaccurate:_
- 2026-06-03 22:47 [bot] github-actions[bot]: Automated PR Cleanup

## #25911 2026Q2 team jlongster (key) -> merged; first reply: nobody; first human response none
fix(core): better state handling of editor context | https://github.com/anomalyco/opencode/pull/25911

## #27169 2026Q2 team kitlangton (key) -> closed; first reply: nobody; first human response none
effect: introduce defineService and drop per-call Effect.provide | https://github.com/anomalyco/opencode/pull/27169

## #27612 2026Q2 team nexxeln (key) -> merged; first reply: nobody; first human response none
test(workspace): use runtime flags in workspace tests | https://github.com/anomalyco/opencode/pull/27612

## #28148 2026Q2 team nexxeln (key) -> merged; first reply: nobody; first human response none
fix(tui): collapse long tool output lines | https://github.com/anomalyco/opencode/pull/28148

## #28405 2026Q2 team kitlangton (key) -> merged; first reply: nobody; first human response none
Skip snapshot seed file commits | https://github.com/anomalyco/opencode/pull/28405

## #30441 2026Q2 non-team stanlymt (occasional) -> pr-cleanup; first reply: other human; first human response 303.6 h
fix(app): avoid O(n^2) dedup hang on large diff summaries | https://github.com/anomalyco/opencode/pull/30441
- 2026-06-02 18:12 [bot] github-actions[bot]: Thanks for updating your PR! It now meets our contributing guidelines. :+1:
- 2026-06-15 08:57 jerrydong1988: Hi team! 👋
- 2026-06-29 10:21 jerrydong1988: 👋 Bumping this PR — combined with #31309, these two fixes completely resolve the desktop renderer freeze that many Windows users experience with large sessions...
- 2026-07-02 22:27 [bot] github-actions[bot]: Automated PR Cleanup

## #30636 2026Q2 non-team literally-dan (occasional) -> pr-cleanup; first reply: bots only; first human response none
fix(storage): add session(time_updated) index for the global session list | https://github.com/anomalyco/opencode/pull/30636
- 2026-06-03 23:15 [bot] github-actions[bot]: <!-- pr-standards:title -->
- 2026-07-04 22:18 [bot] github-actions[bot]: Automated PR Cleanup

## #32064 2026Q2 non-team quadcpu (occasional) -> pr-cleanup; first reply: bots only; first human response none
fix(tui): fix right-click paste on windows | https://github.com/anomalyco/opencode/pull/32064
- 2026-06-12 13:52 [bot] github-actions[bot]: Thanks for updating your PR! It now meets our contributing guidelines. :+1:
- 2026-07-12 22:20 [bot] github-actions[bot]: Automated PR Cleanup

## #33796 2026Q2 non-team niveditabuilds (casual) -> closed; first reply: bots only; first human response none
Opencode voice mode | https://github.com/anomalyco/opencode/pull/33796
- 2026-06-25 05:39 [bot] github-actions[bot]: <!-- issue-compliance -->
- 2026-06-25 05:40 [bot] github-actions[bot]: <!-- pr-standards:title -->
- 2026-06-25 05:40 [bot] github-actions[bot]: _The following comment was made by an LLM, it may be inaccurate:_

## #34454 2026Q2 team nexxeln (key) -> merged; first reply: nobody; first human response none
feat(llm): add tool schema projections | https://github.com/anomalyco/opencode/pull/34454

## #36149 2026Q3 team kitlangton (key) -> merged; first reply: nobody; first human response none
fix(tui): handle session search fetch failures without crashing | https://github.com/anomalyco/opencode/pull/36149

## #38192 2026Q3 team simonklee (key) -> merged; first reply: nobody; first human response none
mini: add hideable footer details | https://github.com/anomalyco/opencode/pull/38192

## #38383 2026Q3 team jlongster (key) -> merged; first reply: nobody; first human response none
refactor(tui): finish V2 theme migration | https://github.com/anomalyco/opencode/pull/38383

## #38684 2026Q3 non-team literally-dan (occasional) -> pr-cleanup; first reply: bots only; first human response none
fix(storage): index session list queries | https://github.com/anomalyco/opencode/pull/38684
- 2026-08-24 22:10 [bot] github-actions[bot]: Automated PR Cleanup

## #40735 2026Q3 team kitlangton (key) -> merged; first reply: nobody; first human response none
refactor(core): remove unused copy strategy registry | https://github.com/anomalyco/opencode/pull/40735

## #41429 2026Q3 non-team leepokai (occasional) -> pr-cleanup; first reply: bots only; first human response none
fix(app): resolve message identity by id when creation time shifts | https://github.com/anomalyco/opencode/pull/41429
- 2026-09-09 22:08 [bot] github-actions[bot]: Automated PR Cleanup

## #45008 2026Q3 non-team scw1109 (occasional) -> pr-cleanup; first reply: bots only; first human response none
feat(opencode): track response model ID for dynamic routing providers | https://github.com/anomalyco/opencode/pull/45008
- 2026-09-25 22:16 [bot] github-actions[bot]: Automated PR Cleanup

## #45778 2026Q3 team kitlangton (key) -> merged; first reply: nobody; first human response none
fix(core): preserve typed config test-layer errors | https://github.com/anomalyco/opencode/pull/45778

## #46431 2026Q3 team nexxeln (key) -> merged; first reply: nobody; first human response none
feat(ai): support typed provider-side compaction | https://github.com/anomalyco/opencode/pull/46431

## #47242 2026Q3 non-team AhsanIqbal-web (occasional) -> open; first reply: bots only; first human response none
feat(serve): --qr flag for Antigravity-style remote pairing (issue #47241) | https://github.com/anomalyco/opencode/pull/47242
- 2026-09-04 09:41 [bot] github-actions[bot]: Thanks for updating your PR! It now meets our contributing guidelines. :+1:

## #48487 2026Q3 non-team artempavlov (casual) -> open; first reply: nobody; first human response none
docs(ecosystem): add opencode-cd plugin | https://github.com/anomalyco/opencode/pull/48487

## #49119 2026Q3 non-team whatnick (occasional) -> pr-compliance; first reply: bots only; first human response none
fix(provider): apply Bedrock au. prefix to full Claude family in AU regions | https://github.com/anomalyco/opencode/pull/49119
- 2026-09-15 05:07 [bot] github-actions[bot]: <!-- issue-compliance -->
- 2026-09-15 07:11 [bot] github-actions[bot]: This pull request has been automatically closed because it was not updated to meet our [contributing guidelines](../blob/dev/CONTRIBUTING.md) within the 2-hour ...

