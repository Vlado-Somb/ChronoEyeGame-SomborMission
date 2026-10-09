# CHRONO EYE Android Host — preparation v0.1

2026-10-09. Candidate source module, **not an APK or completed integration**.
Android is the session owner; Web displays state; AR returns task evidence.

## Existing components inspected

- AR: `chronoeye/ar-diagnostics-20261009`, commit `bc1f9acb23e1bdcfb2da23f2a38074b7aa639226`.
  `native-prototype` uses Kotlin MainActivity, native HelloArActivity, minSdk 26,
  target/compile 35, ARCore 1.56.0, AndroidX WebKit 1.12.1.
  Current AR launcher ignores the activity result and reads `chrono_progress`.
  It cannot yet prove completion of a named mission task.
- Maps: `feature/chronoeye-web-maps-package-20261009`, commit
  `f46b1630ef8dafa7c6b064adab42a43ef2c81bdb`.
  `ChronoEyeMaps.setPlayerLocation/selectMission` and `chronoeye:map:*` events.
  Its GPS zones are display-only, snapshot is not full mission content.
- Standard: main `02a31188fc77462e972ce611a27533a6c7f1283e`, Mission Standard v0.2.

## Implemented preparation

- Java SessionEngine: session UUID, start/save/end times, pause/resume, selection,
  ordered mandatory tasks, configured awards, single mission bonus, correlated AR
  request/result, cancellation, restore/version gate and persistence-before-publication.
- Kotlin AndroidSessionStore: private AtomicFile JSON checkpoint with schema version.
- Web adapter for the existing Maps events and methods; no GPS/scoring authority.
- Dependency-free JVM regression harness. Run `sh android-host/test.sh` with JDK 11+
  and Node. Android Kotlin adapter requires the Android build and device tests.

## State and lifecycle policy

StartedAt never changes on resume. All wall-clock values are UTC epoch milliseconds;
they are audit timestamps, not a gameplay duration clock. Future elapsed gameplay
measurement must use monotonic time, explicitly excluding user pauses.
An Activity recreation uses an application-scoped controller/engine on one serial
worker; it must not reconstruct the engine on every rotation. On process recovery
the session becomes PAUSED and pending AR is invalidated, without awarding anything.
User pause cancels pending AR; Activity background due to launching AR is **not** a
user pause. It stops location/rendering only. Explicit resume follows process recovery.
Finished state is retained; starting over needs a separate archive/reset flow.
Disk errors leave in-memory state unchanged and block launch/acknowledgement.

This first engine supports an explicitly ordered list of required tasks. The content
adapter must reject optional/branching/unknown types until supported; do not flatten
the full timeline/trigger graph into this list silently. Wrong-answer penalties,
collectibles, album, dialogue/timeline, hints, GPS dwell and content loading remain
integration work. No production game package is claimed to run through this core yet.

## Integration sequence for the functional POC

1. Assemble pinned AR, Maps and canonical game-data in one review branch. Keep one
   native-prototype app/applicationId. Add Java source and Kotlin store to its source
   set; store in noBackupFilesDir. Set allowBackup=false or exclude player state.
2. Build application-scoped HostController with a serial I/O executor; activities
   observe snapshots on the main thread. Load and validate the exact manifest,
   contentVersion, task IDs, scoring rules and required task order. No demo counts.
3. Bundle maps under WebViewAssetLoader's HTTPS asset origin. Add the web adapter
   after maps.js. Install WebViewCompat.addWebMessageListener BEFORE loadUrl with
   exact origin `https://appassets.androidplatform.net`, require isMainFrame and
   WEB_MESSAGE_LISTENER capability. Reject unavailable bridge explicitly.
   Block arbitrary navigation/file/content access; no wildcard origins or remote
   pages with native bridge. Existing AR prototype's local-only loader is the base.
4. Decode bridge v1; allow only STATE_REQUEST, SESSION_START/PAUSE/RESUME,
   MISSION_SELECT and AR_REQUEST. Enforce payload size, strict types and gameId;
   return STATE/ERROR. AR_REQUEST derives task ID from native state, not Web input.
   Failed select must republish authoritative STATE so the map returns to the
   actual mission. Never expose completeValidated/arResult as JavaScript methods.
5. Native LocationManager or chosen location adapter requests foreground permission,
   validates fresh, finite, accurate fixes and emits LOCATION to Maps on UI thread.
   Stop updates offscreen; clear stale positions. Disable map browser/simulation
   controls in integrated gameplay. Native GPS engine alone checks configured zones,
   continuous monotonic dwell, stale/out-of-order/mock fixes, gap reset and re-entry;
   only then call completeValidated(..., GPS). No background permission for this POC.
6. beginAr persists Request before ActivityResultLauncher.launch. Pass sessionId,
   requestId, missionId and taskId as Intent extras. HelloArActivity must return the
   same tuple and a typed outcome: completed/cancelled/unavailable/error, plus
   task-specific evidence. Native validator translates outcome to arResult.
   Never translate prototype collected count or RESULT_OK alone into success.
   Launch failures consume request as cancelled. Unsupported/camera denied shows
   2D equivalent task; its validator uses the same correlated request and award.
7. On AR return, validate, save, then send STATE to Web; no additional scoring in
   AR/Web. Correlate diagnostic export via sessionId/requestId (no GPS history by
   default); keep existing AR diagnostic session IDs distinct.
8. Implement actual educational answer validation and completion narrative, then
   complete one canonical mission end-to-end on Galaxy S25 Ultra. Only afterwards
   extend timeline, optional collections and broader mission support.

## POC device acceptance (all pending)

- Start → native GPS → AR/2D task → quiz → map with saved score.
- Pause/resume, rotate, WebView reload, kill/relaunch; original start and awards retained.
- Duplicate/stale/wrong-session AR results, Back/camera denial, unavailable AR.
- Bad GPS/fix gaps do not complete task; map click/simulation never awards points.
- Disk failure does not launch AR or falsely report completion; corrupt state retained.
- Content upgrade blocks with actionable migration UI, not silent progress deletion.
- Offline asset loading; clear Google basemap availability status (no key configured).

## Platform references checked 2026-10-09

- https://developer.android.com/develop/ui/views/layout/webapps/native-api-access-jsbridge
- https://developer.android.com/develop/ui/views/layout/webapps/load-local-content

No map key, Google Cloud activation, APK release, existing branch merge or field test
is included in this preparation. Rollback: remove this isolated directory.
