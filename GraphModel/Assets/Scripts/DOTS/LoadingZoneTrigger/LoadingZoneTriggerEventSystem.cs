using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
/*
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct LoadingZoneTriggerEventSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var sim = SystemAPI.GetSingleton<SimulationSingleton>();

        var triggerTagLookup = SystemAPI.GetComponentLookup<TriggerTag>(true);
        var followTargetLookup = SystemAPI.GetComponentLookup<LoadingZone>(true);

        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        var job = new TriggerJob
        {
            TriggerTagLookup = triggerTagLookup,
            FollowTargetLookup = followTargetLookup,
            ECB = ecb.AsParallelWriter()
        };

        state.Dependency = job.Schedule(sim, state.Dependency);
        state.Dependency.Complete(); // Ensure safe ECB playback after trigger job

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    struct TriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<TriggerTag> TriggerTagLookup;
        [ReadOnly] public ComponentLookup<LoadingZone> FollowTargetLookup;

        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute(TriggerEvent triggerEvent)
        {
            var a = triggerEvent.EntityA;
            var b = triggerEvent.EntityB;

            bool aIsTrigger = TriggerTagLookup.HasComponent(a);
            bool bIsTrigger = TriggerTagLookup.HasComponent(b);

            // We only want to process one direction: trigger vs colliding body
            if (aIsTrigger && !bIsTrigger)
            {
                HandleTrigger(a, b, ECB);
            }
            else if (bIsTrigger && !aIsTrigger)
            {
                HandleTrigger(b, a, ECB);
            }
        }

        private void HandleTrigger(Entity triggerEntity, Entity otherEntity, EntityCommandBuffer.ParallelWriter ecb)
        {
            if (!FollowTargetLookup.HasComponent(triggerEntity)) return;

            var follow = FollowTargetLookup[triggerEntity];
            var world = World.DefaultGameObjectInjectionWorld;

            if (!world.IsCreated) return;

            var em = world.EntityManager;

            if (!em.HasComponent<Transform>(follow.target)) return;

            var go = em.GetComponentObject<GameObject>(follow.target);
            var detector = go.GetComponent<SkytrainInsideStationDetector>();
            if (detector != null)
            {
                bool shouldDestroy = detector.DoTheThing();
                if (shouldDestroy)
                {
                    // Don't destroy the trigger; destroy the other entity
                    var index = 0; // Could use `UnityEngine.Random.Range(0, 100000)` if needed
                    ecb.DestroyEntity(index, otherEntity);
                }
            }
        }
    }
}
*/