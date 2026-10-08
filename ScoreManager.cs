using UnityEngine;
using UnityEngine.UI;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Enable/Disable Scoring")]
    [SerializeField, Tooltip("Isključi bodovanje ako je false")]
    private bool scoringEnabled = true;

    /// <summary>Dozvoljava spoljašnje uključivanje/isključivanje bodovanja</summary>
    public void SetScoringEnabled(bool enabled)
    {
        scoringEnabled = enabled;
        Debug.Log($"[ScoreManager] Scoring enabled = {scoringEnabled}");
    }
    public bool IsScoringEnabled => scoringEnabled;

    /* [Header("UI References")]
    public Text scoreText;
    public Text toastText; */

    [Header("Point Settings")]
    [Tooltip("Poeni po zadatku")]
    [SerializeField] private int pointsPerTask = 10;
    [Tooltip("Bonus poeni na kraj misije")]
    [SerializeField] private int pointsOnMissionComplete = 30;
    [Tooltip("Kazna za netačan odgovor")]
    [SerializeField] private int penaltyForWrongAnswer = 2;

    // ——— Getters ———
    /// <summary>Koliko poena se daje po zadatku (iz Inspektora)</summary>
    public int PointsPerTask => pointsPerTask;

    /// <summary>Koliko poena se daje na kraj misije (iz Inspektora)</summary>
    public int PointsOnMissionComplete => pointsOnMissionComplete;

    /// <summary>Koliko poena se oduzima za netačan odgovor (iz Inspektora)</summary>
    public int PenaltyForWrongAnswer => penaltyForWrongAnswer;

    private int score;
    public int CurrentScore => score;
    /// <summary>Emitovano sa novim skorom nakon promene</summary>
    public event Action<int> OnScoreChanged;
    /// <summary>Emitovano samo sa delta vrednošću promene skora</summary>
    public event Action<int> OnScoreDelta;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        score = PlayerPrefs.GetInt("GameScore", 0);
        // UpdateUI();
    }

    public void AddTaskPoints() => AddPoints(pointsPerTask);
    public void AddMissionCompletePoints() => AddPoints(pointsOnMissionComplete);
    public void DeductWrongAnswer() => AddPoints(-penaltyForWrongAnswer);

    private void AddPoints(int amount)
    {
        if (!scoringEnabled) return;      // ← guard: ništa ne radi ako je isključeno

        score += amount;
        PlayerPrefs.SetInt("GameScore", score);
        PlayerPrefs.Save();

        // UpdateUI();
        // ShowToast(amount); 
        OnScoreChanged?.Invoke(score);
        OnScoreDelta?.Invoke(amount);
    }

    /* private void UpdateUI()
     {
         if (scoreText != null)
             scoreText.text = $"Poeni: {score}";
     }

     private void ShowToast(int amt)
     {
         if (toastText == null) return;
         if (amt > 0)
             toastText.text = $"+{amt} poena!";
         else
             toastText.text = $"{amt} poena.";
         CancelInvoke(nameof(ClearToast));
         Invoke(nameof(ClearToast), 2f);
     }

     private void ClearToast() => toastText.text = "";
    */
}
