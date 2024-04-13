using System.Collections.Generic;
using UnityEngine;
using GD.MinMaxSlider;
using System.Threading.Tasks;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class GraphVisualizer : MonoBehaviour
{
    [SerializeField, MinMaxSlider(0, 1)]
    Vector2 LatitudeBound;

    [SerializeField, MinMaxSlider(0, 1)]
    Vector2 LongitudeBound;

    [SerializeField]
    float ObjectScale = 0.0003f;
    [SerializeField]
    float PositionScale = 100;
    [SerializeField]
    NodeVisuals nodeVisuals;

    // Bounds of Vancouver
    float graph_lat_min = 49.20089037f;
    float graph_lat_max = 49.29449928f;
    float graph_lng_min = -123.2247581f;
    float graph_lng_max = -123.0233095f;

    Transform graphParent;
    LatLngBounds b;

    string database_password = "password";

    // Start is called before the first frame update
    void Start()
    {
        ReloadGraph();
    }

    public async void ReloadGraph()
    {
        if (graphParent != null) Destroy(graphParent.gameObject);

        graphParent = new GameObject("Graph").transform;

        UpdateLatLngBounds();

        if (nodeVisuals.junction.enabled) await LoadNodes<Junction>(nodeVisuals.junction);
        if (nodeVisuals.tree.enabled) await LoadNodes<Tree>(nodeVisuals.tree);
        if (nodeVisuals.business.enabled) await LoadNodes<Business>(nodeVisuals.business);
        if (nodeVisuals.store.enabled) await LoadNodes<Store>(nodeVisuals.store);
        if (nodeVisuals.transit.enabled) await LoadNodes<Transit>(nodeVisuals.transit);
        if (nodeVisuals.rapidTransit.enabled) await LoadNodes<RapidTransit>(nodeVisuals.rapidTransit);
        if (nodeVisuals.crime.enabled) await LoadNodes<Crime>(nodeVisuals.crime);
        if (nodeVisuals.school.enabled) await LoadNodes<School>(nodeVisuals.school);

    }

    private void UpdateLatLngBounds()
    {
        float graph_lat_range = graph_lat_max - graph_lat_min;
        float graph_lng_range = graph_lng_max - graph_lng_min;
        b.LatMin = graph_lat_min + graph_lat_range * LatitudeBound.x;
        b.LatMax = graph_lat_min + graph_lat_range * LatitudeBound.y;
        b.LngMin = graph_lng_min + graph_lng_range * LongitudeBound.x;
        b.LngMax = graph_lng_min + graph_lng_range * LongitudeBound.y;
    }

    private async Task LoadNodes<T>(NodeVisual visual) where T : GraphNode<T>, new()
    {
        GraphLoader loader = new GraphLoader("bolt://localhost:7687", "neo4j", database_password);
        List<T> results = await loader.GetNodes<T>(b);
        CreateResultObjects(results, visual);
    }

    private void CreateResultObjects<T>(List<T> nodes, NodeVisual visual) where T : GraphNode<T>, new() 
    {
        float lat_range = b.LatMax - b.LatMin;
        float lng_range = b.LngMax - b.LngMin;


        foreach (T t in nodes)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.transform.SetParent(graphParent);
            g.transform.position = new Vector3(
                t.longitude - b.LngMin - lng_range / 2,
                visual.offsetY / PositionScale,
                t.latitude - b.LatMin - lat_range / 2
            ) * PositionScale;
            g.transform.localScale = Vector3.one * ObjectScale * visual.scale;
            g.GetComponent<Renderer>().material.color = visual.color;
        }
    }
}

[Serializable]
public struct NodeVisuals
{
    public NodeVisual junction;
    public NodeVisual tree;
    public NodeVisual business;
    public NodeVisual store;
    public NodeVisual transit;
    public NodeVisual rapidTransit;
    public NodeVisual crime;
    public NodeVisual school;
}

[Serializable]
public struct NodeVisual
{
    public bool enabled;
    public float scale;
    public float offsetY;
    public Color color;
}
