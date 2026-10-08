
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;


[CreateAssetMenu(menuName = "Dialog/Dialog Asset", fileName = "NewDialog")]
public class DialogAsset : ScriptableObject
{
    [Tooltip("Unique identifier (used for POI triggers or runtime lookup)")]
    public string assetID = "default_id";
    [Tooltip("List of dialog lines for this asset")]
    public List<DialogLine> lines = new List<DialogLine>();
}

[System.Serializable]
public class DialogLine
{
    [Tooltip("Who is speaking on this line")]
    public string speakerName;
    [TextArea] public string text;
    public AudioClip voiceOver;

    [Header("Branching (optional)")]
    public string[] choices;           // A/B labels
    public DialogAsset nextOnChoice0;  // SO to jump to if player picks choice 0
    public DialogAsset nextOnChoice1;  // SO to jump to if player picks choice 1

    [Header("Distortion (optional)")]
    public bool triggerGlitch;     // whether to fire a glitch after this line
    public float[] glitchPattern;    // e.g. new float[]{0.2f,0.2f,0.2f}
    public float glitchDuration;    // how long to hold the glitch
}

