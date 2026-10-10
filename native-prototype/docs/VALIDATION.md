# Validation — 2026-10-09 — AR Lab 0.2.0

Source APK commit: e170aef98ad6ec28412193ec79fbc736ed60e66f.
Base: native-ar-prototype 0fef5c989e0e7a79c03dd4e5a5433fb452edaece.

- Local `:app:assembleDebug :app:lintDebug`: PASS (JDK 17 / Android SDK 35).
- Lint: 0 errors, 46 warnings (including prototype localization/layout warnings,
  locked orientation, ARM64-only support and pre-existing restricted WebView JS).
  Warnings have not been suppressed and this is not a production release gate.
- `apksigner verify --verbose --print-certs`: PASS, APK Signature Scheme v2,
  one RSA-2048 Android Debug signer.
- `aapt dump badging`: rs.chronoeye.prototype, versionCode=2, versionName=0.2.0,
  minSdk=26, targetSdk=35, ARM64. Camera permission; no INTERNET permission.
- Local delivered APK SHA-256: `385b087db2a765a22533756331b0d491c317a1ebdea21262cb20ca000cac644d`.
- Signer certificate SHA-256: `354a03a6e8c9b7956872dbde6c792726a721ffcda3eb98ea0efbd2c455fd63e9`.
  CI debug keys can differ; CI APK is a separate signed binary from local delivery.
- Python offline analyzer: 2 synthetic tests PASS (valid ZIP/report/native and
  screen heatmaps; truncated depth rejection). These are not phone measurements.
- Mesa surfaceless EGL ES3 compile checks: PASS for diagnostic background,
  occlusion enabled/disabled and tinted environmental HDR fragment shaders.
  Shader compilation is not on-device rendering or driver compatibility proof.
- Source whitespace check: PASS.
- Existing GitHub native APK workflow enabled for diagnostic branch; CI result
  is independently visible in Draft PR #9. No branch has been merged.
- Physical Samsung Galaxy S25 Ultra: NOT RUN. No measured FPS, depth accuracy,
  thermal behavior, drift, logging overhead, share-target availability or lifecycle
  success is claimed for this build. Follow FIELD_TEST_SR.md.

Build environment initially needed SDK/JDK and proxy/trust-store setup. A Java
checked JSON exception and a shader declaration-order error were found and fixed
before the successful checks above. The 100 ms freshness gate was not relaxed.

Native prototype plus its build workflow only. No Unity, game-data, Maps, other
branches or ChatGPT automations changed. Public repository has code and synthetic
fixtures only, never user session logs or credentials.

## V4 source changes pending CI and field QA
- Branch: chronoeye/ar-depth-v4-20261009 (based on AR Lab 0.3.0 branch).
- Fields: per-frame acquisition outcome, camera/depth repetition, watchdog and
  recovery events; default 10 s DEPTH16 snapshots; complete session ZIP share.
- Acceptance: GitHub Actions assembleDebug/lintDebug/unittests/signature verification.
  On-phone acceptance: 10 min, 5 collectibles, 100 ms gate, timestamps,
  snapshots, session_end/end.json/sessionContinues=false.
- No phone evidence for V4 yet. A watchdog reports stalls; it does not magically
  repair ARCore's missing image stream.
