using UnityEngine;
using Unity.Entities;

public static class LoadingZoneSkytrainCallbackHelper
{
    public static void TryCallDoTheThing(Entity entity)
    {
        if (!World.DefaultGameObjectInjectionWorld.IsCreated) return;

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!em.HasComponent<LoadingZone>(entity))
            return;

        var follow = em.GetComponentData<LoadingZone>(entity);
        var targetEntity = follow.target;

        var go = em.GetComponentObject<GameObject>(targetEntity);

        var detector = go.GetComponent<SkytrainInsideStationDetector>();
        if (detector != null)
        {
            detector.DoTheThing();
        }
    }
}
