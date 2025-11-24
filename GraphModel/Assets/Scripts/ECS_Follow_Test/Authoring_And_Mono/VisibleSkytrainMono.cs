using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public class VisibleSkytrainMono : MonoBehaviour
{
}

public class VisibleSkytrainBaker : Baker<VisibleSkytrainMono>
{
    public override void Bake(VisibleSkytrainMono authoring)
    {
        GetEntity(authoring);
    }
}
