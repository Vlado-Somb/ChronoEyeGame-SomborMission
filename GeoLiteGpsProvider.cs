using System;
using UnityEngine;
using Google.XR.ARCoreExtensions.Samples.Geospatial;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

public class GeoLiteGpsProvider : MonoBehaviour, IGpsProvider
{
    public event Action OnLocationChanged;

    [Header("Smoothing")]
    public bool useSmoothing = true;
    public float smoothFactor = 0.1f;

#if UNITY_EDITOR
    [Header("Editor Simulation")]
    [Tooltip("Simulate GPS readiness when playing in Editor.")]
    public bool simulateGpsReadyInEditor = false;
#endif

    private double _lastLat;
    private double _lastLon;
    private double _lastAlt;
    private bool _initialized;

    public bool IsGpsAvailable
    {
        get
        {
#if UNITY_EDITOR
            return simulateGpsReadyInEditor;
#else
            return GeoLite.Instance != null && GeoLite.Instance.IsReady;
#endif
        }
    }

    public double Latitude => _lastLat;
    public double Longitude => _lastLon;
    public double Altitude => _lastAlt;

    private void Update()
    {
#if UNITY_EDITOR
        return;
#else
        if (!IsGpsAvailable) return;

        var pose = GeoLite.Instance.CurrentPose;

        if (!_initialized)
        {
            _lastLat = pose.Latitude;
            _lastLon = pose.Longitude;
            _lastAlt = pose.Altitude;
            _initialized = true;
            Debug.Log("[GeoLiteGpsProvider] First GPS fix initialized.");
            OnLocationChanged?.Invoke();
            return;
        }

        double lat = useSmoothing ? Mathf.Lerp((float)_lastLat, (float)pose.Latitude, smoothFactor) : pose.Latitude;
        double lon = useSmoothing ? Mathf.Lerp((float)_lastLon, (float)pose.Longitude, smoothFactor) : pose.Longitude;
        double alt = useSmoothing ? Mathf.Lerp((float)_lastAlt, (float)pose.Altitude, smoothFactor) : pose.Altitude;

        if (Math.Abs(lat - _lastLat) > 0.00001 || Math.Abs(lon - _lastLon) > 0.00001 || Math.Abs(alt - _lastAlt) > 0.01)
        {
            _lastLat = lat;
            _lastLon = lon;
            _lastAlt = alt;
            OnLocationChanged?.Invoke();
        }
#endif
    }

    public (double lat, double lon, double alt) GetStableGps()
    {
        return (_lastLat, _lastLon, _lastAlt);
    }

    public IEnumerator CheckVpsAvailability(Action<VpsAvailability> callback)
    {
#if UNITY_EDITOR
        Debug.Log("[GeoLiteGpsProvider] Simulating VPS Unavailable in Editor.");
        callback?.Invoke(VpsAvailability.Unavailable);
        yield break;
#else
        if (GeoLite.Instance == null || ARSession.state != ARSessionState.SessionTracking)
        {
            Debug.LogWarning("[GeoLiteGpsProvider] Cannot check VPS: not tracking or GeoLite missing.");
            callback?.Invoke(VpsAvailability.Unavailable);
            yield break;
        }

        var pose = GeoLite.Instance.CurrentPose;
        Debug.Log($"[GeoLiteGpsProvider] Checking VPS at lat={pose.Latitude}, lon={pose.Longitude}");

        var promise = AREarthManager.CheckVpsAvailabilityAsync(pose.Latitude, pose.Longitude);
        yield return promise;

        Debug.Log($"[GeoLiteGpsProvider] VPS availability result: {promise.Result}");
        callback?.Invoke(promise.Result);
#endif
    }
}
