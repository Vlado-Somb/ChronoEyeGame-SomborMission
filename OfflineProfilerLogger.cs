using UnityEngine;
using UnityEngine.Profiling;
using System.IO;

public class OfflineProfilerLogger : MonoBehaviour
{
    [Header("Profiler Logging")]
    public string customFileName = "walk_session"; // You can change this per run
    public bool startOnAwake = true;

    private string _profileFilePath;

    void Awake()
    {
        if (!startOnAwake) return;

        StartProfilerLogging();
    }

    public void StartProfilerLogging()
    {
        string dir = Path.Combine(Application.persistentDataPath, "ProfilerLogs");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _profileFilePath = Path.Combine(dir, customFileName + "_" + timestamp + ".raw");

        Profiler.logFile = _profileFilePath;
        Profiler.enableBinaryLog = true;
        Profiler.enabled = true;

        Debug.Log($"[OfflineProfilerLogger] 📊 Profiler started → {_profileFilePath}");
    }

    public void StopProfilerLogging()
    {
        Profiler.enabled = false;
        Debug.Log("[OfflineProfilerLogger] 🛑 Profiler stopped");
    }
}
