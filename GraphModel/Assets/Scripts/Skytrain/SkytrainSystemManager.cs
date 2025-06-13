using UnityEngine;

public class SkytrainSystemManager : MonoBehaviour
{
    public SerializableGraph graph;

    [Space(5)]
    [Header("Prefabs")]
    [SerializeField] private GameObject skytrainPrefab;


    private void Awake()
    {
        
    }

    public void Initialize(SerializableGraph graph)
    {
        this.graph = graph;
        CreateSkytrains();
    }


    private void CreateSkytrains()
    {
        foreach (var line in graph.lines)
        {
            foreach (var route in line.routes)
            {
                print(route.routeId);
                GameObject skytrain = Instantiate(skytrainPrefab);
                GraphSkytrain skytrainScript = skytrain.GetComponent<GraphSkytrain>();
                skytrainScript.InitializeSkytrain(line, route.routeId);
            }
        }
    }
}
