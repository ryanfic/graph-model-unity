using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct LoadingZoneFollowSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LoadingZone>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var entityManager = state.EntityManager;

        foreach (var (follow, transform, entity) in
                 SystemAPI.Query<RefRO<LoadingZone>, RefRW<LocalTransform>>()
                          .WithEntityAccess())
        {
            if (!entityManager.HasComponent<LocalToWorld>(follow.ValueRO.target))
                continue;

            var targetTransform = entityManager.GetComponentData<LocalToWorld>(follow.ValueRO.target);

            float3 targetPosition = targetTransform.Position;
            quaternion targetRotation = targetTransform.Rotation;

            float3 finalPosition = targetPosition;

            if (SystemAPI.HasComponent<OffsetFromTarget>(entity))
            {
                var offsetData = SystemAPI.GetComponent<OffsetFromTarget>(entity);
                float3 rotatedOffset = math.rotate(targetRotation, offsetData.offset);
                finalPosition += rotatedOffset;
            }

            transform.ValueRW.Position = finalPosition;
            transform.ValueRW.Rotation = targetRotation;
        }
    }
}
