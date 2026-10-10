# Connector publication status — 2026-10-10

This update supersedes earlier access/publication limitations in the historical audit report. GitHub Connector get_repo, branch/tree/commit creation and Draft PR creation succeeded. No browser or OAuth was used.

Pinned main: `814b85d19909bb07bc07b65da784acd9d05dfaaa`.

| Draft PR | Branch | Published head | Validation |
|---|---|---|---|
| [#21 Budapest v2 content](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/21) | integration/budapest-v2-content-20261010 | 9dfd95be83bc4c909447654aee4263ed7ddeed25 | [CI 38075944531](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/actions/runs/38075944531): success, static-content job and validator step successful |
| [#22 Budapest editor](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/22) | integration/budapest-editor-20261010 | 3427ad4c4474ea5cd762f36ece528c8212efd445 | [CI 38075946171](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/actions/runs/38075946171): success, syntax, unit tests and static validation successful |
| [#23 Audit and archive gates](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/23) | integration/repository-stabilization-20261010 | initial publication 176be2e1d39ac970ffc3e59f3ee3f459b415d92e; this file is a follow-up | Documentation/inventory/verifier preparation; no CI claim |

#22 targets #21's branch. #21 and #23 target main. All three reported mergeable=true before this documentation follow-up; no merge was performed. Content and editor published trees exactly match locally tested trees: 8b305c14770f703ac9e14f41a577e2e19b161452 and b237feecee19b4dbdf4dd76bd074c1a97b0da940.

## Private Unity archive preparation

[ChronoEye-Legacy Draft PR #1](https://github.com/Vlado-Somb/ChronoEye-Legacy/pull/1), branch `archive/preparation-20261010`, head `8d34a424a055937bf4d19f00ce2e4b6cbf569afd`, preserves the owner's README and adds the inventory, plan and verifier. The destination was confirmed private. This is preparation only: Unity files/history have NOT been transferred, archive completeness has NOT been established, and no sources were removed. Full object/history transfer, fresh-clone recovery verification, external/LFS dependency review and approval are required before removal. Preserve the Tabán model dependency and all recorded historical source variants.

## Integration order and approvals

1. Review security #19 first. Its exact-head CI is green (run 37989475588), but source containment does not prove provider-side revocation. It also deletes APK/UserSettings files; review these explicitly. No Google Cloud access or credential operations were performed.
2. Review/merge audit #23 only with owner approval.
3. Review #21 as the corrected, explicit game_mission replacement candidate for #16. Do not close #16 until approved.
4. Review stacked #22 after #21; retarget to main and recheck diff/conflicts/CI after the base integration.
5. Selectively integrate #17's Tabán work after act-VI/model review, then #12 host work after resolving documented decision/changelog conflicts. Keep #8 Google Maps on hold and #18 AR as an experiment.
6. Consolidate the Sombor branch chain in the audit matrix; reconcile manifest counts and asset names before a new integration PR. Do not merge stale snapshots individually.
7. Complete and independently verify private archival before proposing any source removal.

The full disposition of all 15 pre-existing open PRs and 36 branches remains in REPORT.md and snapshots. Any future merge, existing-PR closure or Unity removal requires explicit approval. Fresh main/base validation is required at merge time.

## Scope limits

Static/HTTP tests do not establish mobile, field GPS, audio or visual readiness. Budapest retains 11 unsurveyed waypoints and 13 planned media assets; general media production remains deferred except the required Tabán church model. The Sombor GPS-zone event model remains the runtime integration direction. Browser visual QA and editor backup restore functionality remain pending. Main, existing PR states, cloud, credentials and public deployment were not changed.
