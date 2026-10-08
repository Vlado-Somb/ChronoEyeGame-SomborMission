using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using TMPro;

public class DebugLogUI : MonoBehaviour
{
    [Tooltip("TextMeshProUGUI component inside the scroll view")]
    [SerializeField]
    private TMP_Text logText;

    [SerializeField]
    [Tooltip("Maximum number of lines in the on-screen log. Set to 0 for unlimited.")]
    private int maxLines = 0;  // 0 means no line limit

    private Queue<string> lines = new Queue<string>();

    // ====== UI LOGGING ======
    void OnEnable()
    {
#pragma warning disable UDR0005 // Domain Reload Analyzer
        Application.logMessageReceived += HandleLog;      // updates on-screen
#pragma warning restore UDR0005 // Domain Reload Analyzer
#pragma warning disable UDR0005 // Domain Reload Analyzer
        Application.logMessageReceived += HandleLogSave;  // buffers into 'sb'
#pragma warning restore UDR0005 // Domain Reload Analyzer
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        Application.logMessageReceived -= HandleLogSave;
    }

    void HandleLog(string condition, string stackTrace, LogType type)
    {
        // remove any stray ✗ (U+2717) before printing
        var sanitized = condition.Replace("\u2717", string.Empty);
        EnqueueLine($"[{type}] {sanitized}");
    }

    void EnqueueLine(string msg)
    {
        if (maxLines > 0 && lines.Count >= maxLines)
            lines.Dequeue();
        lines.Enqueue(msg);
        logText.text = string.Join("\n", lines);
    }

    public void AddLine(string msg) => EnqueueLine(msg);

    // ====== BUFFERED LOGGING ======
    private StringBuilder sb = new StringBuilder();

    void HandleLogSave(string logString, string stackTrace, LogType type)
    {
        // strip out ✗ so it never enters our saved session log
        var clean = logString.Replace("\u2717", string.Empty);
        sb.AppendLine($"[{DateTime.Now:HH:mm:ss}] [{type}] {clean}");
    }

    public void SaveLogToFile()
    {
        try
        {
            string logFolder = Application.persistentDataPath;
            string fileName = "session_log_ARTeslaGame" + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".txt";
            string fullPath = Path.Combine(logFolder, fileName);

            Directory.CreateDirectory(logFolder);
            File.WriteAllText(fullPath, sb.ToString());
            Debug.Log($"[DebugLogUI] Saved log to: {fullPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DebugLogUI] Failed to save log: {ex}");
        }
    }

    // Ensure the file gets written on mobile (OnApplicationQuit may not run on Android)
    void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveLogToFile();
    }

    void OnApplicationQuit()
    {
        SaveLogToFile();
    }

    // Optional: show where persistentDataPath is
    void Start()
    {
        Debug.Log($"[DebugLogUI] persistentDataPath = {Application.persistentDataPath}");
    }
}
