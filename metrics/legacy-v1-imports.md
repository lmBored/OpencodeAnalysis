# Legacy v1 Imports and References in Opencode

This document lists all instances of modules importing, requiring, or re-exporting legacy v1 versions of code across the `opencode` repository packages.

## Summary of Imports/References by Package and File

### 1. `packages/theme`
* **`packages/theme/test/theme.test.ts`**
  * Line 3: `import type { ThemeV1Json } from "../src/tui/v1.js"`
* **`packages/theme/src/tui/v1-migrate.ts`**
  * Line 5: `import type { Theme, ThemeV1Json } from "./v1.js"`
* **`packages/theme/src/tui/index.ts`**
  * Line 46: `export { migrateV1 } from "./v1-migrate.js"`

### 2. `packages/schema`
* **`packages/schema/test/v1-isolation.test.ts`**
  * Line 4: `import { PermissionV1 } from "../src/permission-v1.js"`
  * Line 5: `import { SessionV1 } from "../src/session-v1.js"`
  * Line 6: `const isolatedLegacyEvent = await import("../src/v1/legacy-event.js")`
  * Line 7: `const isolatedPermission = await import("../src/v1/permission.js")`
  * Line 8: `const isolatedSession = await import("../src/v1/session.js")`
* **`packages/schema/test/legacy-event.test.ts`**
  * Line 3: `import { PermissionV1 } from "../src/permission-v1.js"`
  * Line 5: `import { SessionV1 } from "../src/session-v1.js"`
* **`packages/schema/test/event-manifest.test.ts`**
  * Line 17: `import { FileSystemV1 } from "../src/filesystem-v1.js"`
* **`packages/schema/src/filesystem-v1.ts`**
  * Line 1: `export * from "./v1/filesystem.js"`
* **`packages/schema/src/event-manifest.ts`**
  * Line 11: `import { FileSystemV1 } from "./filesystem-v1.js"`
* **`packages/schema/src/session-v1.ts`**
  * Line 1: `export * from "./v1/session.js"`
* **`packages/schema/src/permission-v1.ts`**
  * Line 1: `export * from "./v1/permission.js"`
* **`packages/schema/src/legacy-event.ts`**
  * Line 1: `export * from "./v1/legacy-event.js"`

### 3. `packages/server`
* **`packages/server/src/routes.ts`**
  * Line 2: `import { V1Migration } from "@opencode/core/database/v1-migration"`
* **`packages/server/src/handlers/migration.ts`**
  * Line 1: `import { V1Migration } from "@opencode/core/database/v1-migration"`

### 4. `packages/core`
* **`packages/core/src/database/v1-migration.ts`**
  * Line 1: `export * as V1Migration from "#v1-migration"`
  * Line 2: `export * from "#v1-migration"`
* **`packages/core/src/database/v1-migration.bun.ts`**
  * Line 1: `export * as V1Migration from "./v1-migration.js"`
  * Line 6: `import { SessionV1 } from "@opencode/schema/session-v1"`
* **`packages/core/src/database/migration.gen.ts`**
  * Line 48: `import m46 from "./migration/20260910120000_clear_v1_session_permission.js"`
* **`packages/core/test/v1-migration.test.ts`**
  * Line 6: `import { V1Migration } from "@opencode/core/database/v1-migration"`
* **`packages/core/src/config/normalize.ts`**
  * Line 21: `import { ConfigAgentV1 } from "../v1/config/agent.js"`
  * Line 22: `import { ConfigCommandV1 } from "../v1/config/command.js"`
  * Line 23: `import { ConfigMCPV1 } from "../v1/config/mcp.js"`
  * Line 24: `import { ConfigPermissionV1 } from "../v1/config/permission.js"`
  * Line 25: `import { ConfigPluginV1 } from "../v1/config/plugin.js"`
  * Line 26: `import { ConfigProviderV1 } from "../v1/config/provider.js"`
  * Line 27: `import { ConfigMigrateV1 } from "../v1/config/migrate.js"`
* **`packages/core/src/config/plugin/agent.ts`**
  * Line 12: `import { ConfigAgentV1 } from "../../v1/config/agent.js"`
  * Line 13: `import { ConfigMigrateV1 } from "../../v1/config/migrate.js"`
* **`packages/core/test/fixture/v1-config/config.ts`**
  * Line 6: `import { ConfigAgentV1 } from "@opencode/core/v1/config/agent"`
  * Line 8: `import { ConfigCommandV1 } from "@opencode/core/v1/config/command"`
  * Line 12: `import { ConfigMCPV1 } from "@opencode/core/v1/config/mcp"`
  * Line 13: `import { ConfigPermissionV1 } from "@opencode/core/v1/config/permission"`
  * Line 14: `import { ConfigPluginV1 } from "@opencode/core/v1/config/plugin"`
  * Line 15: `import { ConfigProviderV1 } from "@opencode/core/v1/config/provider"`
* **`packages/core/test/config/provider-options.test.ts`**
  * Line 2: `import { ConfigProviderOptionsV1 } from "@opencode/core/v1/config/provider-options"`
* **`packages/core/test/config/agent.test.ts`**
  * Line 19: `import { ConfigAgentV1 } from "@opencode/core/v1/config/agent"`
* **`packages/core/test/config/config.test.ts`**
  * Line 14: `import { ConfigV1 } from "../fixture/v1-config/config"`
  * Line 15: `import { ConfigPermissionV1 } from "@opencode/core/v1/config/permission"`

### 5. `packages/tui`
* **`packages/tui/test/theme.test.ts`**
  * Line 10: `import type { ThemeV1Json } from "@opencode/theme/tui/v1"`
* **`packages/tui/test/theme/v2/v1-migrate.test.ts`**
  * Line 12: `import type { ThemeV1Json } from "@opencode/theme/tui/v1"`
* **`packages/tui/test/config.test.tsx`**
  * Line 13: `} from "../src/config/v1"`
* **`packages/tui/test/config-v2.test.tsx`**
  * Line 8: `import { CommandMap, Definitions } from "../src/config/v1/keybind"`
* **`packages/tui/test/cli/tui/theme-mode.test.tsx`**
  * Line 8: `import type { ThemeV1Json } from "@opencode/theme/tui/v1"`
* **`packages/tui/src/theme/v1.ts`**
  * Line 1: `import type { ThemeV1Json } from "@opencode/theme/tui/v1"`
  * Line 35: `export type { ColorValue, Theme, ThemeColor, ThemeV1Json } from "@opencode/theme/tui/v1"`
* **`packages/tui/src/theme/resolve.ts`**
  * Line 3: `import type { ColorValue, Theme, ThemeColor, ThemeV1Json } from "./v1"`
* **`packages/tui/src/theme/index.ts`**
  * Line 3: `import { DEFAULT_THEMES, type Theme, type ThemeV1Json } from "./v1"`
  * Line 7: `export { DEFAULT_THEMES, type Theme, type ThemeV1Json } from "./v1"`

### 6. `packages/cli`
* **`packages/cli/src/run/index.ts`**
  * Line 2: `export { runV1Bridge, type V1RunCommandInput } from "./v1"`
* **`packages/cli/src/config/migrate.ts`**
  * Line 3: `import { TuiConfigV1 } from "@opencode/tui/config/v1"`
  * Line 4: `import { TuiKeybind } from "@opencode/tui/config/v1/keybind"`
