using UnityEngine;

public class AudioDiagnostics : MonoBehaviour
{
    void Start()
    {
        Debug.Log("<color=cyan>[AudioDiagnostics]</color> Starting audio system check...");

        // === AUDIO LISTENER CHECK ===
        var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length == 0)
            Debug.LogWarning("[AudioDiagnostics] ❌ No AudioListener found in scene!");
        else if (listeners.Length > 1)
            Debug.LogWarning($"[AudioDiagnostics] ⚠️ Multiple AudioListeners detected ({listeners.Length}). This may cause issues.");
        else
            Debug.Log("[AudioDiagnostics] ✅ Single AudioListener detected.");

        // === HINT UI CONTROLLER ===
        var hint = FindFirstObjectByType<HintUIController>();
        CheckAudioSource(hint, "HintUIController");

        // === SCORE UI CONTROLLER ===
        var score = FindFirstObjectByType<ScoreUIController>();
        CheckAudioSource(score, "ScoreUIController");

        // === DIALOGUE MANAGER ===
        var dialogue = FindFirstObjectByType<DialogueManager>();
        CheckAudioSource(dialogue, "DialogueManager");

        // === BACKGROUND MUSIC ===
        var bgm = FindFirstObjectByType<BackgroundMusicManager>();
        if (bgm != null)
        {
            var sources = bgm.GetComponents<AudioSource>();
            if (sources.Length >= 2)
                Debug.Log("[AudioDiagnostics] ✅ BackgroundMusicManager has 2 AudioSources for crossfade.");
            else
                Debug.LogWarning("[AudioDiagnostics] ⚠️ BackgroundMusicManager has insufficient AudioSources for crossfade.");
        }
        else
        {
            Debug.LogWarning("[AudioDiagnostics] ❌ BackgroundMusicManager not found.");
        }

        Debug.Log("<color=cyan>[AudioDiagnostics]</color> Audio system check completed.");
    }

    private void CheckAudioSource(MonoBehaviour system, string name)
    {
        if (system == null)
        {
            Debug.LogWarning($"[AudioDiagnostics] ❌ {name} not found in scene.");
            return;
        }

        var source = system.GetComponent<AudioSource>();
        if (source == null)
        {
            Debug.LogWarning($"[AudioDiagnostics] ❌ {name} does not have an AudioSource.");
        }
        else if (!source.enabled || !source.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[AudioDiagnostics] ⚠️ {name}'s AudioSource is present but not active.");
        }
        else
        {
            Debug.Log($"[AudioDiagnostics] ✅ {name} has an active AudioSource.");
        }
    }
#if UNITY_EDITOR
    public static void RunDiagnosticsStatic()
    {
        Debug.Log("<color=cyan>[AudioDiagnostics]</color> Running from editor...");

        var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length == 0)
            Debug.LogWarning("[AudioDiagnostics] ❌ No AudioListener found in scene!");
        else if (listeners.Length > 1)
            Debug.LogWarning($"[AudioDiagnostics] ⚠️ Multiple AudioListeners detected ({listeners.Length}). This may cause issues.");
        else
            Debug.Log("[AudioDiagnostics] ✅ Single AudioListener detected.");

        Debug.Log($"[AudioDiagnostics] 🎧 AudioListener volume: {AudioListener.volume}");

        CheckStatic<HintUIController>("HintUIController");
        CheckStatic<ScoreUIController>("ScoreUIController");
        CheckStatic<DialogueManager>("DialogueManager");
        CheckBGMStatic();

        Debug.Log("<color=cyan>[AudioDiagnostics]</color> Diagnostics complete.");
    }

    private static void CheckStatic<T>(string name) where T : MonoBehaviour
    {
        var system = Object.FindAnyObjectByType<T>();
        if (system == null)
        {
            Debug.LogWarning($"[AudioDiagnostics] ❌ {name} not found.");
            return;
        }

        var source = system.GetComponent<AudioSource>();
        if (source == null)
            Debug.LogWarning($"[AudioDiagnostics] ❌ {name} has no AudioSource.");
        else if (!source.enabled || !source.gameObject.activeInHierarchy)
            Debug.LogWarning($"[AudioDiagnostics] ⚠️ {name}'s AudioSource is disabled or inactive.");
        else
            Debug.Log($"[AudioDiagnostics] ✅ {name} AudioSource OK.");
    }

    private static void CheckBGMStatic()
    {
        var bgm = Object.FindAnyObjectByType<BackgroundMusicManager>();
        if (bgm == null)
        {
            Debug.LogWarning("[AudioDiagnostics] ❌ BackgroundMusicManager not found.");
            return;
        }

        var sources = bgm.GetComponents<AudioSource>();
        if (sources.Length < 2)
            Debug.LogWarning("[AudioDiagnostics] ⚠️ BackgroundMusicManager has fewer than 2 AudioSources.");
        else
            Debug.Log("[AudioDiagnostics] ✅ BackgroundMusicManager has 2+ AudioSources.");
    }
#endif

}
