using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
public struct FollowTarget : IComponentData
{
    public Entity TargetEntity;
    public float Speed;
}

public class FollowAuthoring : MonoBehaviour
{
    public GameObject Target;
    public float Speed;
}

public class FollowAuthoringBaker : Baker<FollowAuthoring>
{
    public override void Bake(FollowAuthoring authoring)
    {
        var comp = new FollowTarget
        {
            TargetEntity = GetEntity(authoring.Target),
            Speed = authoring.Speed
        };
        AddComponent(comp);
    }
}

public partial struct FollowSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (follow, transform) in SystemAPI.Query<RefRO<FollowTarget>, RefRW<LocalTransform>>())
        {
            var targetPos = SystemAPI.GetComponent<LocalTransform>(follow.ValueRO.TargetEntity).Position;
            var currentPos = transform.ValueRO.Position;
            var direction = math.normalize(targetPos - currentPos);
            transform.ValueRW.Position = currentPos + direction * follow.ValueRO.Speed * SystemAPI.Time.DeltaTime;
        }
    }
}