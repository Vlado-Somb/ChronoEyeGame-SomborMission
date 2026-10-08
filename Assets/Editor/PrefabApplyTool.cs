#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

static class SerializedPropertyIterator
{
    public static IEnumerable<SerializedProperty> Enumerate(this SerializedProperty root)
    {
        var copy = root.Copy();
        var end = copy.GetEndProperty();
        while (copy.NextVisible(true) && !SerializedProperty.EqualContents(copy, end))
            yield return copy;
    }
}
public static class PrefabApplyTool
{

    [MenuItem("Tools/Prefabs/👁 Preview All Modified Prefabs (Dry-run)", false, 100)]
    public static void PreviewAllModifiedPrefabs()
    {
        int count = 0;

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(go))
                continue;

            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(go);

            if (!PrefabUtility.HasPrefabInstanceAnyOverrides(root, true))
                continue;

            count++;

            Debug.Log($"🔍 Prefab: <b>{root.name}</b> would be applied.", root);

            // Pregled override tipova
            var addedObjects = PrefabUtility.GetAddedGameObjects(root);
            foreach (var added in addedObjects)
            {
                Debug.Log($"  ➕ Would add GameObject: {added.instanceGameObject.name}");
            }

            var removedComponents = PrefabUtility.GetRemovedComponents(root);
            foreach (var removed in removedComponents)
            {
                Debug.Log($"  ➖ Would remove Component: {removed.assetComponent.GetType().Name} from {removed.assetComponent.gameObject.name}");
            }

            var objectOverrides = PrefabUtility.GetObjectOverrides(root);
            foreach (var change in objectOverrides)
            {
                Debug.Log($"  ✏️ Would modify: {change.instanceObject.name} ({change.instanceObject.GetType().Name})");
            }

            var addedComponents = PrefabUtility.GetAddedComponents(root);
            foreach (var comp in addedComponents)
            {
                var compType = comp.GetType();
                var field = compType.GetField("addedComponent", BindingFlags.Instance | BindingFlags.NonPublic);
                Component added = field?.GetValue(comp) as Component;

                string name = "(unknown GameObject)";
                string typeName = "(unknown Component)";

                if (added != null)
                {
                    name = added.gameObject.name;
                    typeName = added.GetType().Name;
                }

                Debug.Log($"  ➕ Would add Component: {typeName} to GameObject: {name}");
            }

        }

        if (count == 0)
            Debug.Log("<color=grey>✅ No prefab overrides detected in scene.</color>");
        else
            Debug.Log($"<color=orange>👁 Preview complete.</color> Found <b>{count}</b> prefab(s) with unapplied changes.");
    }

    [MenuItem("Tools/Prefabs/📝 Export Modified Prefabs to TXT", false, 110)]
    public static void ExportPrefabOverridesToTxt()
    {
        HashSet<GameObject> seen = new HashSet<GameObject>();
        string logPath = Application.dataPath + "/EditorLogs/ModifiedPrefabReport.txt";
        using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logPath, false))
        {
            int count = 0;

            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {

                if (!PrefabUtility.IsPartOfPrefabInstance(go))
                    continue;

                GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(go);
                if (!PrefabUtility.HasPrefabInstanceAnyOverrides(root, true))
                    continue;

                if (seen.Contains(root))
                    continue;
                seen.Add(root);

                count++;
                writer.WriteLine($"🔧 Prefab: {root.name}");

                // --- Added GameObjects ---
                var added = PrefabUtility.GetAddedGameObjects(root);
                foreach (var add in added)
                    writer.WriteLine($"    ➕ Added GameObject: {add.instanceGameObject.name}");

                // --- Removed Components ---
                var removed = PrefabUtility.GetRemovedComponents(root);
                foreach (var rem in removed)
                    writer.WriteLine($"    ➖ Removed Component: {rem.assetComponent.GetType().Name} on {rem.assetComponent.gameObject.name}");

                // --- Added Components (with reflection) ---
                var addedComps = PrefabUtility.GetAddedComponents(root);
                foreach (var comp in addedComps)
                {
                    var field = comp.GetType().GetField("addedComponent", BindingFlags.Instance | BindingFlags.NonPublic);
                    Component addedComp = field?.GetValue(comp) as Component;
                    string compType = addedComp != null ? addedComp.GetType().Name : "(unknown type)";
                    string goName = addedComp != null && addedComp.gameObject != null ? addedComp.gameObject.name : "(unknown object)";
                    writer.WriteLine($"    ➕ Added Component: {compType} to GameObject: {goName}");

                }

                // --- Modified Properties ---
                var modified = PrefabUtility.GetObjectOverrides(root);
                foreach (var mod in modified)
                {
                    var so = new SerializedObject(mod.instanceObject);
                    SerializedProperty iterator = so.GetIterator();
                    if (iterator.NextVisible(true))
                    {
                        do
                        {
                            if (iterator.prefabOverride)
                            {
                                writer.WriteLine($"        • {iterator.name} = {GetSerializedValueAsString(iterator)}");
                            }
                        }
                        while (iterator.NextVisible(true));
                    }

                }

                writer.WriteLine();
            }

            if (count == 0)
                writer.WriteLine("✅ No prefab overrides found.");
            else
                writer.WriteLine($"🗂 Done. {count} prefab(s) with modifications exported.");

            writer.Flush();
        }

        Debug.Log($"📝 Exported prefab override report to: {logPath}");
        EditorUtility.RevealInFinder(logPath);
    }

    private static string GetSerializedValueAsString(SerializedProperty prop)
    {
        return prop.propertyType switch
        {
            SerializedPropertyType.String => prop.stringValue,
            SerializedPropertyType.Boolean => prop.boolValue.ToString(),
            SerializedPropertyType.Integer => prop.intValue.ToString(),
            SerializedPropertyType.Float => prop.floatValue.ToString("0.###"),
            SerializedPropertyType.Enum => prop.enumDisplayNames[prop.enumValueIndex],
            SerializedPropertyType.ObjectReference => prop.objectReferenceValue != null
                ? prop.objectReferenceValue.name
                : "(null)",
            _ => "(unsupported type)"
        };
    }
    [MenuItem("Tools/Prefabs/🛡 Apply All Modified Prefabs (Undo-Safe)", false, 120)]
    public static void ApplyAllModifiedPrefabs()
    {
        // ❗ Popup potvrda — ako user klikne Cancel, odmah izlazimo
        if (!EditorUtility.DisplayDialog("Apply All Prefabs",
            "This will apply ALL prefab modifications in the scene. " +
            "This action can be undone via CTRL+Z. Are you sure?",
            "Yes, Apply Prefabs", "Cancel"))
            return;

        int count = 0;
        HashSet<GameObject> seen = new(); // da ne obradimo isti prefab više puta

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(go))
                continue;

            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(go);

            if (!seen.Add(root)) // ako je već viđen
                continue;

            if (!PrefabUtility.HasPrefabInstanceAnyOverrides(root, false))
                continue;

            Undo.RegisterFullObjectHierarchyUndo(root, $"Apply {root.name}");
            PrefabUtility.ApplyPrefabInstance(root, InteractionMode.UserAction);
            Debug.Log($"✅ Applied prefab changes: {root.name}", root);
            count++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"<color=green>🛡 Done. Applied {count} prefab(s) with Undo support.</color>");
    }

}

#endif
