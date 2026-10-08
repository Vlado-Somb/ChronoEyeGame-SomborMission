using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class DistortionControllerUITK : MonoBehaviour
{
    [Header("UI Document (assign in Inspector)")]
    [SerializeField] private UIDocument uiDoc;

    [Header("Default Flicker Pattern")]
    [Tooltip("Sequence of delays (seconds) for glitch on/off toggles")]
    public float[] flickerPattern = new float[] { 0.2f, 0.2f, 0.2f };

    private VisualElement dialogPanel;
    private VisualElement glitchPanel;

    void Awake()
    {
        if (uiDoc == null) uiDoc = GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;

        dialogPanel = root.Q<VisualElement>("DialogOverlay");
        glitchPanel = root.Q<VisualElement>("PanelGlitch");

        if (dialogPanel == null)
            Debug.LogError($"[{name}] missing <VisualElement name=\"DialogOverlay\">");
        if (glitchPanel == null)
            Debug.LogError($"[{name}] missing <VisualElement name=\"PanelGlitch\">");
    }

    /// <summary>
    /// Play the default glitch pattern.
    /// </summary>
    public void PlayGlitch()
    {
        PlayGlitch(flickerPattern);
    }

    /// <summary>
    /// Play a custom glitch pattern.
    /// </summary>
    public void PlayGlitch(float[] pattern)
    {
        if (dialogPanel == null || glitchPanel == null) return;
        if (pattern == null || pattern.Length == 0) return;

        StopAllCoroutines();
        StartCoroutine(Flicker(pattern));
    }

    private IEnumerator Flicker(float[] pattern)
    {
        bool showGlitch = true;
        foreach (float wait in pattern)
        {
            // toggle panels
            glitchPanel.style.display = showGlitch ? DisplayStyle.Flex : DisplayStyle.None;
            dialogPanel.style.display = showGlitch ? DisplayStyle.None : DisplayStyle.Flex;

            yield return new WaitForSeconds(wait);
            showGlitch = !showGlitch;
        }

        // when done, ensure normal dialog is back
        dialogPanel.style.display = DisplayStyle.Flex;
        glitchPanel.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Immediately stops any ongoing glitch and restores the dialog panel.
    /// </summary>
    public void StopGlitch()
    {
        StopAllCoroutines();
        if (dialogPanel != null) dialogPanel.style.display = DisplayStyle.Flex;
        if (glitchPanel != null) glitchPanel.style.display = DisplayStyle.None;
    }
}
