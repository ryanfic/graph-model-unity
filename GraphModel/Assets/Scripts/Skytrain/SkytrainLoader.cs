using Neo4j.Driver;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SkytrainLoader : MonoBehaviour
{
    

    public List<SkytrainStation> skytrainStations;
    public List<SkytrainLine> skytrainLines;

    public float stationScale = 10f;

    [Space(5)]
    [Header("Prefabs")]
    public GameObject skytrainStationPrefab;
    public GameObject skytrainLinePrefab;

    private string database_password = "AjSpeed22!!";
    private GraphVisualizer graphVisualizer;

    private readonly Dictionary<string, Color> SkytrainLineColors = new Dictionary<string, Color>
    {
        { "Canada Line", Color.green },
        { "Expo Line", Color.blue },
        { "Millennium Line", new Color(1.0f, 0.84f, 0.0f) }, // gold/yellow
    };


    private void Start()
    {
        graphVisualizer = FindFirstObjectByType<GraphVisualizer>();
        StartCoroutine(WaitForGraphLoaded());
    }

    /// <summary>
    /// Waits for the first layer of the graph to be loaded, just to prevent any potential errors
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForGraphLoaded()
    {
        yield return new WaitUntil(() => graphVisualizer.GraphLoaded);

        var loadTask = LoadAndReturnNodes<RapidTransit>();
        while (!loadTask.IsCompleted) yield return null;

        var stations = loadTask.Result;
        InitializeStations(stations);

        var lineTask = LoadTransitLines();
        while (!lineTask.IsCompleted) yield return null;
        InitializeLinesFromData(lineTask.Result);
    }

    /// <summary>
    /// Creates the gameobject representation of the skytrain (rapid transit) stations
    /// </summary>
    /// <param name="stations"></param>
    public void InitializeStations(List<RapidTransit> stations)
    {
        skytrainStations = new();
        for (int i = 0; i < stations.Count; i++)
        {
            RapidTransit originalRepresentation = stations[i];

            GameObject worldRepresentation = Instantiate(skytrainStationPrefab);
            worldRepresentation.name = originalRepresentation.stationName;

            worldRepresentation.transform.position = graphVisualizer.ConvertLatLonToWorld(originalRepresentation.latitude, originalRepresentation.longitude);
            worldRepresentation.transform.localScale = new Vector3(stationScale, stationScale, stationScale);

            worldRepresentation.transform.parent = transform;

            SkytrainStation station = worldRepresentation.GetComponent<SkytrainStation>();
            station.InitializeStation(
                originalRepresentation.stationName,
                originalRepresentation.latitude,
                originalRepresentation.longitude,
                worldRepresentation
            );



            skytrainStations.Add(station);
        }
    }

    public void InitializeLinesFromData(List<RapidTransitLine> lines)
    {
        skytrainLines = new();
        foreach (var line in lines)
        {
            var skytrainLine = Instantiate(skytrainLinePrefab);
            skytrainLine.gameObject.name = line.lineName;
            var skytrainLineScript = skytrainLine.GetComponent<SkytrainLine>();
            var renderer = skytrainLine.GetComponent<LineRenderer>();
            var positions = line.geoPoints
                .Select(p => graphVisualizer.ConvertLatLonToWorld(p.x, p.z))  // Assuming lat = x, lon = z
                .ToArray();

            renderer.positionCount = positions.Length;
            renderer.SetPositions(positions);
            renderer.startWidth = renderer.endWidth = 2f;
            renderer.material = new Material(Shader.Find("Sprites/Default"));  // Or whatever you're using
            renderer.startColor = renderer.endColor = SkytrainLineColors[line.lineName];

            skytrainLine.transform.parent = transform;

            var stationDatas = StationDatabase.GetLinesFromStation(line.lineName);

            Dictionary<string, SkytrainStation> stations = new();

            foreach (var data in stationDatas)
            {
                stations[data.Name] = skytrainStations.FirstOrDefault(s => s.stationName == data.Name);
            }

            skytrainLineScript.InitializeLine(
                line.lineName,
                SkytrainLineColors[line.lineName],
                positions.ToList(),
                renderer,
                graphVisualizer,
                stations
            );

            skytrainLines.Add(skytrainLineScript);

        }
    }

    /// <summary>
    /// Loads the nodes from the Neo4j database and returns them (so we can have gameobject representations)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private async Task<List<T>> LoadAndReturnNodes<T>() where T : GraphNode<T>, new()
    {
        GraphLoader loader = new GraphLoader("bolt://localhost:7687", "neo4j", database_password);
        List<T> results = await loader.GetNodes<T>(graphVisualizer.GetCurrentLatLonBounds());
        int expected = await loader.GetTotalCount<T>();
        Debug.Log($"For type {typeof(T)} Expected: {expected}, got: {results.Count}");
        Debug.Log($"Loaded {results.Count} nodes of type {typeof(T)}");
        return results;
    }

    private async Task<List<RapidTransitLine>> LoadTransitLines()
    {
        GraphLoader loader = new GraphLoader("bolt://localhost:7687", "neo4j", database_password);
        List<RapidTransitLine> results = await loader.LoadTransitLinesWithPoints();
        return results;
    }
}


