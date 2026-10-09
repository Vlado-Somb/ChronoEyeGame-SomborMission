# Validation — 2026-10-09

Source candidate, Linux scratch runtime, OpenJDK 17.0.20 compiler module,
Java source/bytecode target 11. No Android device or Android SDK build used.

`sh android-host/test.sh`:
- JVM engine compilation and regression harness: PASS.
- Web adapter syntax and event contract regression: PASS.

Covered: ordered completion, duplicate task/mission award prevention, wrong-session
AR rejection, cancelled AR without points, duplicate result, simulated persistence
failure without state advancement, defensive snapshot, restore retaining start time
and score, content-version mismatch, process loss invalidating pending AR, finish.
Web: selected mission intent, no echo loop when native state reselects mission,
no map-position completion handler, native location forwarding, malformed JSON.

Not covered: Kotlin AndroidSessionStore compilation/AtomicFile crash recovery,
Android lifecycle wiring, real Activity results, GPS, AR rendering, 2D educational
equivalence, Google map tiles, full canonical content loading. These remain pending
and are enumerated in README.md. This is not end-to-end POC verification.
