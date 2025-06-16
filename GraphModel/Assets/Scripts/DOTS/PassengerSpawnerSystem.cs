using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial class PassengerSpawnerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        float3[] stationPositions = new float3[] {
            new float3(0, 0, 0),
            new float3(10, 0, 0),
            new float3(-10, 0, 0),
        };

        for (int i = 0; i < 1000; i++)
        {
            Entity passenger = ecb.Instantiate(/* passenger prefab reference */);

            float3 start = new float3(UnityEngine.Random.Range(-20f, 20f), 0, UnityEngine.Random.Range(-20f, 20f));
            float3 closest = FindClosestStation(start, stationPositions);

            ecb.SetComponent(passenger, LocalTransform.FromPosition(start));
            ecb.AddComponent(passenger, new Destination { Value = closest });
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();

        Enabled = false;
    }

    private float3 FindClosestStation(float3 pos, float3[] stations)
    {
        float minDist = float.MaxValue;
        float3 closest = pos;
        foreach (var s in stations)
        {
            float dist = math.distancesq(pos, s);
            if (dist < minDist)
            {
                minDist = dist;
                closest = s;
            }
        }
        return closest;
    }
}
