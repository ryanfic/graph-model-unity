using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class RapidTransitNode : MonoBehaviour
{
    public string id;
    public string lineName;
    public Vector3 position;
    public List<RapidTransitNode> connections = new();
    public Material lineMaterial;

    public List<int> routeIds = new();

    public GameObject linePrefab;


    private void OnDrawGizmos()
    {
        if (connections == null || lineName == null) return;
        Gizmos.color = SkytrainSystemManager.GetLineColor(lineName);
        foreach (var connection in connections)
        {
            if (connection != null)
            {
                Gizmos.DrawLine(transform.position, connection.gameObject.transform.position);
            }
        }
    }

}
