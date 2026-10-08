using UnityEngine;
using Google.XR.ARCoreExtensions;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using System;

public class AREarthTrackingManager : MonoBehaviour
{
    public static AREarthTrackingManager Instance { get; private set; }

    [Header("AR Components")]
    [SerializeField] private ARSession arSession;
    [SerializeField] private ARCoreExtensions arCore;
    [SerializeField] private AREarthManager earthManager;
    [SerializeField] private XROrigin xrOrigin;

    private Vector3 stablePosition;
    private double latitude, longitude, altitude;
    private float stableLatitude, stableLongitude, stableAltitude;

    private float updateThreshold = 1.5f;
    private float smoothingFactor = 0.1f;
    public event Action OnLocationChanged;

    private IEnumerator Start()
    {
        Instance = this;

        StartCoroutine(CheckVPSAvailability());

        // Make sure all AR components are assigned manually
        if (xrOrigin == null) xrOrigin = FindFirstObjectByType<XROrigin>();
        if (earthManager == null) earthManager = FindFirstObjectByType<AREarthManager>();
        if (arSession == null) arSession = FindFirstObjectByType<ARSession>();
        if (arCore == null) arCore = FindFirstObjectByType<ARCoreExtensions>();

        // Disable AR until ready
        xrOrigin.gameObject.SetActive(false);
        arSession.gameObject.SetActive(false);
        arCore.gameObject.SetActive(false);

        // Wait for readiness
        yield return new WaitForSeconds(1.0f);

        xrOrigin.gameObject.SetActive(true);
        arSession.gameObject.SetActive(true);
        arCore.gameObject.SetActive(true);

        Debug.Log("[AREarthTrackingManager] AR components enabled.");

        // Wait for Earth to initialize
        while (earthManager == null ||
               ARSession.state != ARSessionState.SessionTracking ||
               earthManager.EarthState == EarthState.ErrorInternal)
        {
            Debug.Log("[AREarthTrackingManager] Waiting for AR Session to become stable.");
            yield return new WaitForSeconds(0.2f);
            earthManager = FindFirstObjectByType<AREarthManager>();
        }

        Debug.Log("[AREarthTrackingManager] ARCore Earth Manager is fully initialized.");
    }

    private void Update()
    {
        if (earthManager == null ||
            ARSession.state != ARSessionState.SessionTracking ||
            earthManager.EarthState != EarthState.Enabled ||
            earthManager.EarthTrackingState != TrackingState.Tracking)
        {
            return;
        }

        GeospatialPose pose = earthManager.CameraGeospatialPose;
        latitude = pose.Latitude;
        longitude = pose.Longitude;
        altitude = pose.Altitude;

        float distance = HaversineDistance(stableLatitude, stableLongitude, (float)latitude, (float)longitude);
        if (distance > updateThreshold)
        {
            stableLatitude = Mathf.Lerp(stableLatitude, (float)latitude, smoothingFactor);
            stableLongitude = Mathf.Lerp(stableLongitude, (float)longitude, smoothingFactor);
            stableAltitude = Mathf.Lerp(stableAltitude, (float)altitude, smoothingFactor);

            stablePosition = xrOrigin.transform.TransformPoint(new Vector3((float)pose.Latitude, (float)pose.Longitude, (float)pose.Altitude));
            Debug.Log($"[AREarthTrackingManager] Stable GPS Updated: {stableLatitude}, {stableLongitude}, {stableAltitude}");
        }
        Debug.Log("[AREarthTrackingManager] firing OnLocationChanged");
        OnLocationChanged?.Invoke();
    }

    public Vector3 GetStableWorldPosition() => stablePosition;

    public (float, float, float) GetStableGPS() => (stableLatitude, stableLongitude, stableAltitude);

    private float HaversineDistance(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371000f;
        float dLat = Mathf.Deg2Rad * (lat2 - lat1);
        float dLon = Mathf.Deg2Rad * (lon2 - lon1);
        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                   Mathf.Cos(Mathf.Deg2Rad * lat1) * Mathf.Cos(Mathf.Deg2Rad * lat2) *
                   Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return R * c;
    }

    private IEnumerator CheckVPSAvailability()
    {
        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarning("Location services not running. Cannot check VPS.");
            yield break;
        }

        var location = Input.location.lastData;
        var vpsAvailabilityPromise = AREarthManager.CheckVpsAvailabilityAsync(location.latitude, location.longitude);
        yield return vpsAvailabilityPromise;

        Debug.Log($"VPS Availability at ({location.latitude}, {location.longitude}): {vpsAvailabilityPromise.Result}");

        if (vpsAvailabilityPromise.Result != VpsAvailability.Available)
        {
            Debug.LogWarning("VPS is not available — using GPS-only mode. AR object placement may be less accurate.");
        }
    }
}
