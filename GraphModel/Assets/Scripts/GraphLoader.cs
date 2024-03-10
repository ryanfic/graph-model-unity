using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

public class GraphLoader : IDisposable
{
    private readonly IDriver _driver;

    public GraphLoader(string uri, string user, string password)
    {
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        CheckConn();
    }

    async void CheckConn()
    {
        await _driver.VerifyConnectivityAsync();
    }

    public async Task<List<int>> Test()
    {
        await using IAsyncSession session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(
            async tx =>
            {
                List<int> ids = new List<int>();
                var reader = await tx.RunAsync(
                    "Match (j:Junction) LIMIT 10 RETURN j.id");

                while (await reader.FetchAsync())
                {
                    ids.Add(int.Parse(reader.Current[0].ToString()));
                }

                return ids;
            });
    }

    public void Dispose()
    {
        _driver?.Dispose();
    }
}
