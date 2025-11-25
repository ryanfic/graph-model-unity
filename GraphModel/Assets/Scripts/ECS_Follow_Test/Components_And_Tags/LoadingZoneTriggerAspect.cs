using Unity.Entities;


public readonly partial struct LoadingZoneTriggerAspect : IAspect
{
    public readonly Entity Entity;

    //private readonly TransformAspect _transformAspect;

    private readonly RefRO<LoadingZoneComponent> _loadingZones;
}

