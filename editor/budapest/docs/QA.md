# Budapest local editor QA — 2026-10-10

Automated tests included: path allowlist/traversal, SHA digests, WGS84 pairs, unverified-navigation guard, dialogue sequence references, quiz indices, AR/2D parity, cross-file broken references. Run npm --prefix editor/budapest test and npm --prefix editor/budapest run check. GitHub Actions workflow runs both plus the existing Budapest v2 static validator. **Do not claim CI passed until its run is inspected.**

Manual QA pending: open map; click/drag draft waypoint; confirm fieldVerified/navigationEnabled remain false; edit dialogue/subtitle and compare sequence preview; edit quiz and scene skip summary; save/reload/diff/undo; inspect backup and Git diff; force 409 with external file modification; reject cross-origin POST, invalid paths, malformed JSON and invalid coordinates; test Windows and offline/no-CDN fallback.

No live APK, ARCore, Google Cloud, Android Host, historical fact audit or GPS field test is included.


## Integration regression gates (2026-10-10)

- `test/content-contract.test.mjs`: 7 missions, 48 scenes, 375 dialogue lines, 11 waypoints, unique stable IDs, the `evidence_mapping` contract, AR/2D parity and cross-file links.
- `test/map-provider.test.mjs`: official OSM URL, attribution, adapter click/drag events, no Google provider and compatible Referrer/CSP configuration.
- Canonical `validate-v2.mjs` stays strict about release readiness. Browser/physical AR tests and tile-network verification remain separate acceptance steps.
