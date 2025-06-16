using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public struct Passenger : IComponentData
{
    
}

public struct MoveSpeed : IComponentData
{
    public float Value;
}

public struct Destination : IComponentData
{
    public float3 Value;
}

public struct Velocity : IComponentData
{
    public float3 Value;
}

public struct Avoidance : IComponentData
{
    public float Radius;
    public float AvoidanceStrength; // 0–1 weight of avoidance vs. goal
}


