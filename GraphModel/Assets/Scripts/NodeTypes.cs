using Neo4j.Driver;
using System.Collections.Generic;
using UnityEngine;

public abstract class GraphNode<T> where T : GraphNode<T>, new()
{
    public int id;
    public float latitude;
    public float longitude;

    public static T FromINode(INode record)
    {
        T t = new T();
        t.InitializeFromINode(record);
        return t;
    }

    public static string GetTableName()
    {
        if (typeof(T) == typeof(Junction)) return "Junction";
        if (typeof(T) == typeof(Tree)) return "Tree";
        if (typeof(T) == typeof(Business)) return "Business";
        if (typeof(T) == typeof(Crime)) return "Crime";
        if (typeof(T) == typeof(RapidTransit)) return "RapidTransit";
        if (typeof(T) == typeof(School)) return "School";
        if (typeof(T) == typeof(Store)) return "Store";
        if (typeof(T) == typeof(Transit)) return "Transit";
        Debug.LogError("Invalid Node Type");
        return "";
    }

    protected virtual void InitializeFromINode(INode record)
    {
        id = record["id"].As<int>();
        latitude = record["latitude"].As<float>();
        longitude = record["longitude"].As<float>();
    }
}

public class Junction : GraphNode<Junction>
{
    public int crimeCount;
    public float crimeReach;

    protected override void InitializeFromINode(INode record)
    {
        base.InitializeFromINode(record);
        crimeCount = record["crime_count"].As<int>();
        crimeReach = record["crime_reach"].As<float>();
    }
}

public class Tree : GraphNode<Tree> { }

public class Business : GraphNode<Business> { }

public class Crime : GraphNode<Crime> { }

public class RapidTransit : GraphNode<RapidTransit> 
{
    public string stationName;

    protected override void InitializeFromINode(INode record)
    {
        base.InitializeFromINode(record);
        stationName = record["name"].As<string>();
    }
}

public class School : GraphNode<School> { }

public class Store : GraphNode<Store> { }

public class Transit : GraphNode<Transit> { }

public class RapidTransitLine : GraphNode<RapidTransitLine>
{
    public string lineName;
    public List<Vector3> geoPoints = new List<Vector3>();

    protected override void InitializeFromINode(INode record)
    {
        base.InitializeFromINode(record);
        lineName = record["name"].As<string>();
    }
}
