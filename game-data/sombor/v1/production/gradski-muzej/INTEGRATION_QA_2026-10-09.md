# CHRONO EYE Sombor — static integration audit, 2026-10-09

**RESULT: NOT READY.** All nine active mission JSONs were parsed from the GMS PR branch. No main changes or runtime tests.

## Confirmed
- 9 mission JSONs, 34 active tasks: 9 GPS, 8 AR, 12 multiple-choice, 5 text-input. Task IDs are unique. All referenced AR SVG asset paths exist.
- GMS legacy-gms.json matches the unedited current GMS mission; editorial replacements are only candidates.
- PR #11 ZUP live mapping uses 1697/1808/1898; PSM-03 live question uses the eighteenth century and retains the original 1751 in legacyOriginal.
- GK and LK AR fallback now explicitly declares sameScoring=true and mayActivateWithoutCamera=true (commits a8926c9, c2e6213).
- GMS educational design uses four collections; quiz candidate asks what happened in 1945 instead of choosing among three close dates. A new original SVG, assets/gms-four-collections-relic.svg, was created.

## Blocking findings
1. GMS live missions/gms.json still has only three tasks, no GMS-AR-01, GPS zone 3, and a stale question about the 'current' exhibition. Attempts to update this file were blocked by the connected write tool. Legacy must remain unchanged.
2. Global manifest and collectibles are not integrated for GMS; GMS-AR-01, gms-four-collections-relic, gold-gms-001 and gold-gms-002 are not registered. Candidate collectible/registry and asset manifest writes were blocked. Scene-flow now references the new relic, but candidate registry still references the old lantern.
3. Manifest inventory counts are stale: actual 12 multiple-choice / 5 text-input, versus declared 11 / 6. After GMS AR integration expected totals are 35 tasks and 9 AR (12 multiple-choice, 5 text-input).
4. PR #11 ZUP uses correct 1808 and 1898 mapping but the referenced SVG filenames remain zup-hall-1898.svg and zup-painting-1896.svg; AR proposal still describes the old 1697/1896/1898 grouping.
5. LK is marked editorial_pending and lacks the four required production-package documents. Other missions have draft/asset/runtime statuses, not device verification.
6. Editorial deviations to review: ZUP opening refers to clocks inexplicably ticking and 'Chrono Eye' mixing time; NPS opening imagines bells; GMS draft candidate lantern conflicts with the approved factual tone. Direct writes to ZUP/NPS were blocked.
7. GPS approach zones and camera behavior have not been field-tested. GLB, OGG, WebP and Android/ARCore integration remain pending.
8. Sources disagree on the historical name of the museum building's former owner (Fernbah vs Lederer). The 1945 move is supported; do not use the owner as a quiz answer until verified.

## Decision
Do not merge PR #14. Do not schedule the backend editor until GMS live integration and full cross-file QA pass. Keep PR #11, #13, #14 in draft status.

## Historical sources
- https://vojvodina.travel/atrakcije/gradski-muzej-sombor/ (1883, 1887, 1945, collections)
- https://www.politika.rs/sr/clanak/545587/gradski-muzej-u-somboru-obelezava-140-godina-postojanja (museum director on 1883 and 27 October 1945)
