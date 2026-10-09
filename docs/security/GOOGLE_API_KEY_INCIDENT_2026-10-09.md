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
