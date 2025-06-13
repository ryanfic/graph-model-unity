using System.Collections.Generic;
using UnityEngine;

public class GraphSkytrainLine : MonoBehaviour
{
    public string lineName;
    public Color color;
    public List<RapidTransitNode> nodes;
    public GameObject SkytrainPrefab;
    public GraphVisualizer graphVisualizer;
    public Dictionary<string, SkytrainStation> stations;


    public GameObject rapidTransitNodePrefab;


    public void InitializeLine(string lineName, Color color, List<Vector3> shape, GraphVisualizer graphVisualizer, Dictionary<string, SkytrainStation> stations)
    {
        name = lineName;
        this.lineName = lineName;
        this.color = color;
        nodes = new();
        this.graphVisualizer = graphVisualizer;
        this.stations = stations;

        RapidTransitNode previousNode = null;
        for (int i = 0; i < shape.Count; i++)
        {
            var node = Instantiate(rapidTransitNodePrefab);
            RapidTransitNode nodeScript = node.GetComponent<RapidTransitNode>();
            nodeScript.lineName = lineName;
            node.transform.position = shape[i] + new Vector3(0, 0f, 0);
            nodeScript.position = node.transform.position;

            if (previousNode != null)
            {
                nodeScript.connections.Add(previousNode);
            }
            previousNode = nodeScript;
            nodes.Add(nodeScript);
        }

    }
}
