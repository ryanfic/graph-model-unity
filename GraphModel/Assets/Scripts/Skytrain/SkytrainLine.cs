using System.Collections.Generic;
using UnityEngine;

public class SkytrainLine : MonoBehaviour
{
    public string lineName;
    public Color color;
    public List<Vector3> shape;
    public LineRenderer lineRenderer;
    public GameObject SkytrainPrefab;
    public GraphVisualizer graphVisualizer;

    public Dictionary<string, SkytrainStation> stations;

    public void InitializeLine(string lineName, Color color, List<Vector3> shape, LineRenderer lineRenderer, GraphVisualizer graphVisualizer, Dictionary<string, SkytrainStation> stations)
    {
        this.lineName = lineName;
        this.color = color;
        this.shape = shape;
        this.lineRenderer = lineRenderer;
        this.graphVisualizer = graphVisualizer;
        this.stations = stations;
        print(stations.Count);

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
                shape[i],
                graphVisualizer.ConvertLatLonToWorld
                );
        }
    }



}
