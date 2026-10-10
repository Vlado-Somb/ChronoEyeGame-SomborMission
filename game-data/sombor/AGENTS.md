# CHRONO EYE — EDU mission rules (Sombor)

Scope: `game-data/sombor/**`. Mission type: `edu_mission`.
Read `../../docs/mission-standard/MISSION_PROFILES_v0.1.md` and root `AGENTS.md`.

- Historic claims need traceable sources, editorial review and clear separation from illustrative presentation; no magical or fictional explanation presented as history.
- Mission tasks and answer keys in `v1/missions/*.json` are authoritative. Compute inventory from active tasks; do not hardcode counts from outdated manifest values.
- `v1/ar-proposals.json` is an editorial index, not a second source of active tasks.
- Each active AR learning objective needs equivalent 2D completion and scoring, with safety-aware GPS/AR usage.
- Scoring, bonuses, collectibles and album follow Sombor package configuration; do not impose them on Budapest.
- Maintain production brief, scene-flow, asset manifest and QA checklist per mission. Distinguish asset planned from asset shipped.
- Preserve original Unity assets and scripts; migration and editing occur in portable data, not legacy source.
- Sombor editorial UI is distinct from Budapest story/scene editing. No secret/token in browser content.
- Mark device tests as verified only after actual execution.
