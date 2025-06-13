using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RapidTransitEditor : EditorWindow
{
    [MenuItem("Tools/Rapid Transit/Connection Editor")]
    public static void ShowWindow()
    {
        GetWindow<RapidTransitEditor>("Transit Graph Editor");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Fix All Connections"))
        {
            var allNodes = FindObjectsByType<RapidTransitNode>(FindObjectsSortMode.InstanceID);
            int fixedCount = 0;

            foreach (var node in allNodes)
            {
                foreach (var connectedNode in node.connections)
                {
                    if (connectedNode == null)
                        continue;

                    // Make sure connection is bidirectional
                    if (!connectedNode.connections.Contains(node))
                    {
                        Undo.RecordObject(connectedNode, "Fix Connection");
                        connectedNode.connections.Add(node);
                        EditorUtility.SetDirty(connectedNode);
                        fixedCount++;
                    }
                }

                // Mark node dirty in case of null cleanup
                EditorUtility.SetDirty(node);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[TransitEditor] Fixed {fixedCount} one-way connection(s).");
        }

        var selection = Selection.gameObjects;

        GUILayout.Label("Selected Nodes: " + selection.Length);

        if (selection.Length != 2)
        {
            EditorGUILayout.HelpBox("Select exactly 2 nodes to connect/disconnect.", MessageType.Info);
            return;
        }

        if (!ValidateSelection(selection, out RapidTransitNode nodeA, out RapidTransitNode nodeB))
            return;

        if (GUILayout.Button("Create Connection"))
        {
            Undo.RecordObject(nodeA, "Add Connection");
            Undo.RecordObject(nodeB, "Add Connection");

            if (!nodeA.connections.Contains(nodeB))
                nodeA.connections.Add(nodeB);
            if (!nodeB.connections.Contains(nodeA))
                nodeB.connections.Add(nodeA);

            EditorUtility.SetDirty(nodeA);
            EditorUtility.SetDirty(nodeB);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
        }

        if (GUILayout.Button("Destroy Connection"))
        {
            Undo.RecordObject(nodeA, "Remove Connection");
            Undo.RecordObject(nodeB, "Remove Connection");

            nodeA.connections.Remove(nodeB);
            nodeB.connections.Remove(nodeA);

            EditorUtility.SetDirty(nodeA);
            EditorUtility.SetDirty(nodeB);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
        }

        

    }

    private bool ValidateSelection(GameObject[] selection, out RapidTransitNode nodeA, out RapidTransitNode nodeB)
    {
        nodeA = selection[0].GetComponent<RapidTransitNode>();
        nodeB = selection[1].GetComponent<RapidTransitNode>();

        if (nodeA == null || nodeB == null)
        {
            EditorGUILayout.HelpBox("Both objects must have a RapidTransitNode component.", MessageType.Error);
            return false;
        }

        return true;
    }
}
