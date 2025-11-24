using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using Unity.Transforms;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
public partial struct EntityMessageOutputSystem : ISystem
{
    private EntityQuery _EntityMessageQuery;

    public void OnCreate(ref SystemState state)
    {
        _EntityMessageQuery = state.GetEntityQuery(typeof(MessageComponent));
    }

    public void OnUpdate(ref SystemState state)
    {
        //Debug.Log("This is potato calling major tom");

        var messageEntities = _EntityMessageQuery.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < messageEntities.Length; ++i)
        {
            var message = state.EntityManager.GetComponentData<MessageComponent>(messageEntities[i]);

            

            if (state.EntityManager.HasComponent<StatefulCollisionEvent>(messageEntities[i]) && state.EntityManager.GetBuffer<StatefulCollisionEvent>(messageEntities[i]).Length > 0)
            {
                DynamicBuffer<StatefulCollisionEvent> colEventBuffer = state.EntityManager.GetBuffer<StatefulCollisionEvent>(messageEntities[i]);
                Debug.Log("Message (from job): " + message.Message);
                Debug.Log("colEvent: [" + colEventBuffer[0].EntityA + "," + colEventBuffer[0].EntityB + "]");
            }
        }
    }
}
