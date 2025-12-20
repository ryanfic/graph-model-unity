using Unity.Entities;
using Unity.Physics.Stateful;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct LoadingZoneTriggerAspect : IAspect
{
    public readonly Entity Entity;

    private readonly RefRO<LocalTransform> _localTransform;

    private readonly RefRO<LoadingZoneComponent> _loadingZones;
    private readonly DynamicBuffer<StatefulTriggerEvent> _triggerEvents;

    public void HandleTriggerEvent()
    {
        for(int i = 0; i < _triggerEvents.Length; i++) {
            Debug.Log("TRIGGERED["+i+"]");
        }

    }
}

