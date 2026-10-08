using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class EditorGpsSimulator : MonoBehaviour, IGpsProvider
{
    [Header("Simulated GPS")]
    public double SimLatitude;
    public double SimLongitude;
    public double SimAltitude;

    // now matches the interface: no-arg Action
    public event Action OnLocationChanged;

    private double _lastLat, _lastLon, _lastAlt;

    void Awake()
    {
        // 1) initial dispatch
        Debug.Log($"[EditorGpsSimulator] Awake - initial fix = ({SimLatitude}, {SimLongitude}, {SimAltitude})");
        OnLocationChanged?.Invoke();
        _lastLat = SimLatitude;
        _lastLon = SimLongitude;
        _lastAlt = SimAltitude;
        Debug.Log("[EditorGpsSimulator] Awake: OnLocationChanged invoked");
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // 2) inspector tweaks
        Debug.Log($"[EditorGpsSimulator] OnValidate - fix = ({SimLatitude}, {SimLongitude}, {SimAltitude})");
        OnLocationChanged?.Invoke();
        Debug.Log("[EditorGpsSimulator] OnValidate: OnLocationChanged invoked");
    }

    public void TriggerUpdate()
    {
        // optional manual trigger
        Debug.Log($"[EditorGpsSimulator] TriggerUpdate - fix = ({SimLatitude}, {SimLongitude}, {SimAltitude})");
        OnLocationChanged?.Invoke();
        Debug.Log("[EditorGpsSimulator] TriggerUpdate: OnLocationChanged invoked");
    }
#endif

    void Update()
    {
        // 3) automatic on-change detection
        if (!Mathf.Approximately((float)_lastLat, (float)SimLatitude) ||
            !Mathf.Approximately((float)_lastLon, (float)SimLongitude) ||
            !Mathf.Approximately((float)_lastAlt, (float)SimAltitude))
        {
            _lastLat = SimLatitude;
            _lastLon = SimLongitude;
            _lastAlt = SimAltitude;

            Debug.Log($"[EditorGpsSimulator] Update - new fix = ({SimLatitude}, {SimLongitude}, {SimAltitude})");
            OnLocationChanged?.Invoke();
            Debug.Log("[EditorGpsSimulator] Update: OnLocationChanged invoked");
        }
    }

    // IGpsProvider implementation
    public bool IsGpsAvailable => true;
    public double Latitude => SimLatitude;
    public double Longitude => SimLongitude;
    public double Altitude => SimAltitude;
}
