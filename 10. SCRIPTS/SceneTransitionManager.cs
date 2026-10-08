using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

static class SceneTransitionInitializer
{
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        // Intentionally empty – silences UDR0001
    }
}

public class SceneTransitionManager : MonoBehaviour
{
#pragma warning disable UDR0001 // Domain Reload Analyzer
    public static SceneTransitionManager Instance;
#pragma warning restore UDR0001 // Domain Reload Analyzer

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvas;
    public float transitionDuration = 1.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }

        // ensure starting state
        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1f;    // fully black
            fadeCanvas.blocksRaycasts = true;
            fadeCanvas.interactable = false;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // First, stop any running fade-coroutines on the old canvas
        StopAllCoroutines();

        // Try to grab the *new* CanvasGroup
        CanvasGroup sceneCanvas = FindFirstObjectByType<CanvasGroup>();
        if (sceneCanvas != null && sceneCanvas != fadeCanvas)
        {
            fadeCanvas = sceneCanvas;
        }
        else if (fadeCanvas == null)
        {
            Debug.LogError("SceneTransitionManager: No CanvasGroup found to perform fade!");
            return;
        }

        // Reset to fully black then fade in
        fadeCanvas.alpha = 1f;
        fadeCanvas.blocksRaycasts = true;
        fadeCanvas.interactable = false;
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Public entry to fade out, load a scene by name, then fade back in.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not in Build Settings! Check name and spelling.");
            return;
        }

        Debug.Log($"[SceneTransitionManager] Loading scene: {sceneName}");
        StartCoroutine(TransitionToScene(sceneName));
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return true;
        }
        return false;
    }


    private IEnumerator TransitionToScene(string sceneName)
    {
        // fade to black
        yield return StartCoroutine(FadeOut());

        // load the requested scene
        SceneManager.LoadScene(sceneName);

        // fade back in
        yield return StartCoroutine(FadeIn());

        // after fade completes, start any intro sequence in new scene
        var sequencer = FindFirstObjectByType<IntroTextSequencer>();
        if (sequencer != null)
            sequencer.StartSequence();
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        fadeCanvas.blocksRaycasts = true;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(t / transitionDuration);
            yield return null;
        }
    }


    private IEnumerator FadeIn()
    {
        // If the canvas goes missing, bail out
        if (fadeCanvas == null) yield break;

        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            if (fadeCanvas == null) yield break;
            fadeCanvas.alpha = 1f - Mathf.Clamp01(t / transitionDuration);
            yield return null;
        }
        if (fadeCanvas != null)
            fadeCanvas.blocksRaycasts = false;
    }
}

