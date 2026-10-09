# DEC-2026-HOST-001 — Android session owner (2026-10-09)

Status: accepted_for_design; implementation candidate, device verification pending.

Android owns the durable session and accepts typed evidence from native adapters.
Web Maps remains a presentation adapter. A map selection is not GPS evidence;
returning from the existing AR diagnostic activity is not task completion.
One pending AR request carries sessionId, requestId, missionId and taskId.
Only a correlated result, validated by the task adapter, can award configured points.
Pause preserves progress. Process recovery pauses the session and invalidates pending
AR requests; the player explicitly resumes and retries the unfinished task.

Session schema: 1. New state is separate from chrono_progress and game-data.
No migration of diagnostic collected counts into mission points is permitted.
Content identity/version mismatch blocks restore; never silently reset or reinterpret.
Persistence succeeds before a transition becomes visible or AR is launched.
No Cloud, map-provider or ARCore configuration change. No Unity edits.

This directory is a preparation module, not a separately installable APK.
Integrate after reviewing the pinned Maps and AR branches described in README.md.
