# CHRONO EYE SOMBOR — GK QA / Acceptance Checklist

## Content
- [ ] Original historical narrative is retained as legacy; the edited narrative describes Branković (1718), Magistrate (1749), present-day facade year (1842).
- [ ] Quiz `GK-02` still has answers 1718 / 1749 / 1842 and `correctIndex: 2`.
- [ ] Ćelavi trg's former Trinity monument (1774–1947) is additional context, not an invented existing structure.
- [ ] Verify inscription visibility on the device and correct surface from a safe pedestrian viewpoint.

## Gameplay
- [ ] `GK-01` proximity approach confirmation works without forcing a 7 m GPS circle.
- [ ] On GPS approach, AR opens only when the player is stationary and explicitly starts it.
- [ ] Professor Vlada shows subtitles, optional voice and can be summoned again.
- [ ] `GK-AR-01` tower outline can be manually aligned and tapped; tap must not be mistaken for confirmed automatic object recognition.
- [ ] AR fallback 2D photograph has tower/numeric-inscription hotspots; same rewards, no penalty.
- [ ] Historical quiz follows AR and cannot be skipped by collecting bonus coins.
- [ ] Completed task order is GPS → AR → quiz; main token only after completion.

## Characters and assets
- [x] Original SVG placeholder assets exist for tower outline, reticle, mentor portrait and token categories.
- [ ] 3D Professor Vlada model and idle / point / talk / dematerialize animations.
- [ ] Camera-correct tower detection or guided alignment strategy field-tested.
- [ ] Audio VO recorded, permission and app user gesture handled; volume mute and subtitles.
- [ ] 2D fallback image rights/source and crop verified; do not copy unlicensed online image into assets.

## Collectibles and UI
- [ ] Unique tower relic unlocks in Herbarijum exactly once.
- [ ] Golden seal pickup gives +2 only once per spawn key and respects daily cap.
- [ ] Heart and joker are listed in the album but not falsely awarded in GK.
- [ ] Per-item explanatory story text is visible; historical facts have a source.
- [ ] Reopening the game does not re-award points.

## Field test
- [ ] Daylight + low light, standing on safe parts of Ćelavi trg, camera panning.
- [ ] AR unsupported/denied permission and poor GPS fallback flows.
- [ ] Mentor doesn't stand in roadway or float in air without user placement.
- [ ] Native Android back gesture and app resume do not lose state.
- [ ] Public map polygons and exact coordinates checked separately from game narrative.

**QA status:** specification and static data, NOT field-tested / NOT implemented in Android.
