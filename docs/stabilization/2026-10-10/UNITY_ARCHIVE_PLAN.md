# Unity archive preparation — 2026-10-10

Status: PREPARED, NOT CREATED, NOT COMPLETE. No Unity source or binary was deleted.

Owner-created destination: `Vlado-Somb/ChronoEye-Legacy`, verified **private** through the GitHub connector on 2026-10-10 (repository ID 1413515532). Initial reported size: 0. No archive transfer or completeness is claimed. Do not activate copied workflows, Pages or deployment. Never make the archive public.

## Preservation scope

The inventory records all 36 observed branch heads and three distinct legacy trees. Main contains 1,855 tracked legacy entries; all heads together reference 1,837 distinct legacy blob IDs. Preserve `Assets` including `.meta` GUIDs, `Packages`, `ProjectSettings`, legacy user settings, logs, temporary/build material and the tracked APK. Preserve PR #1's alternative repaired Unity tree separately; it is not in main. Preserve the original and security-redacted states without conflating them.

Use a full-history private mirror as the initial preservation archive. This retains incidental modern content in historical commits; its purpose is recovery, not a second active app repository. A Unity-only filtered derivative can be made later, but must not replace the complete preservation copy. Do not mirror-push into any existing repository or configure ongoing mirroring.

## Required procedure before removal from the active repository

1. Refresh source refs and update the inventory if any changed. Capture tags and annotated tag objects too (no tags observed in this snapshot).
2. Create the private destination and independently verify its visibility and owner. Disable automated execution there; do not activate copied workflows.
3. Obtain all Git objects for all inventoried heads/history. A sparse/partial clone is NOT a complete archive. Inspect `.gitattributes` for LFS and tree entries for submodules; fetch and inventory any externally referenced objects separately. Preserve license, Unity/package versions and dependency metadata.
4. Transfer only to the verified private destination. Historical credential values must never be copied into a report, log, public patch or public artifact.
5. Make a second, fresh full clone from the destination. Run `git fsck --full`; verify all branch heads, tags, trees, blob bytes, file modes and .meta files against `unity-source-inventory.json`. Run `verify_unity_archive.py` on that clone. Compare any LFS/submodule external inventory separately.
6. Record destination URL, visibility evidence, source snapshot time, verifier result, object counts and a recovery test. An inventory or successful upload is not proof of recoverability. Compilation is a separate check and is not claimed.
7. Obtain the owner's explicit approval for a **separate removal PR**. Review references first: PR #17's historical FBX inspector reads legacy Assets and must be supplied a preserved model dependency before removal. Remove neither source nor tracked APK/user settings in this phase.

Security: PR #19's source scan does not inspect historical Git objects or APK contents. Owner-reported key deletion is not independent provider verification. Private archival does not revoke credentials or remove public history. No Cloud/Cesium operation or history rewrite is part of this plan.
