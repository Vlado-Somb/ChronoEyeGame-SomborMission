# CHRONO EYE — Mission profiles v0.1

Status: accepted_for_design (2026-10-10). This is a routing and editorial contract, not proof of runtime implementation.

## Core vs profile
Core is shared: stable IDs, versioned JSON, provenance, privacy/security, Android session lifecycle, maps, GPS/AR capability checks, player-state isolation, validation and release gates. Core MUST NOT prescribe narrative structure, scoring, collectibles, required AR count, or a single editor UI for every game.

A package declares `missionType`: `edu_mission` or `game_mission`. A city is an example, not the discriminator; future cities may contain either profile. Until migration, use path routing: `game-data/sombor/**` => edu_mission; `game-data/budapest/**` => game_mission. Never add a required manifest field without schema/version migration.

## edu_mission (Sombor)
Evidence-led place-based learning. GPS arrival, factual explanations, knowledge checks, configured scoring, collectibles where defined, AR interaction with equivalent 2D completion. Educational objective and answer key must be reviewable. Local editor: `tools/sombor-editor/` (implementation status independent of this specification). Preserve Unity legacy source untouched.

## game_mission (Budapest)
Narrative game with acts, scenes, characters, dialogue, discoveries and branching/conditional story state. Story editor and scene-flow validation are profile-specific. Scoring, collectibles and AR fallback requirements are explicitly configured per game, not inherited from Sombor. Narrative invention must not be misrepresented as sourced history.

## Resolution order
1. Root AGENTS.md: safety, repo-wide invariants and routing.
2. Core standard: shared architecture and versioning.
3. Profile AGENTS.md and profile specification: mission-type rules.
4. Package manifest/schema and editorial decisions: concrete data.
Conflicts: security and data protection win; otherwise record an architectural decision and version the contract before changing behavior.

## Release/QA
Validate core invariants plus profile-specific rules separately. Editorially active != implemented in runtime != device-verified. Never merge unvalidated changes into main. Existing Mission Standard v0.2 remains historical design reference; its Sombor-specific examples are NOT universal rules.
