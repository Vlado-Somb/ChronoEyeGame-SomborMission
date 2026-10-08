using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class MissingScriptScanner : EditorWindow
{
    private static readonly List<string> _logLines = new();

    [MenuItem("Tools/Diagnostics/Scan for Missing Scripts")]
    private static void OpenWindow()
    {
        GetWindow<MissingScriptScanner>("Missing Script Scanner").Show();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("🔍 Scan Prefabs & Scenes"))
        {
            _logLines.Clear();
            ScanAllPrefabs();
            ScanCurrentScene();
            SaveReport();
        }
    }

    private static void ScanCurrentScene()
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            CheckGameObject(root, SceneManager.GetActiveScene().name);
        }
    }

    private static void ScanAllPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                CheckGameObject(prefab, path);
        }
    }

    private static void CheckGameObject(GameObject go, string context)
    {
        var components = go.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                string fullPath = GetFullPath(go);
                string log = $"❌ Missing script on [{context}] GameObject: {fullPath}, Component index: {i}";
                Debug.LogWarning(log, go);
                _logLines.Add(log);
            }
        }

        foreach (Transform child in go.transform)
            CheckGameObject(child.gameObject, context);
    }

    private static string GetFullPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }

    private static void SaveReport()
    {
        if (_logLines.Count == 0)
        {
            EditorUtility.DisplayDialog("Scan Complete", "✅ No missing scripts found!", "OK");
            return;
        }

        string folder = "Assets/EditorLogs";
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, $"MissingScripts_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
        File.WriteAllLines(path, _logLines);

        AssetDatabase.Refresh();
        EditorUtility.RevealInFinder(path);
        EditorUtility.DisplayDialog("Scan Complete", $"Found {_logLines.Count} issues.\nLog saved to:\n{path}", "Open Log");
    }
}
