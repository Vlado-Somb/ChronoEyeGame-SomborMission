#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using MyGame.Waypoint;
using MyGame.Timeline;

[CustomPropertyDrawer(typeof(WaypointTriggerBinding.Binding))]
public class BindingDrawer : PropertyDrawer
{

    /* Conditions/triggers naming problem, condH, _conditionsFoldouts */

#pragma warning disable UDR0001 // Domain Reload Analyzer
    private static TimelineBlockDatabase _cachedBlockDb;
    private static string[] _cachedBlockLabels;
    private static int[] _cachedBlockIndices;
#pragma warning restore UDR0001 // Domain Reload Analyzer

    private static readonly Dictionary<int, bool> _bindingFoldouts = new();
    private static readonly Dictionary<int, bool> _triggerFoldouts = new();

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void HookProjectChange()
    {
#pragma warning disable UDR0001 // Domain Reload Analyzer
        EditorApplication.projectChanged += InvalidateBlockCache;
#pragma warning restore UDR0001 // Domain Reload Analyzer
    }

    [MenuItem("Tools/Timeline/Refresh Block Cache")]
    private static void InvalidateBlockCache()
    {
        _cachedBlockDb = null;
        _cachedBlockLabels = null;
        _cachedBlockIndices = null;
        // repaint inspectors so the dropdown updates immediately
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
#endif

    private static void LoadTimelineBlockDatabase()
    {
        var timelineSettings = WaypointEditorContext.ActiveTimelineSettings;
        var currentDb = timelineSettings?.timelineDatabase;

        bool mustRebuild =
            _cachedBlockDb == null ||
            _cachedBlockDb != currentDb ||
            _cachedBlockLabels == null ||
            _cachedBlockIndices == null ||
            (_cachedBlockDb != null && _cachedBlockLabels.Length != _cachedBlockDb.GetBlockCount());

        if (!mustRebuild) return;

        _cachedBlockDb = currentDb;

        if (_cachedBlockDb?.blocks != null)
        {
            _cachedBlockLabels = _cachedBlockDb.GetBlockLabels();
            _cachedBlockIndices = Enumerable.Range(0, _cachedBlockDb.GetBlockCount()).ToArray();
        }
        else
        {
            _cachedBlockLabels = Array.Empty<string>();
            _cachedBlockIndices = Array.Empty<int>();
        }
    }

    private static string GetBlockLabel(int index)
    {
        if (_cachedBlockDb == null || _cachedBlockDb.blocks == null) return $"Block {index}";
        if (index < 0 || index >= _cachedBlockDb.blocks.Count) return $"Invalid [{index}]";
        return _cachedBlockDb.blocks[index]?.blockId ?? $"Unnamed [{index}]";
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property == null || property.serializedObject == null) return;

        var settings = WaypointEditorContext.ActiveSettings;
        var timeline = WaypointEditorContext.ActiveTimelineSettings;
        if (settings == null || timeline == null) return;

        var labelProp = property.FindPropertyRelative("bindingLabel");
        var groupIdProp = property.FindPropertyRelative("groupId");

        var waypointProp = property.FindPropertyRelative("waypointId");
        var zoneProp = property.FindPropertyRelative("activationZone");
        var targetBlockProp = property.FindPropertyRelative("targetBlockIndex");
        var triggerProp = property.FindPropertyRelative("triggers");


        float y = position.y;
        float lineH = EditorGUIUtility.singleLineHeight;
        float spacing = 4f;

        string fallback = waypointProp.stringValue + "_" + targetBlockProp.intValue;
        var groupCondIds = new List<string>();
        for (int i = 0; i < triggerProp.arraySize; i++)
        {
            var condProp = triggerProp.GetArrayElementAtIndex(i);
            var condIdProp = condProp.FindPropertyRelative("conditionId");
            if (condIdProp != null && !string.IsNullOrEmpty(condIdProp.stringValue))
                groupCondIds.Add(condIdProp.stringValue);
        }
        string computedGroupId = $"{fallback}_{string.Join("_", groupCondIds)}";
        groupIdProp.stringValue = computedGroupId;
        property.serializedObject.ApplyModifiedProperties();



        if (string.IsNullOrEmpty(labelProp.stringValue))
        {
            EditorGUI.HelpBox(new Rect(position.x, y, position.width, lineH), "Optional: Add a descriptive Binding Label", MessageType.Info);
            y += lineH + spacing;
        }

        if (waypointProp == null || targetBlockProp == null || zoneProp == null || triggerProp == null)
        {
            EditorGUI.HelpBox(position, "Serialized properties are missing or corrupted.", MessageType.Error);
            return;
        }

        LoadTimelineBlockDatabase();
        var blockLabels = _cachedBlockLabels ?? Array.Empty<string>();
        var blockIndices = _cachedBlockIndices ?? Array.Empty<int>();

        // Resolve waypoint IDs
        string[] wpIds = Array.Empty<string>();
        var defaultDb = settings.GetDefaultDatabase();
        if (defaultDb != null)
            wpIds = defaultDb.GetWaypointIds() ?? Array.Empty<string>();

        int hash = property.propertyPath.GetHashCode();
        if (!_bindingFoldouts.ContainsKey(hash)) _bindingFoldouts[hash] = true;
        if (!_triggerFoldouts.ContainsKey(hash)) _triggerFoldouts[hash] = true;

        // Draw editable label field
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineH), labelProp, new GUIContent("Binding Label"));
        y += lineH + spacing;

        EditorGUI.LabelField(new Rect(position.x, y, position.width, lineH),
            $"Group Key Preview: {computedGroupId}", EditorStyles.miniLabel);
        y += lineH + spacing;


        // Binding foldout
        Rect foldRect = new Rect(position.x, y, position.width, lineH);
        string wpLabel = string.IsNullOrEmpty(waypointProp.stringValue) ? "?" : waypointProp.stringValue;
        string foldTitle = $"{wpLabel} → {GetBlockLabel(targetBlockProp.intValue)}";

        _bindingFoldouts[hash] = EditorGUI.Foldout(foldRect, _bindingFoldouts[hash], foldTitle, true);
        y += lineH + spacing;

        if (!_bindingFoldouts[hash]) return;

        // Waypoint dropdown
        if (wpIds.Length > 0)
        {
            string currentId = waypointProp?.stringValue ?? "";
            int selected = Mathf.Max(0, Array.IndexOf(wpIds, currentId));

            int newIdx = EditorGUI.Popup(new Rect(position.x, y, position.width, lineH), "Waypoint", selected, wpIds);
            if (newIdx != selected)
                waypointProp.stringValue = wpIds[newIdx];
        }
        else
        {
            EditorGUI.HelpBox(new Rect(position.x, y, position.width, lineH), "No waypoint database found.", MessageType.Info);
        }
        y += lineH + spacing;

        // Activation zone
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineH), zoneProp);
        y += lineH + spacing;

        // Block dropdown
        if (blockIndices.Length > 0)
        {
            int selected = targetBlockProp.intValue;
            int newIdx = EditorGUI.IntPopup(new Rect(position.x, y, position.width, lineH), "Target Block", selected, blockLabels, blockIndices);
            if (newIdx != selected)
                targetBlockProp.intValue = newIdx;
        }
        else
        {
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineH), targetBlockProp);
        }
        y += lineH + spacing;

        // Triggers/Conditions foldout
        _triggerFoldouts[hash] = EditorGUI.Foldout(new Rect(position.x, y, position.width, lineH), _triggerFoldouts[hash], $"Conditions/triggers ({triggerProp.arraySize})", true);
        y += lineH + spacing;

        if (_triggerFoldouts[hash])
        {
            for (int i = 0; i < triggerProp.arraySize; i++)
            {
                var trigger = triggerProp.GetArrayElementAtIndex(i);
                string condId = trigger.FindPropertyRelative("conditionId")?.stringValue ?? $"Trigger {i}";
                int localHash = (property.propertyPath + i).GetHashCode();

                if (!_triggerFoldouts.ContainsKey(localHash))
                    _triggerFoldouts[localHash] = true;

                var expanded = _triggerFoldouts[localHash];

                // Foldout header
                Rect headerRect = new Rect(position.x + 10, y, position.width - 20, lineH);
                expanded = EditorGUI.Foldout(headerRect, expanded, condId, true);
                _triggerFoldouts[localHash] = expanded;
                y += lineH + spacing;

                if (expanded)
                {
                    float triggerH = EditorGUI.GetPropertyHeight(trigger, true);
                    Rect triggerRect = new Rect(position.x + 12, y, position.width - 24, triggerH);

                    // Light debug background
                    EditorGUI.DrawRect(triggerRect, new Color(0.9f, 0.95f, 1f, 0.1f));

                    // Label (optional)
                    // EditorGUI.LabelField(new Rect(triggerRect.x + 4, triggerRect.y, triggerRect.width, lineH), "🧩 Condition Properties");

                    // Now actual drawer
                    var drawer = new TriggerConditionDrawer();
                    drawer.OnGUI(triggerRect, trigger, new GUIContent($"Trigger {i}"));


                    y += triggerH + spacing;



                    Rect delBtn = new Rect(position.x + 10, y, 180, lineH);
                    if (GUI.Button(delBtn, "− Remove Trigger/Condition"))
                    {
                        triggerProp.DeleteArrayElementAtIndex(i);
                        property.serializedObject.ApplyModifiedProperties();
                        break;
                    }
                    y += lineH + spacing;
                }

            }

            // Add trigger button
            Rect addBtn = new Rect(position.x + 10, y, 180, lineH);
            if (GUI.Button(addBtn, "+ Add Trigger/Condition"))
            {
                triggerProp.arraySize++;
            }
            y += lineH + spacing * 0.5f;

        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int hash = property.propertyPath.GetHashCode();
        if (!_bindingFoldouts.TryGetValue(hash, out bool expanded) || !expanded)
            return EditorGUIUtility.singleLineHeight + 4;

        var triggerProp = property.FindPropertyRelative("triggers");
        float h = 6 * (EditorGUIUtility.singleLineHeight + 8);

        if (_triggerFoldouts.TryGetValue(hash, out bool condOpen) && condOpen)
        {
            for (int i = 0; i < triggerProp.arraySize; i++)
            {
                int localHash = (property.propertyPath + i).GetHashCode();
                if (_triggerFoldouts.TryGetValue(localHash, out bool isOpen) && isOpen)
                {
                    h += EditorGUIUtility.singleLineHeight + 4; // Foldout
                    var drawer = new TriggerConditionDrawer();
                    h += drawer.GetPropertyHeight(triggerProp.GetArrayElementAtIndex(i), label) + 4;
                    h += EditorGUIUtility.singleLineHeight + 4; // Remove button
                }
                else
                {
                    h += EditorGUIUtility.singleLineHeight + 4; // Foldout only
                }
            }

            h += EditorGUIUtility.singleLineHeight;
        }

        return h + 24f;
    }
}

#endif

/*
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using MyGame.Waypoint;
using MyGame.Timeline;

[CustomPropertyDrawer(typeof(WaypointTriggerBinding.Binding))]
public class BindingDrawer : PropertyDrawer
{

    Conditions/triggers naming problem, condH, _conditionsFoldouts 

#pragma warning disable UDR0001 // Domain Reload Analyzer
    private static TimelineBlockDatabase _cachedBlockDb;
    private static string[] _cachedBlockLabels;
    private static int[] _cachedBlockIndices;
#pragma warning restore UDR0001 // Domain Reload Analyzer

    private static readonly Dictionary<int, bool> _bindingFoldouts = new();
    private static readonly Dictionary<int, bool> _triggerFoldouts = new();


    private static void LoadTimelineBlockDatabase()
    {
        if (_cachedBlockDb != null) return;

        var timelineSettings = WaypointEditorContext.ActiveTimelineSettings;
        _cachedBlockDb = timelineSettings?.timelineDatabase;

        if (_cachedBlockDb?.blocks != null)
        {
            _cachedBlockLabels = _cachedBlockDb.GetBlockLabels();
            _cachedBlockIndices = Enumerable.Range(0, _cachedBlockDb.GetBlockCount()).ToArray();
        }
    }

    private static string GetBlockLabel(int index)
    {
        if (_cachedBlockDb == null || _cachedBlockDb.blocks == null) return $"Block {index}";
        if (index < 0 || index >= _cachedBlockDb.blocks.Count) return $"Invalid [{index}]";
        return _cachedBlockDb.blocks[index]?.blockId ?? $"Unnamed [{index}]";
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property == null || property.serializedObject == null) return;

        var settings = WaypointEditorContext.ActiveSettings;
        var timeline = WaypointEditorContext.ActiveTimelineSettings;
        if (settings == null || timeline == null) return;

        var waypointProp = property.FindPropertyRelative("waypointId");
        var zoneProp = property.FindPropertyRelative("activationZone");
        var targetBlockProp = property.FindPropertyRelative("targetBlockIndex");
        var triggerProp = property.FindPropertyRelative("triggers");

        if (waypointProp == null || targetBlockProp == null || zoneProp == null || triggerProp == null)
        {
            EditorGUI.HelpBox(position, "Serialized properties are missing or corrupted.", MessageType.Error);
            return;
        }


        LoadTimelineBlockDatabase();
        var blockLabels = _cachedBlockLabels ?? Array.Empty<string>();
        var blockIndices = _cachedBlockIndices ?? Array.Empty<int>();

        // Resolve waypoint IDs
        string[] wpIds = Array.Empty<string>();
        var defaultDb = settings.GetDefaultDatabase();
        if (defaultDb != null)
            wpIds = defaultDb.GetWaypointIds() ?? Array.Empty<string>();

        int hash = property.propertyPath.GetHashCode();
        if (!_bindingFoldouts.ContainsKey(hash)) _bindingFoldouts[hash] = true;
        if (!_triggerFoldouts.ContainsKey(hash)) _triggerFoldouts[hash] = true;

        float y = position.y;
        float lineH = EditorGUIUtility.singleLineHeight;
        float spacing = 4f;

        // Binding foldout
        Rect foldRect = new Rect(position.x, y, position.width, lineH);
        string wpLabel = string.IsNullOrEmpty(waypointProp.stringValue) ? "?" : waypointProp.stringValue;
        string foldTitle = $"{wpLabel} → {GetBlockLabel(targetBlockProp.intValue)}";

        _bindingFoldouts[hash] = EditorGUI.Foldout(foldRect, _bindingFoldouts[hash], foldTitle, true);
        y += lineH + spacing;

        if (!_bindingFoldouts[hash]) return;

        // Waypoint dropdown
        if (wpIds.Length > 0)
        {
            string currentId = waypointProp?.stringValue ?? "";
            int selected = Mathf.Max(0, Array.IndexOf(wpIds, currentId));

            int newIdx = EditorGUI.Popup(new Rect(position.x, y, position.width, lineH), "Waypoint", selected, wpIds);
            if (newIdx != selected)
                waypointProp.stringValue = wpIds[newIdx];
        }
        else
        {
            EditorGUI.HelpBox(new Rect(position.x, y, position.width, lineH), "No waypoint database found.", MessageType.Info);
        }
        y += lineH + spacing;

        // Activation zone
        EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineH), zoneProp);
        y += lineH + spacing;

        // Block dropdown
        if (blockIndices.Length > 0)
        {
            int selected = targetBlockProp.intValue;
            int newIdx = EditorGUI.IntPopup(new Rect(position.x, y, position.width, lineH), "Target Block", selected, blockLabels, blockIndices);
            if (newIdx != selected)
                targetBlockProp.intValue = newIdx;
        }
        else
        {
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineH), targetBlockProp);
        }
        y += lineH + spacing;

        // Triggers/Conditions foldout
        _triggerFoldouts[hash] = EditorGUI.Foldout(new Rect(position.x, y, position.width, lineH), _triggerFoldouts[hash], $"Conditions/triggers ({triggerProp.arraySize})", true);
        y += lineH + spacing;

        if (_triggerFoldouts[hash])
        {
            for (int i = 0; i < triggerProp.arraySize; i++)
            {
                var trigger = triggerProp.GetArrayElementAtIndex(i);
                string condId = trigger.FindPropertyRelative("conditionId")?.stringValue ?? $"Trigger {i}";
                int localHash = (property.propertyPath + i).GetHashCode();

                if (!_triggerFoldouts.ContainsKey(localHash))
                    _triggerFoldouts[localHash] = true;

                var expanded = _triggerFoldouts[localHash];

                // Foldout header
                Rect headerRect = new Rect(position.x + 10, y, position.width - 20, lineH);
                expanded = EditorGUI.Foldout(headerRect, expanded, condId, true);
                _triggerFoldouts[localHash] = expanded;
                y += lineH + spacing;

                if (expanded)
                {
                    float triggerH = EditorGUI.GetPropertyHeight(trigger, true);
                    Rect triggerRect = new Rect(position.x + 12, y, position.width - 24, triggerH);

                    // Light debug background
                    EditorGUI.DrawRect(triggerRect, new Color(0.9f, 0.95f, 1f, 0.1f));

                    // Label (optional)
                    // EditorGUI.LabelField(new Rect(triggerRect.x + 4, triggerRect.y, triggerRect.width, lineH), "🧩 Condition Properties");

                    // Now actual drawer
                    var drawer = new TriggerConditionDrawer();
                    drawer.OnGUI(triggerRect, trigger, new GUIContent($"Trigger {i}"));


                    y += triggerH + spacing;



                    Rect delBtn = new Rect(position.x + 10, y, 180, lineH);
                    if (GUI.Button(delBtn, "− Remove Trigger/Condition"))
                    {
                        triggerProp.DeleteArrayElementAtIndex(i);
                        property.serializedObject.ApplyModifiedProperties();
                        break;
                    }
                    y += lineH + spacing;
                }

            }

            // Add trigger button
            Rect addBtn = new Rect(position.x + 10, y, 180, lineH);
            if (GUI.Button(addBtn, "+ Add Trigger/Condition"))
            {
                triggerProp.arraySize++;
            }
            y += lineH + spacing * 0.5f;

        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int hash = property.propertyPath.GetHashCode();
        if (!_bindingFoldouts.TryGetValue(hash, out bool expanded) || !expanded)
            return EditorGUIUtility.singleLineHeight + 4;

        var triggerProp = property.FindPropertyRelative("triggers");
        float h = 6 * (EditorGUIUtility.singleLineHeight + 4);

        if (_triggerFoldouts.TryGetValue(hash, out bool condOpen) && condOpen)
        {
            for (int i = 0; i < triggerProp.arraySize; i++)
            {
                int localHash = (property.propertyPath + i).GetHashCode();
                if (_triggerFoldouts.TryGetValue(localHash, out bool isOpen) && isOpen)
                {
                    h += EditorGUIUtility.singleLineHeight + 4; // Foldout
                    var drawer = new TriggerConditionDrawer();
                    h += drawer.GetPropertyHeight(triggerProp.GetArrayElementAtIndex(i), label) + 4;
                    h += EditorGUIUtility.singleLineHeight + 4; // Remove button
                }
                else
                {
                    h += EditorGUIUtility.singleLineHeight + 4; // Foldout only
                }
            }

            h += EditorGUIUtility.singleLineHeight;
        }

        return h + 24f;
    }
}
#endif
*/

