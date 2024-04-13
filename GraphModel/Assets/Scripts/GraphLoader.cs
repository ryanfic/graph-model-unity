using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using UnityEngine;

public class GraphLoader : IDisposable
{
    private readonly IDriver _driver;

    public GraphLoader(string uri, string user, string password)
    {
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }

    public async Task<List<T>> GetNodes<T>(LatLngBounds bounds) where T : GraphNode<T>, new()
    {
        string tableName = GraphNode<T>.GetTableName();
        await using IAsyncSession session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(
            async tx => {
                var reader = await tx.RunAsync(
                    $"MATCH (n : {tableName}) " +
                    $"WHERE (n.latitude >= $latMin) AND (n.latitude <= $latMax)" +
                    $"  AND (n.longitude >= $lngMin) AND (n.longitude <= $lngMax)" +
                    $"RETURN n",
                    new
                    {
                        latMin = bounds.LatMin,
                        latMax = bounds.LatMax,
                        lngMin = bounds.LngMin,
                        lngMax = bounds.LngMax
                    }
                );
                var records = await reader.ToListAsync();
                return records.Select(x => GraphNode<T>.FromINode(x["n"].As<INode>())).ToList();
            });
    }

    public void Dispose()
    {
        _driver?.Dispose();
    }
}

public struct LatLngBounds
{
    public float LatMin, LatMax;
    public float LngMin, LngMax;
}
