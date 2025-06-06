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
            var positionsList = line.geoPoints.Select(p => graphVisualizer.ConvertLatLonToWorld(p.x, p.z)).ToList(); // Assuming lat = x, lon = z 
            
            // this inst working the best yet. dont sort if you just want to see vancouver
            // will need to make tis better to incorporate multiple cities
            var positions = SortPointsGreedy(positionsList).ToArray(); 

            renderer.positionCount = positions.Length;
            renderer.SetPositions(positions);
            renderer.startWidth = renderer.endWidth = 2f;
            renderer.material = new Material(Shader.Find("Sprites/Default")); 
            renderer.startColor = renderer.endColor = SkytrainLineColors[line.lineName];

            skytrainLine.transform.parent = transform;

            var stationDatas = StationDatabase.GetLinesFromStation(line.lineName);

            Dictionary<string, SkytrainStation> stations = new();

            foreach (var data in stationDatas)
            {
                var matchedStation = skytrainStations.FirstOrDefault(s => s.stationName == data.Name);
                stations[data.Name] = matchedStation;
            }


            skytrainLineScript.InitializeLine(
                line.lineName,
                SkytrainLineColors[line.lineName],
                positionsList,
                renderer,
                graphVisualizer,
                stations
            );

            skytrainLines.Add(skytrainLineScript);

        }
    }

    public static List<Vector3> SortPointsGreedy(List<Vector3> points)
    {
        if (points == null || points.Count <= 1)
            return new List<Vector3>(points);

        const float anglePenaltyWeight = 0.02f;
        const float maxSqrDistance = 100000f; // increase if your points are farther apart

        List<Vector3> sorted = new();
        HashSet<int> visited = new();

        // Start at southeast-most point (min z, then min x)
        int startIndex = 0;
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].z < points[startIndex].z ||
                (Mathf.Approximately(points[i].z, points[startIndex].z) && points[i].x < points[startIndex].x))
            {
                startIndex = i;
            }
        }

        sorted.Add(points[startIndex]);
        visited.Add(startIndex);

        while (visited.Count < points.Count)
        {
            Vector3 current = sorted[^1];

            float bestScore = float.MaxValue;
            int bestIndex = -1;

            Vector3 lastDir = sorted.Count >= 2
                ? (sorted[^1] - sorted[^2]).normalized
                : Vector3.zero;

            for (int i = 0; i < points.Count; i++)
            {
                if (visited.Contains(i)) continue;

                Vector3 candidate = points[i];
                float dist = Vector3.SqrMagnitude(candidate - current);
                if (dist > maxSqrDistance) continue;

                float anglePenalty = 0f;
                if (lastDir != Vector3.zero && (candidate - current).sqrMagnitude > 0.001f)
                {
                    Vector3 dir = (candidate - current).normalized;
                    float angle = Vector3.Angle(lastDir, dir);
                    anglePenalty = angle * anglePenaltyWeight;
                }

                float score = dist + anglePenalty;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            if (bestIndex == -1)
                break; // disconnected points?

            sorted.Add(points[bestIndex]);
            visited.Add(bestIndex);
        }

        return sorted;
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


