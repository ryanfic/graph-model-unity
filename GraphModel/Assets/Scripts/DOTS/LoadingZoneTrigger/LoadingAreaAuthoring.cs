using Unity.Entities;
using Unity.Mathematics;

public struct LoadingArea : IComponentData
{
    public float3 OffsetFromSkytrain;
    public float Radius;
}

[UnityEngine.DisallowMultipleComponent]
public class LoadingAreaAuthoring : UnityEngine.MonoBehaviour
{
    public UnityEngine.Vector3 offset;
    public float radius;

    class Baker : Baker<LoadingAreaAuthoring>
    {
        public override void Bake(LoadingAreaAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new LoadingArea
            {
                OffsetFromSkytrain = authoring.offset,
                Radius = authoring.radius
            });
        }
    }
}