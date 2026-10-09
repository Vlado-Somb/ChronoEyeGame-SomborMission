# Diagnostic package v1

Each AR Activity opening has a UUID sessionId, UTC ISO start and Android
elapsedRealtimeNanos origin. Pause/resume stays in that session; a new Activity
creates another ID. Frame/depth timestamps are ARCore nanoseconds and are only
compared with each other. Use captureElapsedMs/elapsedMs to align screen recordings.
The HUD shows an 8-character prefix of the full UUID and elapsed seconds at 1 Hz.

- `manifest.json`: build/device/ARCore version, clocks, normalization, limits,
  unavailable measurements, depth/occlusion configuration and units.
- `events.jsonl`: {sessionId, elapsedMs, type, data}. Worker-generated snapshot
  status records also carry elapsedMs; session is unambiguously the containing
  manifest. Events include session_start/end, tracking changes, place/select/
  collect/reset, modes, pause/resume, errors, marks, snapshots, system and samples.
- `series.csv`: 1 Hz render-loop interval FPS, mean/max frame intervals, counts
  >50 ms (per interval), latest depth age/state, occlusion, center depth, selected
  object's center Z, object count, cumulative missing/stale counts, full/basic
  mode and producerMs. Empty numeric cells = unavailable. This is delivered frame
  cadence including ARCore blocking update, not GPU render duration.
- `system` events: every ~2 s on worker while resumed and full logging: own-process
  PSS/private dirty/native/Java heap KiB, process CPU elapsed ms and utilization
  normalized to one core (can exceed 100%), Android thermal severity, battery
  percent and battery temperature. Battery temperature is NOT CPU temperature.
- `sample` events add camera/anchor poses and pair distances when full mode is on.
  Pose translation in meters; quaternion qx,qy,qz,qw; anchor ID session-local.
  Poses retained while not tracking are estimates, not valid localization proof;
  pair records explicitly carry bothTracking.
- `depth-*.u16`: tightly packed row-major unsigned little-endian 16-bit depth in
  mm, 0 missing, camera optical-axis Z. No raw confidence and no RGB/audio.
- Sidecar JSON: width/height, original format/strides, packed stride, ARCore frame
  and depth timestamps, capture elapsed, age, fresh flag, camera pose/intrinsics,
  column-major view/projection matrices, display rotation/viewport and NDC quad
  → TEXTURE_NORMALIZED UV quad. A problem snapshot can be stale, explicitly marked.
  A failed acquisition produces snapshot_unavailable rather than fabricated data.
- `export.json`: cutoff elapsed time, dropped queue count and current write state.
  ZIP is a consistent prefix at the worker barrier; later events belong to a later
  export. `end.json` appears only once Activity cleanup finishes normally.

Limits: 128 queued jobs, at most 2 queued depth snapshots, 200 snapshots, ~64 MiB
conservative session accounting, 30 min logging duration, 5 session directories
and 3 export ZIPs retained. Background-only disk/ZIP/system queries; frame thread
copies image bytes before close and enqueues immutable records. The worker drops
excess work (counter exposed) rather than blocking rendering. Abrupt process kill
can omit session_end/end.json; flushed JSONL/CSV prefixes remain usable. Export is
available inside the current AR session; share before leaving it. Old sessions
are retained privately but do not yet have an in-app archive browser.

Full/basic A/B: basic keeps 1 Hz frame/depth samples and events, disables periodic
snapshots, pose/pair serialization and 2 s resource sampling. `producerMs` measures
depth processing/upload plus diagnostic preparation, NOT pure logging overhead.
Use matched full/basic runs and analyzer summaries; do not infer causal overhead
from unmatched scene segments. There is no claimed physical overhead measurement.

Depth acquisition stays on when occlusion is off, enabling independent heatmap
comparison. `active` means fresh image available, not all pixels valid. Zero pixels
show checkerboard in heatmap and never occlude. Freshness: 0..100 ms inclusive.
Camera-axis Z from inverse pose and GL depth reconstructed via projection agree
in units; inherited shader bias -80 mm and blur remain, so center Z is a diagnostic
reference, not exact fragment/surface Z. The shader uses nearest DEPTH16 sampling
to avoid interpolating separately packed bytes. No guessed confidence values.

Primary reference (checked 2026-10-09):
https://developers.google.com/ar/develop/java/depth/developer-guide
