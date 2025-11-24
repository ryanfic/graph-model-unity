using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
public class Entity_Collider_Parent_Mono : MonoBehaviour
{
    public GameObject _colliderObject;
    public float3 _colliderPosition;
}

public class Entity_Collider_Parent_Baker : Baker<Entity_Collider_Parent_Mono>
{
    public override void Bake(Entity_Collider_Parent_Mono authoring)
    {
        AddComponent(new ColliderParentProperties
        {
            _colliderPosition = authoring._colliderPosition,
            _colliderObject = GetEntity(authoring._colliderObject)
        });
    }
}
