import { Cause, Effect } from "effect"
import type { Event } from "@opencode/schema/event"

export type Subscriber<D extends Event.Definition = Event.Definition> = (event: Event.Payload<D>) => Effect.Effect<void>

export const observe = (event: Event.Payload, observer: (event: Event.Payload) => Effect.Effect<void>) =>
  Effect.suspend(() => observer(event)).pipe(
    Effect.catchCauseIf(
      (cause) => !Cause.hasInterrupts(cause),
      (cause) => Effect.logError("Event listener failed", { eventID: event.id, eventType: event.type, cause }),
    ),
  )
