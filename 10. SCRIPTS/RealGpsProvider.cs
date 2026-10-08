using UnityEngine;
using System;

/// <summary>
/// Bridges AREarthTrackingManager into the IGpsProvider interface.
/// Fires a parameterless OnLocationUpdated whenever ARCore reports a new fix.
/// Consumers then pull Latitude/Longitude/Altitude from this provider.
/// </summary>
public class RealGpsProvider : IGpsProvider
{
    // 1) Declare the no-arg event exactly as the interface requires:
    public event Action OnLocationChanged;

    public RealGpsProvider()
    {
        // 2) Subscribe to ARCore’s update event:
        var arMgr = AREarthTrackingManager.Instance;
        if (arMgr != null)
        {
            Debug.Log("[RealGpsProvider] Subscribing to AREarthTrackingManager.OnLocationUpdated");
            arMgr.OnLocationChanged += HandleArLocationChanged;
        }
        else
        {
            Debug.LogWarning("[RealGpsProvider] AREarthTrackingManager.Instance is null – GPS will not update");
        }
    }

    private void HandleArLocationChanged()
    {
        // 3a) Read the new stable fix:
        var stable = AREarthTrackingManager.Instance.GetStableGPS();
        double lat = stable.Item1;
        double lon = stable.Item2;
        double alt = stable.Item3;

        // 3b) Log it for debugging:
        Debug.Log($"[RealGpsProvider] New fix: lat={lat:F6}, lon={lon:F6}, alt={alt:F2}");

        // 3c) Fire our parameterless event:
        OnLocationChanged?.Invoke();
    }

    public bool IsGpsAvailable => AREarthTrackingManager.Instance != null;
    public double Latitude => IsGpsAvailable
                                        ? AREarthTrackingManager.Instance.GetStableGPS().Item1
                                        : 0.0;
    public double Longitude => IsGpsAvailable
                                        ? AREarthTrackingManager.Instance.GetStableGPS().Item2
                                        : 0.0;
    public double Altitude => IsGpsAvailable
                                        ? AREarthTrackingManager.Instance.GetStableGPS().Item3
                                        : 0.0;
}
