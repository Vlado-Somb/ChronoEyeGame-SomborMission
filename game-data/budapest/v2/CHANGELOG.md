# Budapest v2 candidate changes

## 2.0.0-draft.2 — 2026-10-10
- Add explicit `missionType: game_mission` under Mission Type Contract v0.1; retain candidate schema version per its additive migration rule.
- Preserve all PR #16 authored content and stable IDs. Import only the existing prologue `evidence_mapping` correction from editor head 5a260dfe (the only content delta there).
- Add repeatable content CI and reject missing/conflicting missionType.
- No automatic v1 player-state migration. No runtime, GPS, cloud or media activation.
- Validator task/count/scoring checks describe this particular inherited draft. They are NOT mandatory EDU rules for future GAME content. A later narrative mechanics revision must update these package assertions explicitly; this stabilization does not rewrite authored scenes or tasks.

Verification: original PR #16 validator fails at BP2_A0_EXPLORE; corrected candidate passes. 11 unsurveyed waypoints and 13 uncreated media assets remain release blockers. GPS zones are adopted for future scene/audio triggers; no field coordinates are invented here. Media production remains deferred except the separately reviewed Tabán model.
