using Unity.Entities;
using Unity.Mathematics;

public struct ColliderParentProperties : IComponentData
{
    public Entity _colliderObject;
    public float3 _colliderPosition;
}
