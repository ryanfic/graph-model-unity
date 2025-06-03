using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Skytrain : MonoBehaviour
{
    [Header("Skytrain Movement Settings")]
    public float maxSpeed = 20f;
    public float acceleration = 2f;
    public float deceleration = 2.5f;
    public float stopDuration = 3f;

    private int currentPositionIndex;
    private float currentSpeed;
    private SkytrainState currentState;
    private float stopTimer;
    private Vector2 currentPosition;
    private Vector2 nextPosition;
    private float distanceToNext;
    private List<Vector3> positions;

    private HashSet<int> stationIndices = new();
    private Dictionary<int, string> stationIndexToName = new();
    private bool hasTransferredPassengers;

    public SkytrainLine line;


    private int passengerCount = 200;

    private const int MAX_PASSENGERS = 200;


    private enum SkytrainState
    {
        Stopped,
        Accelerating,
        Decelerating,
        Cruising
    }


    public void InitializeSkytrain(SkytrainLine line, string name, Vector3 initialPosition, Func<float, float, Vector3> convertLatLonToWorld)
    {
        this.line = line;
        this.positions = line.shape;

        transform.parent = line.gameObject.transform;
        gameObject.name = name;
        transform.position = initialPosition;

        this.stationIndices = GetStationIndices(convertLatLonToWorld);
        
    }


    private void Update()
    {
        MoveAlongLine();
    }
    private void MoveAlongLine()
    {
        if (positions == null || positions.Count < 2) return;

        Vector3 currentPos = transform.position;
        Vector3 nextPos = positions[currentPositionIndex + 1];
        Vector3 targetPos = new Vector3(nextPos.x, 0f, nextPos.z);
        float distanceToTarget = Vector3.Distance(currentPos, targetPos);

        // Advance to next point if close enough and not a station
        if (distanceToTarget < 0.01f && !stationIndices.Contains(currentPositionIndex + 1))
        {
            currentPositionIndex++;
            return;
        }

        switch (currentState)
        {
            case SkytrainState.Stopped:
                if (!hasTransferredPassengers && stationIndexToName.TryGetValue(currentPositionIndex+1, out string stationName))
                {
                    hasTransferredPassengers = true;
                    print("transfering passengers");
                    foreach (var (k, v) in line.stations)
                    {
                        print($"{k}");
                    }
                    print(stationName);
                    SkytrainStation station = line.stations[stationName];
                    var passengersOff = UnloadPassengers(5);
                    if (station != null)
                        station.IncreasePassengers(passengersOff);
                    
                }
                stopTimer -= Time.deltaTime;
                if (stopTimer <= 0f)
                {
                    currentState = SkytrainState.Accelerating;
                    currentPositionIndex++;
                }
                return;

            case SkytrainState.Accelerating:
                if (DistanceToNextStation() < DecelerationDistance())
                {
                    currentState = SkytrainState.Decelerating;
                    break;
                }

                currentSpeed += acceleration * Time.deltaTime;
                if (currentSpeed >= maxSpeed)
                {
                    currentSpeed = maxSpeed;
                    currentState = SkytrainState.Cruising;
                }
                break;

            case SkytrainState.Cruising:
                if (DistanceToNextStation() < DecelerationDistance())
                {
                    currentState = SkytrainState.Decelerating;
                }
                break;

            case SkytrainState.Decelerating:
                var d = deceleration * (NextStationIndex() == currentPositionIndex + 1 ? 1 : 0.95f);
                currentSpeed -= d * Time.deltaTime;
                if (currentSpeed <= 0f)
                {
                    currentSpeed = 0f;
                    transform.position = targetPos;

                    if (currentPositionIndex >= positions.Count - 1)
                    {
                        enabled = false;
                        return;
                    }

                    currentState = SkytrainState.Stopped;
                    stopTimer = stopDuration;
                    return;
                }
                break;
        }

        transform.position = Vector3.MoveTowards(
            currentPos,
            targetPos,
            currentSpeed * Time.deltaTime
        );
    }


    private float DistanceToNextStation()
    {
        Vector3 currentPos = transform.position;

        for (int i = currentPositionIndex + 1; i < positions.Count; i++)
        {
            if (stationIndices.Contains(i))
            {
                Vector3 stationPos = new Vector3(positions[i].x, 0f, positions[i].z);
                return Vector3.Distance(currentPos, stationPos);
            }
        }

        return float.MaxValue; // No more stations
    }

    private int NextStationIndex()
    {
        for (int i = currentPositionIndex + 1; i < positions.Count; i++)
        {
            if (stationIndices.Contains(i))
            {
                return i;
            }
        }
        return -1;
    }

    public HashSet<int> GetStationIndices(Func<float, float, Vector3> convertLatLonToWorld)
    {
        HashSet<int> stationIndices = new();
        stationIndexToName = new();

        foreach (var station in StationDatabase.GetLinesFromStation(line.lineName))
        {
            Vector3 worldPos = convertLatLonToWorld(station.Latitude, station.Longitude);
            int closestIndex = -1;
            float closestDist = float.MaxValue;
            for (int i = 0; i < positions.Count; i++)
            {
                float dist = Vector3.Distance(worldPos, positions[i]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                }
            }
            if (closestIndex != -1)
            {
                stationIndices.Add(closestIndex);
                stationIndexToName[closestIndex] = station.Name;
            }
        }
        return stationIndices;
        
    }

    private float DecelerationDistance()
    {
        return (currentSpeed * currentSpeed) / (2 * deceleration);
    }

    public int LoadPassengers(int count)
    {
        passengerCount += count;

        if (passengerCount > MAX_PASSENGERS)
        {
            var overflow = passengerCount - MAX_PASSENGERS;
            passengerCount = MAX_PASSENGERS;
            return overflow;
        }
        return 0;
    }

    public int UnloadPassengers(int count)
    {
        passengerCount -= count;

        if (passengerCount < 0)
        {
            var overflow = -passengerCount;
            passengerCount = 0;
            return overflow;
        }
        return 0;
    }


    private void OnDrawGizmos()
    {
        if (positions == null) return;
        Gizmos.color = Color.red;
        foreach (int idx in stationIndices)
        {
            Gizmos.DrawSphere(positions[idx], 3f);
        }
    }

    
}
