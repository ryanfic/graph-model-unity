using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

public class RapidTransitRouteEditor : EditorWindow
{
    private List<RapidTransitNode> routeNodes = new();
    private int routeId = 0;
    private Vector2 scrollPos;

    [MenuItem("Tools/Rapid Transit/Route Editor")]
    public static void ShowWindow()
    {
        GetWindow<RapidTransitRouteEditor>("Transit Route Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Route Builder", EditorStyles.boldLabel);

        routeId = EditorGUILayout.IntField("Route ID", routeId);

        if (GUILayout.Button("Add Selected Node(s)"))
        {
            foreach (var go in Selection.gameObjects)
            {
                var node = go.GetComponent<RapidTransitNode>();
                if (node != null && !routeNodes.Contains(node))
                {
                    if (routeNodes.Count > 0)
                    {
                        var expectedLine = routeNodes[0].lineName;
                        if (node.lineName != expectedLine)
                        {
                            Debug.LogWarning($"Node {node.name} is on a different line ({node.lineName}) than expected ({expectedLine}). Skipping.");
                            continue;
                        }
                    }

                    routeNodes.Add(node);
                }
            }
        }

        if (GUILayout.Button("Clear Route"))
        {
            routeNodes.Clear();
        }

        GUILayout.Space(10);

        if (routeNodes.Count > 0)
        {
            string detectedLine = routeNodes[0].lineName;
            GUILayout.Label($"Line Name: {detectedLine}", EditorStyles.boldLabel);
        }

        GUILayout.Label("Current Route:");
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        for (int i = 0; i < routeNodes.Count; i++)
        {
            GUILayout.Label($"{i + 1}. {routeNodes[i].name}");
        }
        GUILayout.EndScrollView();

        GUILayout.Space(10);

        if (GUILayout.Button("Assign Route ID to Nodes"))
        {
            if (routeNodes.Count == 0)
            {
                Debug.LogWarning("No nodes in the route.");
                return;
            }

            string detectedLine = routeNodes[0].lineName;

            foreach (var node in routeNodes)
            {
                Undo.RecordObject(node, "Assign Route ID");

                if (!node.routeIds.Contains(routeId))
                    node.routeIds.Add(routeId);

                node.lineName = detectedLine;

                EditorUtility.SetDirty(node);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"Assigned route ID {routeId} to {routeNodes.Count} nodes on line '{detectedLine}'.");
        }

        if (GUILayout.Button("Remove Route ID from Selected Node(s)"))
        {
            foreach (var go in Selection.gameObjects)
            {
                var node = go.GetComponent<RapidTransitNode>();
                if (node != null && node.routeIds.Contains(routeId))
                {
                    Undo.RecordObject(node, "Remove Route ID");
                    node.routeIds.Remove(routeId);
                    EditorUtility.SetDirty(node);
                }
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"Removed route ID {routeId} from selected nodes.");
        }
    }
}
