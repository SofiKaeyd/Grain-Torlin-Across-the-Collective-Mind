#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class DialogGraphWindow : EditorWindow
{
    private DialogGraph _graph;
    private Vector2 _scrollPosition;

    [MenuItem("Tools/Dialog Graph Manager")]
    public static void ShowWindow()
    {
        GetWindow<DialogGraphWindow>("Dialog Graph Manager");
    }

    private void OnEnable()
    {
        RefreshGraphReference();
    }

    private void RefreshGraphReference()
    {
        var guids = AssetDatabase.FindAssets("t:DialogGraph");
        if (guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _graph = AssetDatabase.LoadAssetAtPath<DialogGraph>(path);
        }
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        DrawHeader();
        DrawGraphSelection();

        if (_graph != null)
        {
            DrawControls();
            DrawStatistics();
        }
        else
        {
            DrawNoGraphWarning();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dialog Graph Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();
    }

    private void DrawGraphSelection()
    {
        EditorGUILayout.BeginHorizontal();
        {
            _graph = (DialogGraph)EditorGUILayout.ObjectField("Dialog Graph", _graph, typeof(DialogGraph), false);
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
            {
                RefreshGraphReference();
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    private void DrawControls()
    {
        EditorGUILayout.HelpBox("This parser works using WebClient.", MessageType.Info);
        EditorGUILayout.Space();
        if (GUILayout.Button("Load from Google Sheets", GUILayout.Height(30)))
        {
            LoadFromGoogleSheets();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Clear Data", GUILayout.Height(25)))
        {
            ClearData();
        }

        EditorGUILayout.Space(15);
    }

    private void DrawStatistics()
    {
        EditorGUILayout.LabelField("Statistics:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Loaded Nodes: {_graph.Nodes.Count}");
    }

    private void DrawNoGraphWarning()
    {
        EditorGUILayout.HelpBox("No Dialog Graph asset found in project. Please create or assign one.", MessageType.Warning);

        EditorGUILayout.Space();
        if (GUILayout.Button("Create New Dialog Graph"))
        {
            CreateNewDialogGraph();
        }
    }

    private void LoadFromGoogleSheets()
    {
        var success = DialogParser.LoadAndParseFromGoogleSheets();
        if (success)
        {
            _graph.Set(DialogParser.DialogGraph);
            EditorUtility.DisplayDialog("Success", $"Successfully loaded {_graph.Nodes.Count} dialog nodes", "OK");
            EditorUtility.SetDirty(_graph);
            AssetDatabase.SaveAssets();
            Repaint();
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to load dialog data from Google Sheets", "OK");
        }
    }

    private void ClearData()
    {
        if (EditorUtility.DisplayDialog("Clear Data",
            "Are you sure you want to clear all dialog data?", "Yes", "No"))
        {
            _graph.ClearData();
            EditorUtility.SetDirty(_graph);
            AssetDatabase.SaveAssets();
            Repaint();
        }
    }

    private void CreateNewDialogGraph()
    {
        var newGraph = CreateInstance<DialogGraph>();

        var path = EditorUtility.SaveFilePanelInProject(
            "Create Dialog Graph",
            "DialogGraph",
            "asset",
            "Save Dialog Graph");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newGraph, path);
            AssetDatabase.SaveAssets();
            _graph = newGraph;
            Repaint();
        }
    }
}
#endif
