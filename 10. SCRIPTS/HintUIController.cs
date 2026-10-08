using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

// === HintType.cs ===
public enum HintType
{
    Info,
    Warning,
    Help
}

/// <summary>
/// Kontroliše redosled i trajanje prikaza hint-ova u UI.
/// Od sada podržava override trajanje po hint-u.
/// </summary>
public class HintUIController : MonoBehaviour
{
    [Header("Timing Settings")]
    [Tooltip("Koliko sekundi hint ostaje vidljiv po default-u, ako nema override-a.")]
    public float autoHideSeconds = 5f;
    [Tooltip("Trajanje fade-in i fade-out animacije.")]
    public float fadeDuration = 0.7f;
    [Tooltip("Minimalno vreme između dva hint-a.")]
    public float minimumTimeSinceLastHint = 3f;

    [Header("Audio Cues")]
    public AudioClip infoSound;
    public AudioClip warningSound;
    public AudioClip helpSound;

    [Header("Hint Colors (editable in Inspector)")]
    [SerializeField] private Color defaultColor = Color.black;
    [SerializeField] private Color infoColor = new Color(0.133f, 0.333f, 0.867f);
    [SerializeField] private Color warningColor = new Color(0.867f, 0.2f, 0.2f);
    [SerializeField] private Color helpColor = new Color(0.133f, 0.667f, 0.333f);

    private UIDocument _doc;
    private VisualElement _root;
    private Label _title;
    private Label _text;
    private AudioSource _audio;

    private Queue<(string message, HintType type, float duration)> _hintQueue = new Queue<(string, HintType, float)>();
    private bool _hintShowing = false;
    private float _lastHintTime = -999f;

    private void Awake()
    {
        _doc = GetComponent<UIDocument>();
        _root = _doc.rootVisualElement.Q<VisualElement>("hintRoot");
        _title = _doc.rootVisualElement.Q<Label>("hintTitle");
        _text = _doc.rootVisualElement.Q<Label>("hintText");

        _audio = gameObject.AddComponent<AudioSource>();
        _root.style.opacity = 0;
        _root.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Prikazuje hint sa opcionalnim override-om trajanja.
    /// <param name="message">Tekst hint-a.</param>
    /// <param name="type">Tip hint-a (boja, zvuk).</param>
    /// <param name="durationOverride">Ukupno vreme prikaza; ako <= 0, koristi autoHideSeconds.</param>
    /// </summary>
    public void ShowHint(string message, HintType type, float durationOverride = -1f)
    {
        if (Time.time - _lastHintTime < minimumTimeSinceLastHint)
            return;

        _lastHintTime = Time.time;
        float duration = durationOverride > 0f ? durationOverride : autoHideSeconds;
        _hintQueue.Enqueue((message, type, duration));

        if (!_hintShowing)
            StartCoroutine(ProcessHintQueue());
    }

    private IEnumerator ProcessHintQueue()
    {
        _hintShowing = true;

        while (_hintQueue.Count > 0)
        {
            var (message, type, duration) = _hintQueue.Dequeue();

            _text.text = message;
            _title.text = type.ToString().ToUpper();
            _root.style.display = DisplayStyle.Flex;

            SetColorByType(type);
            PlaySoundByType(type);

            yield return FadeIn();
            yield return new WaitForSeconds(duration);
            yield return FadeOut();

            _root.style.display = DisplayStyle.None;
        }

        _hintShowing = false;
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        _root.style.opacity = 0f;

        while (elapsed < fadeDuration)
        {
            _root.style.opacity = elapsed / fadeDuration;
            elapsed += Time.deltaTime;
            yield return null;
        }

        _root.style.opacity = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startOpacity = _root.resolvedStyle.opacity;

        while (elapsed < fadeDuration)
        {
            _root.style.opacity = Mathf.Lerp(startOpacity, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _root.style.opacity = 0f;
    }

    private void SetColorByType(HintType type)
    {
        Color bg;
        switch (type)
        {
            case HintType.Info: bg = infoColor; break;
            case HintType.Warning: bg = warningColor; break;
            case HintType.Help: bg = helpColor; break;
            default: bg = defaultColor; break;
        }

        _root.style.backgroundColor = new StyleColor(bg);
    }

    private void PlaySoundByType(HintType type)
    {
        AudioClip clip = type switch
        {
            HintType.Info => infoSound,
            HintType.Warning => warningSound,
            HintType.Help => helpSound,
            _ => null
        };

        if (clip != null)
            _audio.PlayOneShot(clip);
    }
}
