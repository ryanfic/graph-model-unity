using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;

public class SkytrainLine : MonoBehaviour
{
    public string lineName;
    public Color color;
    public List<Vector3> points;
    public LineRenderer lineRenderer;
    public GameObject SkytrainPrefab;
    public GraphVisualizer graphVisualizer;

    public GameObject editableNodePrefab;

    public Dictionary<string, SkytrainStation> stations;

    public void InitializeLine(string lineName, Color color, List<Vector3> shape, LineRenderer lineRenderer, GraphVisualizer graphVisualizer, Dictionary<string, SkytrainStation> stations)
    {
        this.lineName = lineName;
        name = lineName;
        this.color = color;
        this.points = shape;
        this.lineRenderer = lineRenderer;
        this.graphVisualizer = graphVisualizer;
        this.stations = stations;

        if (lineName == "Canada Line")
            InitializeSkytrains(1);
    }

    public void InitializeLine(string lineName, Color color, List<Vector3> shape, GraphVisualizer graphVisualizer, Dictionary<string, SkytrainStation> stations)
    {
        name = lineName;
        this.lineName = lineName;
        this.color = color;
        this.points = shape;
        this.graphVisualizer = graphVisualizer;
        this.stations = stations;

        RapidTransitNode previousNode = null;
        for (int i = 0; i < shape.Count; i++)
        {
            var node = Instantiate(editableNodePrefab);
            RapidTransitNode nodeScript = node.GetComponent<RapidTransitNode>();
            nodeScript.lineName = lineName;
            node.transform.position = shape[i] + new Vector3(0, 0f, 0);

            if (previousNode != null)
            {
                nodeScript.connections.Add(previousNode);
            }
            previousNode = nodeScript;
        }

        if (lineName == "Canada Line")
            InitializeSkytrains(1);
    }

    public void InitializeSkytrains(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var skytrain = Instantiate(SkytrainPrefab);
            var skytrainScript = skytrain.GetComponent<Skytrain>();
            skytrainScript.InitializeSkytrain(
                this,
                $"Skytrain {i}",
                points[i],
                graphVisualizer.ConvertLatLonToWorld
                );
        }
    }

}
