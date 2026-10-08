using System;
using UnityEngine;

public class UnifiedGpsProvider : MonoBehaviour, IGpsProvider
{
    public event Action OnLocationChanged;

    [Header("Editor Mode")]
    [SerializeField] private EditorGpsSimulator editorGpsProvider;

    [Header("Android Runtime")]
    [SerializeField] private GeoLiteGpsProvider geoLiteProvider;

    [Header("Throttle")]
    [Tooltip("How often to emit OnLocationChanged (in seconds).")]
    public float emitInterval = 1f;

    private IGpsProvider gpsProvider;
    private float lastEmitTime = 0f;

    public bool IsGpsAvailable => gpsProvider != null && gpsProvider.IsGpsAvailable;
    public double Latitude => gpsProvider?.Latitude ?? 0;
    public double Longitude => gpsProvider?.Longitude ?? 0;
    public double Altitude => gpsProvider?.Altitude ?? 0;

    private void Awake()
    {
#if UNITY_EDITOR
        gpsProvider = editorGpsProvider as IGpsProvider;
#elif UNITY_ANDROID
        gpsProvider = geoLiteProvider as IGpsProvider;
#else
        gpsProvider = null;
#endif

        if (gpsProvider == null)
        {
            Debug.LogError("[UnifiedGpsProvider] No valid IGpsProvider assigned or active platform is unsupported.");
            return;
        }

        gpsProvider.OnLocationChanged += HandleThrottledRelay;
    }

    private void HandleThrottledRelay()
    {
        if (Time.time - lastEmitTime >= emitInterval)
        {
            lastEmitTime = Time.time;
            Debug.Log($"[UnifiedGpsProvider TimeCheck] deltaTime: {Time.deltaTime:F4} | frameRate: {(1f / Time.deltaTime):F1} | timeScale: {Time.timeScale}");
            OnLocationChanged?.Invoke();
        }
    }
}
