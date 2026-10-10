# CHRONO EYE — Mission Type Contract v0.1

Status: accepted_for_design; runtime adoption pending. Date: 2026-10-10.

## Manifest field

`missionType` is a **package-level** field in `game-data/<city>/v<major>/manifest.json`, not the type of each task. Accepted exact values:
- `edu_mission`: evidence-led educational missions (Sombor)
- `game_mission`: narrative game missions (Budapest)

One city may later host multiple packages/profiles. Each editor is separately launched and has a fixed configured package root; no automatic editor switching is required. The backend must enforce that root for every read/write and verify the manifest `missionType` matches the editor's configured profile. Do not infer task types from `missionType`.

## Backward compatibility and migration

This is an additive contract extension for candidate packages. Existing `schemaVersion` is retained for now to avoid falsely declaring full schema compatibility or breaking deployed readers. This document is the migration specification, not a claim of runtime support.

1. New or edited packages MUST explicitly declare `missionType`.
2. Older packages lacking the field MAY be read in **legacy compatibility mode**, but editors MUST show a warning and request explicit migration before saving. No silent rewrite of historical data.
3. Legacy compatibility lookup is constrained to known, documented package IDs: `chrono-eye-sombor` -> `edu_mission`; `chrono-eye-budapest` -> `game_mission`. Unknown IDs MUST fail closed and ask for profile selection.
4. Invalid values, empty values, or a conflicting declared profile MUST fail validation; do not silently substitute a default.
5. On migration: copy package to a review branch; add `missionType`; validate all referenced files and profile rules; record change in package changelog; approve and merge through PR. Preserve stable IDs and player-state migration.
6. Before marking the field required by JSON Schema and incrementing the package schema version, update all producers/readers and test older packages. No assertion of full runtime verification until device/editor tests pass.

## Validation boundaries

Core checks: manifest identity, allowed missionType, relative resource paths, unique IDs, referenced resources, secrets absent.
`edu_mission` checks: educational tasks, source review, scoring rules, AR/2D parity where applicable.
`game_mission` checks: acts/scenes, narrative references, dialogue and branching conditions; Sombor scoring/quiz counts are not mandatory.

## Source of truth

Shared technical contract: root `CHRONO_EYE_STANDARD.md` and `docs/mission-standard/`.
Profile rules: `game-data/sombor/AGENTS.md` and `game-data/budapest/AGENTS.md` on the documentation feature branch until merged.
Existing `MISSION_STANDARD_v0.2.md` is retained as historical reference; profile-specific examples do not become universal requirements.

## Rollout gates

Documentation + manifest field are only phase 1. Phase 2: schema validators and editor dispatch; phase 3: Android/Web runtime and regression tests; phase 4: release approval. Until then mark integration `not_verified`.
