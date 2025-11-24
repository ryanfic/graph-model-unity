using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class LoadingZoneAuthoring : MonoBehaviour
{
    public GameObject target;
    public float offsetDistance = 2f;
    public GameObject triggerCubePrefab;

    class Baker : Baker<LoadingZoneAuthoring>
    {
        public override void Bake(LoadingZoneAuthoring authoring)
        {
            var mainEntity = GetEntity(TransformUsageFlags.Dynamic);
            var targetEntity = GetEntity(authoring.target, TransformUsageFlags.Dynamic);

            AddComponent(mainEntity, new LoadingZone
            {
                target = targetEntity
            });

            AddComponent(mainEntity, new TriggerOffset
            {
                offset = authoring.offsetDistance
            });

            // Create left trigger
            var left = CreateAdditionalTrigger(authoring.triggerCubePrefab, targetEntity, -authoring.offsetDistance);
            // Create right trigger
            var right = CreateAdditionalTrigger(authoring.triggerCubePrefab, targetEntity, authoring.offsetDistance);
        }

        private Entity CreateAdditionalTrigger(GameObject prefab, Entity targetEntity, float xOffset)
        {
            var entity = GetEntity(prefab, TransformUsageFlags.Dynamic);
            AddComponent(entity, new LoadingZone { target = targetEntity });
            AddComponent(entity, new OffsetFromTarget
            {
                offset = new float3(xOffset, 0f, 0f)
            });
            AddComponent<TriggerTag>(entity);
            return entity;
        }
    }
}

public struct LoadingZone : IComponentData
{
    public Entity target;
}

public struct OffsetFromTarget : IComponentData
{
    public float3 offset;
}

public struct TriggerOffset : IComponentData
{
    public float offset;
}

public struct TriggerTag : IComponentData { }

public struct InvisibleTag : IComponentData { }
