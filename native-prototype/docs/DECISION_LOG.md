# AR Lab decisions — 2026-10-09

DEC-AR-20261009-01 — accepted_for_design, requested diagnostic build 0.2.0.
Read native AR base 0fef5c9 and Mission Standard v0.2 at 5ad5a7d (separate,
unmerged standards branch). This local addendum deliberately does not merge or
modify narrative/standards/maps branches. Native Activity + existing WebView
bridge remain; no orchestrator, Cloud activation, GPS or persistence added.

Filtered DEPTH16, millimetres, camera-axis Z; 100 ms gate retained. Camera,
heatmap and overlay independent of occlusion. Zero depth is missing, never
confidence. Keep original occlusion bias (-80 mm) and blur for comparison;
correct byte strides, invalid zero depth handling and initial aspect ratio.
Precompile shaders on surface creation, avoiding shader asset I/O on mode changes.

Local bounded telemetry with explicit UTC/monotonic clocks; background-only
writes and system sampling. FileProvider ZIP export, no RGB/audio capture.
No approved private ingestion endpoint/authentication found in reviewed AR code
or service registry; upload integration status = not_configured. No credentials
or network permission added. Future receiver requires owner-approved HTTPS URL,
short-lived user authentication, retention/access policy and upload receipt/retry
contract keyed by sessionId. Diagnostic logs are private device data, never Git.

Stable session-local object IDs and five colors. Anchors are not persisted.
Future integration: AR result Intent event artifact_collected + sessionId;
game engine remains responsible for scoring/idempotency. Diagnostic format
schemaVersion=1 is independent of mission schema; no mission migration.

Validation status: implemented_not_field_tested. See VALIDATION.md for actual
build/lint/analyzer checks. S25 Ultra physical test is still required.
