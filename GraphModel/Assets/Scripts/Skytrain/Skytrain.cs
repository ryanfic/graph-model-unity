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
        print("initializing skytrain");
        this.line = line;
        transform.parent = line.gameObject.transform;
        gameObject.name = name;
        transform.position = initialPosition;

        this.positions = new List<Vector3>(line.points);
        this.stationIndices = new HashSet<int>();
        this.stationIndexToName = new Dictionary<int, string>();

        var stationList = StationDatabase.GetStationsFromLine(line.lineName);
        var pendingInserts = new List<(int index, string name, Vector3 position)>();

        foreach (var station in stationList)
        {
            Vector3 stationWorldPos = convertLatLonToWorld(station.Latitude, station.Longitude);

            int bestIndex = -1;
            float bestDist = float.MaxValue;

            for (int i = 0; i < positions.Count - 1; i++)
            {
                Vector3 a = positions[i];
                Vector3 b = positions[i + 1];
                Vector3 ab = b - a;
                Vector3 ap = stationWorldPos - a;

                float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / ab.sqrMagnitude);
                Vector3 projected = a + t * ab;
                float dist = Vector3.Distance(projected, stationWorldPos);

                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = i + 1;
                }
            }

            if (bestIndex != -1)
            {
                pendingInserts.Add((bestIndex, station.Name, stationWorldPos));
            }
        }

        pendingInserts.Sort((a, b) => a.index.CompareTo(b.index));

        int offset = 0;
        foreach (var (index, n, pos) in pendingInserts)
        {
            int insertIndex = index + offset;
            positions.Insert(insertIndex, pos);
            stationIndices.Add(insertIndex);
            stationIndexToName[insertIndex] = n;
            offset++;
        }
    }



    private void Update()
    {
        MoveAlongLine();
    }

    /// <summary>
    /// Handles the movement and interaction between skytrains and the stations.
    /// </summary>
    private void MoveAlongLine()
    {
        if (positions == null || positions.Count < 2) return;

        Vector3 currentPos = transform.position;
        Vector3 nextPos = positions[currentPositionIndex + 1];
        Vector3 targetPos = new Vector3(nextPos.x, 0.2f, nextPos.z);
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
                    HandleStationStop(stationName);
                }

                stopTimer -= Time.deltaTime;
                if (stopTimer <= 0f)
                {
                    currentState = SkytrainState.Accelerating;
                    currentPositionIndex++;
                    hasTransferredPassengers = false;
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
                var d = deceleration /** (NextStationIndex() == currentPositionIndex + 1 ? 1 : 0.95f)*/;
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

        // position
        transform.position = Vector3.MoveTowards(
            currentPos,
            targetPos,
            currentSpeed * Time.deltaTime
        );

        // rotation
        Vector3 direction = (targetPos - currentPos).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

    }

    /// <summary>
    /// Currently a placeholder for more complex things
    /// </summary>
    private void HandleStationStop(string stationName)
    {
        hasTransferredPassengers = true;
        SkytrainStation station = line.stations[stationName];

        UnloadPassengers(5);

        if (station != null)
            station.IncreasePassengers(5);
        else
            print("station is null");
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

        foreach (var station in StationDatabase.GetStationsFromLine(line.lineName))
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
