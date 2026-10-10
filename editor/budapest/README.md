# CHRONO EYE — Budapest Local Mission Editor v0.1

This is a **local-only editorial tool** for Budapest v2 JSON, not an APK, public backend or runtime deployment.

## Start

1. Check out the editor branch with the full repository and Node.js 20+.
2. At the repository root run: **node editor/budapest/server.mjs**
3. On the same computer open **http://127.0.0.1:4177/**.
4. Stop with Ctrl+C. Never port-forward the editor.

No npm install, Google Cloud key, Cesium token or database is needed. Leaflet 1.9.4 (pinned CDN + SRI) and OpenStreetMap raster tiles require internet; JSON editing and validation do not. Respect the OpenStreetMap tile usage policy.

## Supported in v0.1

- Map: select waypoint, click or drag a pin, edit WGS84 latitude/longitude, separate safe standing point, draft AR geospatial anchor and JSON zones. Existing null coordinates remain null until manually entered. Edits never enable navigation.
- Dialogues: scene selection, text, speaker, subtitles, voice asset IDs, sound cues, preview using sequence IDs.
- Missions: tasks, instructions, quiz choices/correct index and historical paragraphs.
- Scene flow: next links, choices and dialogue skip summaries.
- Sources: titles, links, scope, review status.
- JSON: advanced editing for characters, collectibles, album, triggers, evidence, media and other allowlisted Budapest v2 files.

Save validates and creates a local timestamped backup before atomic JSON replacement. Undo is browser-session-only, Diff shows changed JSON paths, Export downloads a file. Save does **not** commit, push, merge or deploy.

## Git workflow

Before editing check your branch and git status. After editing use the **Провери везе** button, then run:
- node --test editor/budapest/test/*.test.mjs
- node game-data/budapest/v2/tools/validate-v2.mjs
- git diff -- game-data/budapest/v2

Review the changes, then commit to a dedicated content branch and open a Draft PR. Do not merge directly into main.

The HTTP server binds only to 127.0.0.1, checks Host/Origin, does not enable CORS, allowlists paths, checks canonical realpaths, rejects stale revisions (409), and caps requests at 2 MiB. Localhost is not general authentication: do not run it alongside untrusted web pages. Backups and .env are gitignored.

The .env.example has no secrets. A future **Budapest-only** key requires a separate decision and must never reuse Sombor credentials. This editor does not read .env or call Google Cloud.

**Limitations:** safeStandingPoint and arAnchor coordinate object shapes are draft editorial proposals, not approved ARCore runtime contracts. No surveyed GPS points, altitude/heading, production AR anchors, live Android integration, fact-check sign-off or device tests are claimed. See docs/ARCHITECTURE.md and docs/QA.md.

## OSM/Google migration and provider contract (integration 2026-10-10)

The browser uses `public/map-provider.js` as an **adapter**: UI calls `create/onClick/clearPins/addPin/resize`, not Leaflet directly. Current provider is **OSM only**; Google is intentionally not implemented and no Sombor key is used. All coordinates remain WGS84 latitude/longitude in Budapest v2 JSON; safeStandingPoint, draft arAnchor and zones are separate. Changing a pin or draft point resets fieldVerified/navigationEnabled to false. GPS radius/zone runtime semantics and ARCore altitude/heading are not approved yet.

**OSM Foundation tile policy**: https://operations.osmfoundation.org/policies/tiles/ — the editor uses the exact `https://tile.openstreetmap.org/{z}/{x}/{y}.png` endpoint with visible attribution and a referrer-compatible response header. It relies on the browser's normal HTTP tile cache, does not proxy, scrape, prefetch or download offline tiles. Public tile service is best-effort and is for modest interactive local editing, **not** a production-game SLA. For offline/public rollout use a licensed hosted or self-hosted tile provider.

**Later Google migration**: implement a second adapter behind the same WGS84/marker contract only after creating **Budapest-specific** credentials and checking billing/quotas. Browser Maps JavaScript API keys are client-visible: use HTTP referrer + API restrictions and suitable quotas, not a claim that `.env` hides a frontend key. Android Maps/AR services require their own package+SHA-1-restricted credentials and platform-specific integration. Never reuse, copy, or migrate Sombor credentials. No Google code is active in this editor.

**Validation warning**: the canonical v2 release validator deliberately rejects *surveyed/filled waypoint coordinates* while the project is still in its unsurveyed release phase; editing draft pins locally is supported, but coordinate release requires an approved contract and a separate authoring/release gate. Do not bypass that release guard to make CI green.
