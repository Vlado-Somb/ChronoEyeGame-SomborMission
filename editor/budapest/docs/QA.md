# Budapest local editor QA — 2026-10-10

Automated tests included: path allowlist/traversal, SHA digests, WGS84 pairs, unverified-navigation guard, dialogue sequence references, quiz indices, AR/2D parity, cross-file broken references. Run npm --prefix editor/budapest test and npm --prefix editor/budapest run check. GitHub Actions workflow runs both plus the existing Budapest v2 static validator. **Do not claim CI passed until its run is inspected.**

Manual QA pending: open map; click/drag draft waypoint; confirm fieldVerified/navigationEnabled remain false; edit dialogue/subtitle and compare sequence preview; edit quiz and scene skip summary; save/reload/diff/undo; inspect backup and Git diff; force 409 with external file modification; reject cross-origin POST, invalid paths, malformed JSON and invalid coordinates; test Windows and offline/no-CDN fallback.

No live APK, ARCore, Google Cloud, Android Host, historical fact audit or GPS field test is included.
