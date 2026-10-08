# CHRONO EYE — Sombor: resource stabilization (2026-10-08)

This branch deliberately preserves existing Unity .asset, .unity, .prefab, UXML, USS and C# paths and .meta GUIDs. Replacing them with differently named assets would break Unity serialized references. The clean replacements exist **on this branch only**; main is untouched.

## Conflict-resolution decisions

- Resolved all 19 Git merge-conflicted files found with `Updated upstream` / `Stashed changes` markers.
- Retained the 26-block Sombor Timeline Block Database instead of the unrelated 8-block stashed list. Existing trigger block indices therefore still refer to the 26-block version.
- Kept the expanded upstream Unity scene, UI prefab, C# runtime/editor logic and UXML layout. Kept the functional HOLD_THRESHOLD constant where needed.
- Waypoint database: selected the narrower stashed test coordinates/radii as the provisional baseline, **except** for Gradski muzej (SO_GMS) where the stashed position accidentally duplicated the Muzej Podunavskih Švaba coordinates. For SO_GMS, retained the other available candidate coordinates. These coordinates have not been field-verified.
- Retained BOTH distinct Sombor waypoints `SO_VP` (Veljko Petrović) and `SO_SČ` (Srpska čitaonica), plus `SO_PED`, `SO_SVĐ`, `SO_ŽUP` and `SO_GLOBAL`.
- Kept `SO_GLOBAL` because the current Sombor GAME Waypoint Trigger Binding references it.
- All 9 mission assets and their 26 tasks remain. Reset 5 persisted test-only `isCompleted` flags in two mission assets; MissionManager also resets these at mission start.
- Removed a non-existent `NextBlockId` timeline request: the mission block already signals timeline advancement through its completion callback.
- Closed an otherwise unterminated `StartGpsTask` C# method. Removed a UnityEditor import from runtime UI code, corrected HideAllTaskUI completion panel, and propagated individual waypoint confirmation delays into WaypointRuntime.

## Provisional waypoint radii

Each waypoint has three concentric GPS zones (L1 outer, L2 approach, L3 close). Examples:

- SO_GK (Gradska kuća): 75 / 21 / 7 m.
- SO_MKG (Galerija Milan Konjović): 50 / 15 / 7 m.
- Other point locations commonly use 50 / 12 / 5–7 m, with site-specific differences.

These are **not calibrated accuracy guarantees**, particularly the 5–7 m inner radii. A future onsite pass should set outer activation radii according to visibility/accessibility and adjust confirmation delays based on device GPS performance. Latitude/longitude geofencing here is two-dimensional; Unity AR altitude/geoid handling is a separate issue.

## Static checks executed on the branch

- 19/19 formerly conflicting files: no remaining Git conflict markers.
- 13 waypoint IDs, with no duplicates in the Sombor database.
- 26 timeline block references, 10 Sombor GAME trigger bindings; all trigger block indices were inside the 26-block range, and binding waypoint IDs exist.
- 9 mission resources, 26 tasks; all GPS task waypoint IDs exist, quiz answer indices were in range, text-input tasks have expected answers, and no `isCompleted: 1` remain.
- MissionTaskUIController and ScoreUIController UXML `name` selectors match their corresponding layout elements.
- A lightweight source delimiter balance check was performed; this is **not a Unity compiler or runtime test**.

## Deliberately NOT changed

- Historical facts, currently shown exhibitions, question texts, answers and editorial prose are preserved for the separate content review.
- Most non-GPS tasks still contain unused copied `SO_GK` waypoint fields. Their runtime task type does not use this field; an eventual schema cleanup can remove those misleading values after confirming design conventions.
- Player progress persistence, GPS error handling, acceptance of Cyrillic/Latin and variant answers, camera-based AR and Google Maps/My Maps coordinates are outside this stabilization pass.

## Required before merging into main

1. Open branch as a Unity project and wait for import to finish; check Console compiler/errors.
2. Run Sombor scene and verify introductory dialogue → first mission → question → completion → next timeline block.
3. Check score UI and flexible ABC choices, especially 3–4 answer layouts.
4. Check SO_VP and SO_SČ markers exist and do not overlap unexpectedly.
5. Test geofence activation in Sombor on Android before declaring the radii final.
6. Only after these pass, merge into main. This branch is **not** a validated APK build.
