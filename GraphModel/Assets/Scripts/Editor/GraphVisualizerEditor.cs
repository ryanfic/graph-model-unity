using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(GraphVisualizer))]
public class GraphVisualizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GraphVisualizer visualizer = (GraphVisualizer)target;
        if (GUILayout.Button("Reload Graph"))
        {
            visualizer.ReloadGraph();
        }
    }
}
