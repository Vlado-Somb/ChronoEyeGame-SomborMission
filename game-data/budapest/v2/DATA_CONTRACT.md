# CHRONO EYE Budapest v2 — local content contract

Version: 2.0.0-budapest-draft.1. A content authoring candidate, NOT a globally deployed runtime schema.

## Loader
- Read manifest.json from game-data/budapest/v2. Reject unsupported requiredCapabilities with an explicit message; never silently fall back to legacy v1 acts 0–5.
- The seven mission JSONs and seven dialogues are mandatory; production scene-flow is authoritative. Task, dialogue, choice, and mission_completion nodes must be honored in their graph order.
- All dialogue.sequence entries are ID-based: line -> lines, stage -> stageDirections, cue -> mediaCues, beat -> gameplayBeats. The prologue has the same format as acts I–VI.
- Dialogue Skip records that node's specific skipSummary and preserves the transcript, but never awards a task or skips validation.
- A choice uses the chosen choice.id and exact next target, saves optional setFlag in the same transaction; no penalty. Act VI supports two valid endings and a repair path. A prologue rules-choice converges safely.
- Task success is typed. An evidence map must satisfy every ar.successCondition.allEvidenceMappings exactly; fallback.successCondition must be identical. Returning from a diagnostic AR Activity is not task completion.
- A mission completion gate verifies required task IDs, awards only once and unlocks next mission only after MISSION_COMPLETED; replaying a dialogue or GPS event cannot duplicate credits.

## Geography, assets, safety
- A null latitude/longitude or fieldVerified=false prohibits production GPS arrival and automatic navigation. An explicit manual confirmation is marked manual, never forged as GPS. GPS waypoint is never an AR anchor.
- Crossing Elisabeth Bridge is map-only and must not initiate AR or require stopping with a raised phone. Act V shows the vanished temple's trace only; full construction in Act VI awaits spatial/archival confirmation.
- Planned assets are not real files: any null asset path must trigger original 2D/card fallback. Historical voices require original performed recordings, not invented authentic archival audio.
- Captions/transcripts, mute, reduced motion, safe standing, and offline evidence-cards are required.

## Session and scoring
- Player state is stored separately from content with gameId, contentVersion, currentFlowNodeId, completedTaskIds, completedMissionIds, scoreAwardsByKey, unlockedCollectibleIds, processedAttemptIds, storyFlags, evidenceSelections, firedTriggerIds, savedAt.
- Award identity is stable across contentVersion changes: gameId/task-or-mission-id/awardType, not per version. Per-task +10 and mission +30 in this content profile (7 × 60 = 420), no hint or retry penalty.
- Budapest v1 -> v2 automatic progress migration is prohibited, especially because the former Act V is no longer the finale. Archive old state and require a reviewed migration map and explicit version acceptance.

## Release gate
- No built Android APK, Web Maps/AR engine integration, device GPS/ARCore survey, production media rights, or full historical fact audit is asserted by these JSON/MD files. See QA_REPORT.md and the per-act QA checklists.

## Изузетак: редослед V акта
Act V graph почиње сценом пешачке навигације (bridge) и скоро непримећеног места храма (near_absence), а тек затим захтева задаћу GPS/manual ARRIVE код Златног јелена. То не значи потврду GPS-а док је телефон у покрету: навигација не покреће AR, сценски говор само кад се стане, недоказане координате остају null. 
