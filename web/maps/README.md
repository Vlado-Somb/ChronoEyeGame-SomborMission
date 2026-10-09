# CHRONO EYE — Web Maps package v0.1 (candidate)

**Status:** `accepted_for_design`, browser map prototype; **not** Android/WebView/ARCore verified.  
**Date:** 2026-10-09.  
**Source map implementation:** `Vlado-Somb/serbian-heritage-map-hungary`, `assets/maps-loader.js`, `assets/app.js`, `assets/styles.css`, `index.html`.  
**Mission source:** `game-data/sombor/v1` from branch `content/sombor-portable-game-data-20261008` (candidate; not yet in `main`).

## What is reused / adapted

From Heritage Map: Google Maps **JavaScript** rendering approach, own JSON-owned location markers, overlay/circle/polygon principles, marker click → information card, responsive Web layout and restricted **browser** API key approach. This is a focused rewrite, **not a copy of Heritage Map**, its editorial database, or its API key.

Map is a **presentation adapter**. The `waypoints.json` WGS84 coordinates remain authoritative. No Places/Roads/Routes API, geocoding, Firebase, ARCore, Unity or session management is required to display this map.

## Structure

- `index.html` — standalone Web UI
- `config.js` — intentionally empty API key; runtime options
- `geo.js` — reusable WGS84 distance, zone classification and validation
- `maps.js` — Google Maps adapter, offline coordinate preview, selection, location update interface
- `styles.css` — independent responsive layout
- `data/sombor-demo.json` — **generated candidate snapshot**, not a second authoritative content source
- `tests/maps.test.cjs` — Node built-in tests

The app first attempts `../../game-data/sombor/v1/manifest.json`, `waypoints.json`, and each referenced `missions/*.json`. This path becomes active once the portable content PR is merged into `main`. Until then it falls back to the pinned demo snapshot from the candidate branch. Do **not** manually edit the snapshot as game content.

## Quick start

Run an HTTP static server **from the repository root** (do not use `file://`):

```bash
python3 -m http.server 8080
# open http://localhost:8080/web/maps/
```

With the default empty `googleMapsApiKey`, the app shows an honest **coordinate-only preview**, not Google street tiles. Mission selection, GPS-zone visualization, marker click, and simulated player movement work without any Google key.

For a real Google basemap: create a **separate CHRONO EYE Maps JavaScript API browser key**, restrict by exact HTTPS origin/referrer and by the Maps JavaScript API, then set `googleMapsApiKey` in a **local or deployment-injected** config. A browser key is publicly visible by design; the restriction, not hiding it, is the control. **Do not copy the Heritage Map key.** Do not commit private/server credentials. For public GitHub Pages, the referrer restriction must cover the exact deployed Pages host/path. For localhost, create a dedicated restricted dev key and never deploy it. Cloud billing/quotas need owner approval.

**WebView warning:** local `file://` or asset origins can omit HTTP referrers and break restricted browser keys. Use a tested HTTPS origin or revisit native Android Maps SDK only if necessary; do not weaken key restrictions to bypass this.

## Demo features

- 9 Sombor missions from the current candidate package, 12 visible point markers (13th `SO_GLOBAL` is a broad invisible game trigger, intentionally not drawn as a POI).
- Select a mission by list or marker; show story intro, waypoint, task count and unverified-zone warning.
- Display three circles (outer / approach / close) only for selected mission.
- Optional GeoJSON `Polygon`/`LineString` overlay when a waypoint carries `geometry` (not present in current Sombor package).
- `Следећа тачка` / `Аутоматски` simulates positions **without** completing a task.
- `Моја локација` uses browser geolocation only after explicit click and browser permission (HTTPS/localhost secure context required).

## Android Location Engine → WebView contract (future integration)

Native code will call (on WebView main thread; bridge security and lifecycle still to be designed):

```js
window.ChronoEyeMaps.setPlayerLocation({
  latitude: 45.7724780193,
  longitude: 19.1138807951,
  accuracyMeters: 8,
  timestampMs: Date.now(), // UTC Unix epoch milliseconds, not Android elapsedRealtime
  source: "native"
});
```

Returns `true` if accepted, `false` for invalid/outdated/out-of-order coordinates. Accepted data is **not stored**, sent to Google by this module as player telemetry, or committed to Git. Rendering Google map tiles may of course contact Google under their service terms.

Available API: `setPlayerLocation`, `clearPlayerLocation`, `selectMission(id)`, `fitAll()`, `getStatus()`. Events emitted on `window`:
- `chronoeye:map:ready` — `{gameId, mode, missions, waypoints}`
- `chronoeye:map:mission-selected` — `{gameId, missionId, waypointId, source}`
- `chronoeye:map:position-updated` — `{gameId, source, accuracyMeters, timestampMs, selectedMissionId, distanceMeters, zoneCandidate, confirmed:false}`

**Critical:** `zoneCandidate` is **display-only**. Android/Game Engine must validate accuracy, temporal dwell, hysteresis, safe approach and authoritative mission/task conditions. Only the Game Engine may emit `GPS_ZONE_CONFIRMED` and award points. A marker tap, simulator or inaccurate fix never completes a task.

`SO_GLOBAL` remains in canonical data but is not drawn as a map POI; its game trigger remains outside this map adapter. GPS waypoint coordinates are not AR anchors; legacy altitude is ignored.

## Tests

```bash
node --check web/maps/geo.js
node --check web/maps/maps.js
node --test web/maps/tests/maps.test.cjs
```

Static checks validate data references, 9 mission IDs, 13 waypoints, coordinate range, ordered zones, Haversine distance and zone boundaries. **They do not test** Google billing, API referrer restrictions, live browser rendering, native Android bridge, GPS outdoors, iOS WebView or ARCore.

## Integration / acceptance checklist

1. Merge/resolve portable game-data PR first; then confirm runtime loads **canonical** data rather than snapshot.
2. Provision restricted Maps JavaScript browser key, exact allowed domain, API quota and billing alerts; verify on HTTPS.
3. Verify Google rendering and marker/card behavior on desktop and mobile.
4. Test Android WebView under actual production origin and signing configuration.
5. Connect native Location Engine with precision and timestamp; test background/resume and bad fixes.
6. Connect Game Engine to verified zone events and session state, then integrate ARCore separately.
7. Field-verify all waypoint positions, safe standing areas and three zone radii.
8. Decide whether to keep or remove demo snapshot after data branch merges.

No edits to the Heritage Map repository, Unity assets, production content JSON, credentials, Google Cloud, Android code or ARCore configuration are included in this PR.
