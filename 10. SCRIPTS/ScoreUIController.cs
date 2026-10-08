using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ScoreUIController : MonoBehaviour
{
    [Header("UI Document")]
    [Tooltip("UI Document that contains the score panel UXML")]
    public UIDocument scoreUIDocument;

<<<<<<< Updated upstream
    [Header("Manager Reference")]
    [Tooltip("Link to ScoreManager instance (drag from hierarchy)")]
    [SerializeField] private ScoreManager scoreManager;

    private bool debugScoreUI = false;
    public int _toastTime;

=======
>>>>>>> Stashed changes
    [Header("Audio Settings")]
    [Tooltip("Sound to play whenever the score changes")]
    public AudioClip normalPointsClip;
    public AudioClip bonusPointsClip;
    public AudioClip penaltyPointsClip;
    [Tooltip("Volume for the scoring sound")]
    [Range(0f, 1f)] public float audioVolume = 1f;

    [Header("Color Settings")]
    [Tooltip("Background color of the score panel")]
    public Color normalPointsColor = new Color(0f, 0.5f, 0f, 0.5f);
    public Color bonusPointsColor = new Color(0f, 0f, 0.5f, 0.5f);
    public Color penaltyPointsColor = new Color(0.5f, 0f, 0f, 0.5f);

    // — Intuitivna imena —
    private VisualElement scoreRootElement;
<<<<<<< Updated upstream
    private VisualElement _scorePanel;

=======
>>>>>>> Stashed changes

    [Header("UI Text Defaults (srpski)")]
    [Tooltip("Tekst naslova panela poena")]
    [SerializeField]
    private string defaultScoreTitle = "Osvojeni poeni:";
<<<<<<< Updated upstream

    private Label _toastLabel;
    private Label _newPointsLabel;
    private Label _scoreTitleLabel; // sada se vrednost postavlja iz defaultScoreTitle
    private Label _scoreValueLabel;

    private AudioSource _audioSource;

=======
    private Label scoreTitleLabel; // sada se vrednost postavlja iz defaultScoreTitle
    private Label scoreValueLabel;
    private AudioSource _audioSource;

    private TextField _normalInput, _bonusInput, _penaltyInput;
    // toast display
    private Label _toastLabel;
    // current messages
    private string _normalToast;
    private string _bonusToast;
    private string _penaltyToast;
>>>>>>> Stashed changes

    [Header("Toast Text Defaults (srpski)")]
    [Tooltip("Poruka kada igrač dobije obične poene")]
    [SerializeField] private string defaultNormalToast = "Bravo! Osvojili ste poene!";
    [Tooltip("Poruka kada igrač dobije bonus poene")]
    [SerializeField] private string defaultBonusToast = "Fantastično! Bonus poeni su vaši!";
    [Tooltip("Poruka kada igrač izgubi poene")]
    [SerializeField] private string defaultPenaltyToast = "Ups! Izgubili ste poene!";


    void Awake()
    {
        // Guard: UIDocument assigned?
        if (scoreUIDocument == null)
        {
            Debug.LogError("[ScoreUIController] scoreUIDocument is not assigned!", this);
            enabled = false;
            return;
        }

        // Grab the root visual element
        scoreRootElement = scoreUIDocument.rootVisualElement;
        if (scoreRootElement == null)
        {
            Debug.LogError("[ScoreUIController] Failed to get rootVisualElement!", this);
            enabled = false;
            return;
        }

<<<<<<< Updated upstream
        // Query visual elements
        _scorePanel = scoreRootElement.Q<VisualElement>("ScorePanel");
        _toastLabel = scoreRootElement.Q<Label>("ScoreToast");
        _newPointsLabel = scoreRootElement.Q<Label>("NewPoints");
        _scoreTitleLabel = scoreRootElement.Q<Label>("ScoreTitle");
        _scoreValueLabel = scoreRootElement.Q<Label>("ScoreValue");


        // Setup label content
        // Hide all UI labels and setup initial values
        _scorePanel.visible = false;
        _scoreTitleLabel.visible = false;
        _scoreValueLabel.visible = false;
        _toastLabel.visible = false;
        _newPointsLabel.visible = false;

        if (_scoreTitleLabel != null)
            _scoreTitleLabel.text = defaultScoreTitle;

        if (_scorePanel != null)
            _scorePanel.style.backgroundColor = new StyleColor(normalPointsColor);

        // Optional debug logs
        if (debugScoreUI)
        {
            if (_scoreTitleLabel == null)
                Debug.LogWarning("[ScoreUIController] 'ScoreTitle' Label not found.");

            if (_scoreValueLabel == null)
                Debug.LogWarning("[ScoreUIController] 'ScoreValue' Label not found.");

            if (_toastLabel == null)
                Debug.LogWarning("[ScoreUIController] 'ScoreToast' Label not found.");

            if (_newPointsLabel == null)
                Debug.LogWarning("[ScoreUIController] 'NewPoints' Label not found.");

            if (_scorePanel == null)
                Debug.LogWarning("[ScoreUIController] 'ScorePanel' not found.");
        }


        // Setup audio
=======
        // Query the two labels
        scoreTitleLabel = scoreRootElement.Q<Label>("ScoreTitle");
        scoreValueLabel = scoreRootElement.Q<Label>("ScoreValue");

        if (scoreTitleLabel != null)
            scoreTitleLabel.text = defaultScoreTitle;
        else
            Debug.LogWarning("[ScoreUIController] 'ScoreTitle' Label not found.");

        _toastLabel = scoreRootElement.Q<Label>("ScoreToast");
        _toastLabel.visible = false;

        // grab inputs and set defaults
        _normalInput = scoreRootElement.Q<TextField>("NormalToastInput");
        _bonusInput = scoreRootElement.Q<TextField>("BonusToastInput");
        _penaltyInput = scoreRootElement.Q<TextField>("PenaltyToastInput");

        _normalInput.value = _normalToast;
        _bonusInput.value = _bonusToast;
        _penaltyInput.value = _penaltyToast;

        // on change, overwrite the running strings
        _normalInput.RegisterValueChangedCallback(evt => _normalToast = evt.newValue);
        _bonusInput.RegisterValueChangedCallback(evt => _bonusToast = evt.newValue);
        _penaltyInput.RegisterValueChangedCallback(evt => _penaltyToast = evt.newValue);

        // Apply the inspector‐set panel color
        scoreRootElement.style.backgroundColor = new StyleColor(normalPointsColor);

        // Set up audio source
>>>>>>> Stashed changes
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.volume = audioVolume;

<<<<<<< Updated upstream
        // Guard: ScoreManager assigned?
        if (scoreManager == null)
        {
            Debug.LogError("[ScoreUIController] ScoreManager reference not assigned in Inspector!", this);
            scoreRootElement.style.display = DisplayStyle.None;
            return;
        }

        if (!scoreManager.IsScoringEnabled)
        {
            Debug.Log("[ScoreUIController] Score panel hidden (scoring disabled).", this);
            scoreRootElement.style.display = DisplayStyle.None;
            return;
        }

        // Register to score change events
        if (_scoreValueLabel != null)
        {
            scoreManager.OnScoreChanged += HandleScoreChanged;
            scoreManager.OnScoreDelta += HandleScoreDelta;
            HandleScoreChanged(scoreManager.CurrentScore);
            Debug.Log("[ScoreUIController] Subscribed to score changes.", this);
        }
    }

=======
        // inicijalizuj fallback poruke iz Inspektora
        _normalToast = defaultNormalToast;
        _bonusToast = defaultBonusToast;
        _penaltyToast = defaultPenaltyToast;

        // podešavanje TextField inputa
        _normalInput.value = _normalToast;
        _bonusInput.value = _bonusToast;
        _penaltyInput.value = _penaltyToast;

        // Subscribe to ScoreManager events
        var scoreMgr = ScoreManager.Instance;
        if (scoreValueLabel != null && scoreMgr != null && scoreMgr.IsScoringEnabled)
        {
            scoreMgr.OnScoreChanged += HandleScoreChanged;
            scoreMgr.OnScoreDelta += HandleScoreDelta;
            HandleScoreChanged(scoreMgr.CurrentScore);
            Debug.Log("[ScoreUIController] Subscribed to score changes.", this);
        }
        else
        {
            // Hide if scoring disabled or missing
            scoreRootElement.style.display = DisplayStyle.None;
            Debug.Log("[ScoreUIController] Score panel hidden (scoring disabled or no ScoreManager).", this);
        }

    }
>>>>>>> Stashed changes

    void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
            ScoreManager.Instance.OnScoreDelta -= HandleScoreDelta;
        }

    }
    string msgToastDelta;
    /// <summary>
    /// Updates displayed score, plays a sound, and logs the change.
    /// </summary>
    private void HandleScoreChanged(int newScore)
    {
<<<<<<< Updated upstream
        if (_scoreValueLabel == null) return;

        _scoreValueLabel.text = newScore.ToString();
=======
        if (scoreValueLabel == null) return;

        scoreValueLabel.text = newScore.ToString();
>>>>>>> Stashed changes
        Debug.Log($"[ScoreUIController] Score updated to {newScore}.", this);

        StopAllCoroutines();
        _toastLabel.text = msgToastDelta;
        _toastLabel.visible = true;
        StartCoroutine(HideToastAfter(5f));
    }

    // bira zvuk i boju po tipu promene
    private void HandleScoreDelta(int delta)
    {
<<<<<<< Updated upstream
        if (_audioSource == null || _toastLabel == null)
            return;

        if (delta > 0)
        {
            if (delta == scoreManager.PointsOnMissionComplete)
            {
                _toastLabel.text = defaultBonusToast;
                _audioSource.PlayOneShot(bonusPointsClip);
                _scorePanel.style.backgroundColor = new StyleColor(bonusPointsColor);
            }
            else
            {
                _toastLabel.text = defaultNormalToast;
                _audioSource.PlayOneShot(normalPointsClip);
                _scorePanel.style.backgroundColor = new StyleColor(normalPointsColor);
=======
        if (_audioSource == null) return;

        if (delta > 0)
        {
            // bonus ili obični
            if (delta == ScoreManager.Instance.PointsOnMissionComplete)
            {
                msgToastDelta = _bonusToast;
                _audioSource.PlayOneShot(bonusPointsClip);
                scoreRootElement.style.backgroundColor = new StyleColor(bonusPointsColor);
                Debug.Log($"[ScoreUIController] Bonus {delta} poena!", this);
            }
            else
            {
                msgToastDelta = _normalToast;
                _audioSource.PlayOneShot(normalPointsClip);
                scoreRootElement.style.backgroundColor = new StyleColor(normalPointsColor);
                Debug.Log($"[ScoreUIController] +{delta} običnih poena.", this);
>>>>>>> Stashed changes
            }
        }
        else if (delta < 0)
        {
<<<<<<< Updated upstream
            _toastLabel.text = defaultPenaltyToast;
            _audioSource.PlayOneShot(penaltyPointsClip);
            _scorePanel.style.backgroundColor = new StyleColor(penaltyPointsColor);
        }

        _newPointsLabel.text = $"{(delta > 0 ? "+" : "")}{delta}";
        _scorePanel.visible = true;
        _toastLabel.visible = true;
        _newPointsLabel.visible = true;
        _scoreTitleLabel.visible = true;
        _scoreValueLabel.visible = true;
        StopAllCoroutines();
        _toastActive = true;
        StartCoroutine(HideToastAfter(_toastTime));
    }

    private bool _toastActive = false;
    public IEnumerator WaitForToastToComplete()
    {
        while (_toastActive)
            yield return null;
=======
            msgToastDelta = _penaltyToast;
            _audioSource.PlayOneShot(penaltyPointsClip);
            scoreRootElement.style.backgroundColor = new StyleColor(penaltyPointsColor);
            Debug.Log($"[ScoreUIController] {delta} kaznenih poena.", this);
        }
>>>>>>> Stashed changes
    }

    private IEnumerator HideToastAfter(float secs)
    {
        yield return new WaitForSeconds(secs);
<<<<<<< Updated upstream
        _scorePanel.visible = false;
        _toastLabel.visible = false;
        _newPointsLabel.visible = false;
        _scoreTitleLabel.visible = false;
        _scoreValueLabel.visible = false;
        _toastActive = false;
    }

=======
        _toastLabel.visible = false;
    }
>>>>>>> Stashed changes
}
