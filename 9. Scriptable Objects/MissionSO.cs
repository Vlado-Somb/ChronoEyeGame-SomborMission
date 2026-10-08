using System;
using System.Collections.Generic;
using UnityEngine;

// This is the only SO you’ll create in the Project window.
[CreateAssetMenu(
    fileName = "NewMission",
    menuName = "Missions/MissionSO",
    order = 100)]

public class MissionSO : ScriptableObject
{
    [Tooltip("Friendly name for this mission")]
    public string missionName;

    [Tooltip("All of this mission’s tasks in order")]
    public List<MissionTask> tasks = new List<MissionTask>();

    [Header("Waypoint Database")]
    [Tooltip("Central list of all preconfigured waypoints")]
    public WaypointDatabase waypointDatabase;

    /// <summary> True when every task is marked completed. </summary>
    public bool IsComplete()
    {
        foreach (var t in tasks)
            if (!t.isCompleted) return false;
        return true;
    }
}

[Serializable]
public class MissionTask
{
    public enum MissionTaskType { GPS, POI, Question, TextInput, Custom }

    [Tooltip("What kind of task this is")]
    public MissionTaskType taskType;
    [Tooltip("Optional title for this task (displayed in UI headers)")]
    public string title;

    public string completionTitle;
    public string completionQuote;
    public float taskCompletionDelay = 2f;

    [Header("Common")]
    [Tooltip("What the player sees as instructions for this task")]
    [TextArea(2, 4)]
    public string instruction;

    [Header("Waypoint-Based GPS")]
    [Tooltip("ID of the waypoint to monitor during this task.")]
    public string waypointId;

    [Tooltip("Which zone the player must enter to complete this GPS task.")]
    public WaypointZoneLevel confirmationZone = WaypointZoneLevel.Zone2;

    [Header("POI Settings")]
    [Tooltip("Prefab to spawn when we reach this task (must be tagged \"POI\")")]
    public GameObject poiPrefab;
    [Tooltip("Unique identifier for this POI (e.g. \"museum_entrance\")")]
    public string poiIdentifier;

    [Header("Question Settings")]
    [Tooltip("Text of the multiple-choice question")]
    public string questionText;
    [Tooltip("Possible answers")]
    public string[] Choices;
    [Tooltip("Index of the correct answer in Choices[]")]
    public int CorrectChoiceIndex;

    [Header("Text-Input Settings")]
    [Tooltip("Placeholder text for the input field")]
    public string inputPlaceholder;
    [Tooltip("Placeholder text inside the answer input field (e.g. 'Type your answer here…')")]
    public string answerPlaceholder;
    [Tooltip("The exact string we consider correct")]
    public string expectedAnswer;

    [Header("Custom Settings")]
    public string customData;

    [HideInInspector]
    public bool isCompleted;
}