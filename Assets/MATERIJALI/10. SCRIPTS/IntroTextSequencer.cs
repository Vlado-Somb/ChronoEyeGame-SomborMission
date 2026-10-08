
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class IntroTextSequencer : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button skipButton;    // active immediately
    [SerializeField] private Button nextButton;    // active after sequence

    [Header("Target Scenes")]
    [Tooltip("Scene name to load when Skip is pressed.")]
    [SerializeField] private string skipSceneName;
    [Tooltip("Scene name to load when Next is pressed.")]
    [SerializeField] private string nextSceneName;

    [Header("Text Sequence")]
    [SerializeField] private TextMeshProUGUI textUI;
    [TextArea(3, 10)]
    [SerializeField] private string[] textChunks;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float displayDuration = 3f;

    private void Start()
    {
        StartSequence();
    }

    /// <summary>
    /// Hides/initializes buttons and starts the coroutine.
    /// </summary>
    public void StartSequence()
    {
        // ensure clean slate
        StopAllCoroutines();
        skipButton.onClick.RemoveAllListeners();
        nextButton.onClick.RemoveAllListeners();

        // Skip is available immediately
        skipButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(false);
        skipButton.onClick.AddListener(() =>
        {
            StopAllCoroutines();
            SceneTransitionManager.Instance.LoadScene(skipSceneName);
        });

        // begin text flow
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        textUI.alpha = 0f;
        foreach (var chunk in textChunks)
        {
            textUI.text = chunk;
            yield return StartCoroutine(FadeText(0f, 1f));
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(FadeText(1f, 0f));
        }

        // after sequence, swap to Next
        skipButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(true);
        nextButton.onClick.AddListener(() =>
        {
            SceneTransitionManager.Instance.LoadScene(nextSceneName);
        });
    }

    private IEnumerator FadeText(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            textUI.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
    }
}
