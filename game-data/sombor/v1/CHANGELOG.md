# CHANGELOG — CHRONO EYE SOMBOR

## 2026-10-08 — v1.1 editorial and production package

**Decision log** (data-only branch `content/sombor-portable-game-data-20261008`):

1. Kept existing 9 mission locations and original historical quiz tasks. Five AR interactions are now **active mission tasks**, one per GK, MKG, NPS, LK and EB; total **31** tasks.
2. GK is the reference production mission; the **historical quiz about 1842 remains**, with original answer order and preserved `legacyOriginal`. Ćelavi trg and Branković/Magistrat history are documented in mission narrative.
3. Set GK GPS approach-zone completion temporarily to zone 2 rather than forcing a noisy 7 m inner circle; final radius and polygon must be field-verified.
4. Created Professor Vlada mentor class and placeholders for local historical shadows/voices. The first mentor has prepared voice scripts, summon behavior and separate 2D and 3D asset requirements.
5. Formalized four collectible classes: unique per-location relic, repeatable golden 1749 seal (small sourced history bytes and limited score), romantic heart, and comedic joker. Created user-facing Herbarijum Sombora album specs and one-time reward keys.
6. Added original SVG concept files for AR target, tower, token categories and a temporary mentor portrait. No historical seal facsimile, 3D `glb`, voice file, or captured 2D fallback photo exists yet.
7. Created production brief, scene flow, asset manifest and acceptance checklist for GK.
8. For LK, the accepted AR mini-game orders the words **Santa / Maria / della Salute**; further poetic fragments may be explored editorially later.
9. No Unity application code, frontend, Android APK, API keys or external maps have been modified.

**Validated:** JSON cross-references, mission/waypoint/hint identifiers, task count, sample score rules, SVG basic syntax/availability on the GitHub branch. Validation is static, not an on-device AR test.

**Pending:** visual references and safe GPS location calibration, final 3D models/animations, sound production, original/permission-cleared facade photo, independent Web+Android+ARCore task runtime, accessible album UI, tests.

## 2026-10-08 — v1.0 extraction

Imported existing Unity ScriptableObject/YAML contents into portable JSON: 9 missions / 26 tasks, 13 GPS waypoints, 4 dialogue resources (one inactive test), 13 hint texts, 26 timeline registry blocks, 10 trigger groups, 18 conditions, and mechanics summary. All unedited historical sources remained in repository.
