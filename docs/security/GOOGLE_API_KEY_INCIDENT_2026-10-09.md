# CHRONO EYE — Google API key exposure (2026-10-09)

**Incident status:** SOURCE PATCH PREPARED; CLOUD ROTATION / REVOCATION NOT VERIFIED. This is an incident record, not evidence of abuse.

## Confirmed findings

- GitGuardian notified of a Google API key found in public repository `Vlado-Somb/ChronoEyeGame-SomborMission`, alert referencing a push on **2026-10-09 18:23:17 UTC**.
- Inspection of `main` revealed **two distinct Google API key values** in **six tracked text files**.
- One value occurs in `ProjectSettings/ARCoreExtensionsProjectSettings.json` and four Unity scene/prefab files; the other occurs in one Unity scene.
- The Unity scene/prefab references call `https://tile.googleapis.com/v1/3dtiles/root.json`. The ProjectSettings value is used by legacy ARCore Extensions.
- We have **not** established that either key was unrestricted, active or abused. We have **not** accessed Google Cloud Console.
- The repository also contains legacy build artifacts. Binaries, Git history and other branches require separate inspection; a clean current-source scan is **not** proof that historic keys disappeared.

## Immediate actions — account owner (Google Cloud Console)

1. Go to https://console.cloud.google.com/apis/credentials , identify **both** affected key values privately. **Do not paste them into issues, chat, screenshots, or PR descriptions.**
2. **Promptly revoke/delete or regenerate both exposed keys.** If service continuity matters, first apply the strictest compatible API/application restrictions as immediate containment, then rotate. Treat publicly committed old values as compromised even if GitGuardian detects only one.
3. Review **Metrics/Monitoring, API usage, quotas and Cloud Billing** around and after exposure; investigate unexplained traffic/charges, especially the **Map Tiles API** and **ARCore API**.
4. Introduce **separate credentials and restrictions** for each client/service/environment. Prefer Android **keyless OAuth** for supported ARCore usage (package name + SHA-1). For Maps web use proper HTTPS referrer restrictions; for native Maps Android use package name + SHA-1. The **Map Tiles REST API** may require a controlled proxy or compatible request restrictions; **verify** rejected requests from unauthorized origins before deployment.
5. Record non-secret Cloud Project ID, enabled API names, approved restrictions, quotas, cost alert and the result of tests in the service registry. Never commit real key values.
6. Rebuild and test old Unity features and new Android/Web maps only after revised authentication is provisioned.

## Repository mitigation in this branch

- Replaced committed Unity Map Tiles key values with `CONFIGURE_RESTRICTED_MAP_TILES_API_KEY`.
- Cleared `AndroidCloudServicesApiKey` in legacy Unity project settings. This intentionally prevents working API-key authentication without subsequent provisioning.
- Added a present-tree text-key scan and ignore patterns for local credentials/build artifacts.
- Did **not** rewrite Git history, overwrite working AR branches, change Cloud IAM/billing, change the actual APK, or validate Unity runtime.
- Existing checked-in binary files and older commits **can retain old values**. Rotate/revoke the keys regardless of the code patch. History rewriting, if later needed, should be a separate coordinated operation after auditing forks and branch consumers.

## Validation criteria

- [x] Confirmed two distinct API keys in six text source files on `main`.
- [x] Source replacements staged in the isolated security-fix branch.
- [ ] PR reviewed and merged into `main`.
- [ ] Both original keys revoked/rotated in Google Cloud (account owner confirmation).
- [ ] Cloud usage/charges checked.
- [ ] Historical refs/other branches and build artifacts assessed.
- [ ] New restricted auth configured safely, Android/Web/AR regression verified.
- [ ] Security incident closed only when all applicable items are complete.

References:
- https://developers.google.com/maps/api-security-best-practices
- https://developers.google.com/maps/documentation/tile/get-api-key
- https://developers.google.com/ar/develop/authorization?platform=android


## Follow-up audit — 2026-10-09

- PR #19 head reviewed: `61af2fe82b186be182f94eb80e4ea37e9bbe6516`; main: `91cd59d689cac63395cebbc4087df6377ccf26e9`. PR was open/draft, mergeable; Google source CI run `37987361296` passed. This was not a complete secret or binary audit.
- Targeted comparison covered the six known Google-bearing paths plus two Cesium settings paths across 28 fetched remote branch heads. The same two Google values remain in all six paths on the other 27 heads, including main. The eight-path audit does not certify unrelated files.
- Two distinct Cesium ion JWT values were found in three locations: `Assets/CesiumSettings/Resources/CesiumIonServers/ion.cesium.com.asset`, `Assets/CesiumSettings/Resources/CesiumRuntimeSettings.asset`, and `Assets/MATERIJALI/6. Scene Acts/3_ACT0_Embassy 1.unity`. Owner explicitly authorized clearing these legacy credentials. Their validity/scopes were not tested against Cesium. Provider-side revocation remains necessary if active.
- Source scanner now checks JWT/private-key patterns as well as Google keys, including extensionless files, backups and large text files. Its output redacts values. It explicitly reports uninspected binary/archive/missing files. It does not prove removal from history or compressed APK content.
- Historical exposure remains confirmed by reachable main/source blob versions; no force-push or history rewrite performed. Full-history and all-binary clearance is **not claimed**.
- A checked-in legacy APK, `SOMBOR TEST 31.10.e.apk` (76,736,664 bytes in the tree), remains present. Latest AR Lab workflow `37985787225` succeeded and advertises artifact `11642539540` (5,330,345-byte ZIP). An artifact URL was returned, but byte download failed; that artifact's embedded secrets and signing certificate are not yet verified.
- Cloud Browser returned `Site Unavailable — Unable to access this site` at console.cloud.google.com; user's separately logged-in browser tab was not exposed to this session. No Google/Cesium credential was revoked, rotated or created; no API, billing, IAM or quota was changed. Abuse and charges cannot be assessed without console access.
- Keep incident open. Repository containment and Cloud recovery are separate gates.

### Legacy APK inspection and source validation

- Downloaded the tracked legacy APK (76,736,664 bytes), SHA-256 `b115ba9abb92492c253c2a8a65a8fec69f677e8d2753f168d338cb8f3461f930`. A byte-pattern scan of every decompressed ZIP entry found one JWT in `assets/bin/Data/data.unity3d`; no literal Google-key match in that scan. This does not clear nested Unity compression/serialized data or obfuscated credentials.
- Remove this obsolete checked-in APK from the updated PR tree, preserving history and all source assets. Installed apps, Cloud projects and OAuth clients are unaffected by this repository-file removal. Historical copies still require token revocation and coordinated distribution cleanup.
- Scanner fixture checks passed: clean text, extensionless Google key, backup JWT, >8 MiB text key, PEM private key; matching values are not emitted. Full source CI on the updated remote commit is a separate gate. No Android/Unity runtime build was run here.

### Expanded CI finding

Full-checkout source CI run `37989345627` inspected 1,774 text files and correctly failed on `UserSettings/CesiumIonServerManager.asset`; 178 binary/archive files were excluded. This revealed a third distinct Cesium JWT (four source occurrences total, superseding the earlier two-token count). Remove this tracked editor-session file from the PR; UserSettings is already ignored for future additions. All three Cesium values require provider-side scope/revocation review. No claim is made that the JWT in the legacy APK is the only credential within nested Unity content.
