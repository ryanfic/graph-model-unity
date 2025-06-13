using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableNode
{
    public string id;
    public string lineName;
    public Vector3 position;
    public List<int> routeIds;
    public List<string> connections = new();
}

[System.Serializable]
public class SerializableLine
{
    public string lineName;
    public List<SerializableNode> nodes = new();
    public List<SerializableRoute> routes;
}

[System.Serializable]
public class SerializableGraph
{
    public List<SerializableLine> lines = new();
}

[System.Serializable]
public class SerializableRoute
{
    public string lineName;
    public int routeId;
    public List<string> nodeIds = new();
}

