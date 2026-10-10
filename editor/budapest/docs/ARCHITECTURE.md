# Budapest editor architecture — v0.1

Browser UI (Leaflet map, forms, previews) → same-origin localhost Node 20+ HTTP service → allowlisted Budapest v2 JSON → validation + SHA-256 revision → local backup/temp/fsync/rename → normal Git diff/PR.

**Read:** GET /api/list, GET /api/file?path=..., GET /api/qa.
**Write:** POST /api/validate, POST /api/save with JSON document and expected SHA-256 revision.
**Root:** game-data/budapest/v2 only. No public hosting, cloud backend, account, database or GitHub token.

Existing unknown JSON fields are preserved. Dialogues use scene and sequence IDs; skip summaries live in production/act-N/scene-flow.json. Missions, dialogues, characters, source IDs and waypoint IDs are linked by stable IDs. The adapter does not change the Budapest content schema version or auto-migrate v1.

**Proposed draft location model:** waypoint primary latitude/longitude; optional safeStandingPoint and arAnchor WGS84 coordinate objects, and existing zones array. Editing coordinates resets fieldVerified=false, navigationEnabled=false and coordinateStatus=survey_required_v2. This is **not** a runtime ARCore world pose or surveyed geofence contract; a separate approved schema revision and field survey are mandatory before runtime consumption.

Validation includes waypoint ranges, ID uniqueness, dialogue sequence references, quiz index, AR/2D success-condition parity, flow next targets, and cross-file mission/source/waypoint/character/dialogue references. These checks complement the canonical Budapest v2 validator; they do not establish historical truth, legal media rights or physical AR correctness.

Backups are local and gitignored. Undo is an in-memory snapshot, diff is path-based, and SHA-256 revision prevents stale saves. Multi-file transactions and OS-level locking are not yet supported. Security: localhost-only binding, strict Host/Origin, no CORS, path allowlist, canonical realpath, request cap, CSP, no credentials. External Leaflet code is pinned to v1.9.4 and SRI; OSM tile network access remains necessary for the map.

**Next gates:** approve coordinate/zone/anchor schema, survey 11 waypoints, integrate Android Host + Web Maps + ARCore and 2D fallback, audit sources and rights, test UI on browsers and physical devices. Keep PR draft until acceptance.
