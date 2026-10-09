# CHRONO EYE — Cloud recovery candidate

2026-10-09. **Status: source containment prepared; Cloud access blocked.**
This is a reviewable proposal, not a deployed configuration. Actual project IDs, billing account linkage, enabled services, quotas, OAuth clients, signing fingerprints and domains are unknown until read from the owner's console.

## Inventory to complete in the console

For each accessible project record its ID/number, purpose (legacy/new/unrelated), enabled APIs, credential resource names and types (never values), application/API restrictions, Android package + certificate pairing, last observed usage, billing linkage and cost trend. Inspect API metrics by credential/service where available for at least 30 days and around the reported 2026-10-09 18:23:17 UTC push; expand the interval if source history shows earlier exposure. A key's presence in Git does not prove it is valid or abused. Missing telemetry cannot establish absence of abuse.

Privately match both exposed values to credential resources. Preserve non-secret evidence of restrictions and usage. The shared Google value serves legacy ARCore and four Map Tiles scene/prefab references; the second serves a separate Map Tiles scene. Check for consumers outside this repository. The owner accepts interruption of the legacy Unity/Cesium project; do not infer permission to interrupt unrelated applications. Revoke exposed Google values after dependency verification; coordinate restricted replacements only for confirmed active consumers. Also revoke the three exposed Cesium values at Cesium if active; clearing Git does not revoke them.

## Minimal new configuration (not activated)

| Component | Candidate authorization | Readiness gate |
|---|---|---|
| Local native AR/Depth | ARCore SDK; no Maps key needed for local placement/depth | Existing device regression test |
| Android Geospatial/VPS | ARCore API with Android Keyless OAuth client for each actual package/SHA-1 pair | Approved project; INTERNET/location permissions; auth dependency; VPS/device/denied-auth tests |
| Web Maps | Separate Maps JavaScript API key restricted to that API and approved exact HTTPS origins/referrers | Real deployed domain and WebView request-origin test |
| Maps SDK for Android | Only if deliberately chosen instead of Web Maps; separate API-limited package/SHA-1 key | Do not activate both map renderers speculatively |
| Map Tiles | No new activation for current 2D Web Maps + local AR POC | Separate 3D requirement, licensing and cost approval |
| Places/Routes/Geocoding/Roads | No new activation | Separate feature decision |

Android package verified in AR Lab source: `rs.chronoeye.prototype`; debug/release/Play signing SHA-1 still unverified. The Kotlin/Java namespace is not the application ID. CI debug certificates can differ between builds, so first establish a stable approved signing identity, then register each actual signer. Do not embed a service-account private key in Android or Web.

For Geospatial, add the required Android authorization dependencies/configuration in a separate integration change; selecting Keyless in a design document does not implement it. Google's Android guide requires OAuth Android client IDs tied to the package/signing fingerprint. Web Maps uses a different authorization model; do not reuse a native credential.

The Host proposal uses the local HTTPS asset origin `https://appassets.androidplatform.net`. That origin is not proof of ownership of a public domain, nor proof that Google receives an acceptable Referer. Verify actual requests and unauthorized-origin rejection before enabling the basemap. Preserve the local bridge's strict origin/main-frame checks; do not weaken key restrictions or expose the native bridge to arbitrary remote pages.

Keep legacy infrastructure isolated from the new development configuration. Choose a new development project only after inventory; defer a production project until release. No existing project, OAuth client or active app is scheduled for deletion.

## Spending proposal — approval required before application

- Start with **5,000 HUF per month** as the development planning budget, subject to the actual billing currency and observed API costs. This is not a forecast or permission to spend. Agree the exact billing-currency amount before applying it.
- Alerts-only budget: actual-spend thresholds 50%, 80%, 100%, plus forecast at 100% if offered. These notify; they do **not** stop API calls or guarantee a maximum invoice. Confirm recipients in the existing billing UI.
- After inventory, propose service-specific daily/rate quotas based on a small pilot (e.g. 50 map initializations/day as an initial demand target). Verify which quotas are enforceable for the exact API/SKU; a UI target is not automatically a quota, and request count is not necessarily the billable unit. No numerical Cloud quota is claimed configured.
- Keep Map Tiles out of the new POC until specifically required. Use separate credentials per client/environment and an application-side basemap off switch with coordinate-only fallback. Client-side controls alone cannot stop misuse of a copied key.
- Current Google documentation lists spend-cap budgets for selected services, but does not list Maps/ARCore among eligible services. Do not promise a Maps hard spending cap. Even supported caps have enforcement delay and can incur overages.
- API quota rejection can constrain request rate/volume, not guarantee a universal monthly currency ceiling. Avoid automatic whole-project billing disconnection without a separate approved impact and recovery plan.

## Verification / merge gates

1. Finish provider-side revocation and verify both old values no longer authorize relevant calls using a controlled test that does not log credentials. Record resource IDs/status, not values.
2. Review the updated PR #19 diff and run the expanded source CI. Clear legacy source credentials; do not restore them on rollback.
3. Inspect retained APKs and Actions artifacts, including decompressed entries and actual signing certificates. Rebuild any affected distribution after remediation. No binary is certified by a clean source scan.
4. Bring each active development branch forward with the security correction and test the resulting merge tree; avoid blindly merging unrelated content branches.
5. Test authorized and rejected origins/packages, quota-exceeded behavior, offline map fallback, AR session lifecycle and Geospatial unavailable/denied cases on the actual device. Preserve existing session state.
6. Only then change service statuses to configured/verified and close the incident.

## Official references checked

- https://developers.google.com/ar/develop/authorization?platform=android
- https://developers.google.com/ar/develop/java/geospatial/enable
- https://developers.google.com/maps/api-security-best-practices
- https://developers.google.com/maps/documentation/tile/usage-and-billing
- https://docs.cloud.google.com/billing/docs/how-to/budgets
- https://docs.cloud.google.com/billing/docs/how-to/budgets-spend-caps
