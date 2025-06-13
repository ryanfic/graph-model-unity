using System.Collections;
using System.Linq;
using UnityEngine;

public class SkytrainSystemManager : MonoBehaviour
{
    public SerializableGraph graph;

    [Space(5)]
    [Header("Prefabs")]
    [SerializeField] private GameObject skytrainPrefab;

    private float t = 0;

    private float oringialThreshold = 5f;
    private float currrentThreshold;
    private void Awake()
    {
        currrentThreshold = oringialThreshold;
    }

    public void Initialize(SerializableGraph graph)
    {
        this.graph = graph;
        CreateSkytrains();
    }

    private void Update()
    {
        t += Time.deltaTime;

        if (t < 200 && t > currrentThreshold)
        {
            CreateSkytrains();
            currrentThreshold += oringialThreshold;
        }
    }


    private void CreateSkytrains()
    {
        foreach (var line in graph.lines)
        {
            foreach (var route in line.routes)
            {
                GameObject skytrain = Instantiate(skytrainPrefab);
                GraphSkytrain skytrainScript = skytrain.GetComponent<GraphSkytrain>();
                skytrainScript.InitializeSkytrain(line, route.routeId);
            }
        }
    }
}
