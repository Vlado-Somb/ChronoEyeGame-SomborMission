#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class AudioDiagnosticsEditor
{
    [MenuItem("Tools/Run Audio Diagnostics")]
    public static void RunDiagnostics()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("<color=yellow>[AudioDiagnostics]</color> Editor mode: results may not reflect runtime behavior.");
        }

        AudioDiagnostics.RunDiagnosticsStatic();
    }
}
#endif
