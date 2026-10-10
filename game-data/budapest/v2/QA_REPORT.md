# CHRONO EYE BUDAPEST v2 — STATIC QA REPORT
Date: 2026-10-09; branch: content/budapest-v2-editorial-finalization-20261009

## Summary
**PASS — in-session static content verification.** This is NOT a PASS for Android, ARCore, Web Maps or a public release.
The same repeatable rules are provided in tools/validate-v2.mjs (Node 18+); its JavaScript body passed syntax compilation with ESM imports removed for the syntax check. The standalone Node CLI has NOT been executed in this environment and must be run in CI or a local checkout before merge.

## Checks actually performed through the GitHub connector
- 7 mission JSONs, 7 dialogue JSONs and 7 scene-flow JSONs read and parsed.
- 21 tasks, including 7 GPS/manual arrivals, 7 AR/2D evidence tasks and 7 knowledge checks; all quiz correctIndex values are in range.
- All 7 AR and 2D task successCondition objects match exactly; all task completion narratives are present.
- 48 distinct scenes, 375 dialogue lines, 492 ID-based sequence entries; all dialogue sequence references and character speaker references resolve.
- 7 flow graphs: all 79 nodes reachable, no missing target nodes, no dead ends, no unintended graph cycles. 11 complete traversal paths, including prologue alternatives and the final repair branch.
- All 48 dialogue nodes have scene-specific skip summaries and transcript-preserving policy.
- 7 triggers map to 7 timeline blocks; signal-to-scene references resolve.
- 7 complete four-file production directories (28 files total), verified by listing each directory on GitHub.
- 13 registered historic/editorial sources; 11 registered waypoint IDs, 13 asset registry entries; checked mission source/waypoint/asset/evidence references. Four supported collectible types are declared, with seven authored relics.
- Act V ordering repaired and rechecked: bridge navigation -> near temple absence -> Golden Stag arrival -> Pačić -> return to temple -> Gellért. On the bridge, no AR while walking.
- Scoring policy was amended to once per stable award key, not a new reward on each contentVersion.

## Historical spot checks
- Centrál Grand Café's institutional history states opening in 1887 (Károlyi utca 9). The fictional agency record 1886 is deliberately wrong.
- Serbian Institute: Tekelijanum founded 1838; current building erected 1907–1908. Mihailo's former salon is a separate route location.
- National Széchényi Library's Tabán history describes the Serbian church demolished in 1949. Neither that record nor supplied photographs provide an authoritative survey anchor.
- Every historical person's new dialogue, Circle membership, secret sign and temporal code are fictional dramatization. The complete line-by-line historical source audit remains open.

## Release blockers — not falsely marked as PASS
- **11/11 waypoints unverified** and without production coordinates/approved safe standing positions; no real GPS or route distance results.
- **13/13 registered media assets planned_not_created** (no shipped GLB/OGG/WebP, recorded voices or verified reproduction rights). New production asset-manifests are specifications, not binaries.
- No Unity/native Android loader, Web Maps integration, 2D render execution, ARCore camera/world anchoring, VPS or physical device tests.
- No end-to-end test of session restore, per-task AR result correlation, native progress store, idempotent scoring on hardware, accessibility or offline media.
- No formal authorization for reuse of historic photographs, audio and models. A linked historical source is not a media license.
- Complete archival/geospatial fact checking of Tabán coordinates, measurements, historical reconstruction phases, and each historical claim still requires reviewer sign-off.

## Editorial state and next phase
**The Budapest v2 screenplay, migration/authoring contract and production specification are ready for editorial fine-tuning.** Nothing has been deployed and the existing Sombor campaign and shared runtime remain unchanged.

To rerun locally: node game-data/budapest/v2/tools/validate-v2.mjs
Then complete the unchecked per-act QA items and run on-device integration before marking verified_in_test.
