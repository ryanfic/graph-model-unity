using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class SkytrainStation : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI stationNameAsset;
    [SerializeField] private TextMeshProUGUI passengerCountAsset;

    [Space(5)]
    [Header("Data")]
    public string stationName;
    public float lat;
    public float lon;
    public GameObject gameObjectRepresentation;

    private int passengerCount = 0;

    private List<int> expectedNumPassengersGettingOnTrainInTimeFrameList;
    private List<int> expectedNumPassengersGettingOffTrainInTimeFrameList;

    [SerializeField] private int numPassengersGottenOnTrainInTimeFrame = 0;
    [SerializeField] private int numPassengersGottenOffTrainInTimeFrame = 0;
    [SerializeField] private int curExpectedNumPassengersGettingOnTrainInTimeFrame = 0;
    [SerializeField] private int curExpectedNumPassengersGettingOffTrainInTimeFrame = 0;

    public void InitializeStation(string name, float lat, float lon, GameObject gameObjectRepresentation)
    {
        this.name = name;
        this.stationName = name;
        this.lat = lat;
        this.lon = lon;
        this.gameObjectRepresentation = gameObjectRepresentation;
        SetNameAsset(name);
        UpdatePassengerCountAsset();

        InitializeIngressEgressValues();
    }

    public void InitializeStation(string name, float lat, float lon, GameObject gameObjectRepresentation, int passengerCount)
    {
        this.name = name;
        this.lat = lat;
        this.lon = lon;
        this.gameObjectRepresentation = gameObjectRepresentation;
        this.passengerCount = passengerCount;
        UpdatePassengerCountAsset();
    }

    public void IncreasePassengers(int count)
    {
        passengerCount += count;
        UpdatePassengerCountAsset();
    }

    public void DecreasePassengers(int count)
    {
        passengerCount -= count;
        passengerCount = Mathf.Max(passengerCount, 0);
        UpdatePassengerCountAsset();
    }

    private void SetNameAsset(string name)
    {
        stationNameAsset.text = name;
    }

    public void UpdatePassengerCountAsset()
    {
        passengerCountAsset.text = $"Passenger count: {passengerCount}";
    }

    public void ChangeTimeFrame(int timeFrameNumber)
    {
        Debug.Log(gameObjectRepresentation.name + " changed to time frame " + timeFrameNumber);
        // change the number of people expected to exit the trains
        // change the number of people expected to enter the trains
        // set the current number of people having exited trains during time frame to 0
        // set the current number of people having entered trains during time frame to 0


        int cyclicTimeFrameNumber = timeFrameNumber % expectedNumPassengersGettingOnTrainInTimeFrameList.Count;

        numPassengersGottenOnTrainInTimeFrame = 0;
        numPassengersGottenOffTrainInTimeFrame = 0;
        curExpectedNumPassengersGettingOnTrainInTimeFrame = expectedNumPassengersGettingOnTrainInTimeFrameList[cyclicTimeFrameNumber];
        curExpectedNumPassengersGettingOffTrainInTimeFrame = expectedNumPassengersGettingOffTrainInTimeFrameList[cyclicTimeFrameNumber];
    }

    private void InitializeIngressEgressValues()
    {
        SimulationTimeManager manager = FindObjectOfType<SimulationTimeManager>();

        int timeFramesPerDay = 48;


        if (manager != null)
        {
            timeFramesPerDay = manager.GetTimeFramesPerDay();
        }
        else
        {
            Debug.LogWarning("SimulationTimeManager not found in the scene, using default value of 48");
        }
        LoadIngressEgressValues(timeFramesPerDay);
    }

    private void LoadIngressEgressValues(int timeFramesPerDay)
    {
        // get a list of time frames from config (or construct a hardcoded value)
        List<(int, int)> ingressEgressValueList = LoadHardCodedIngressEgressValues(timeFramesPerDay);

        expectedNumPassengersGettingOnTrainInTimeFrameList = new List<int>();
        expectedNumPassengersGettingOffTrainInTimeFrameList = new List<int>();

        int count = 0;
        while (count < timeFramesPerDay)
        {
            int i = count % ingressEgressValueList.Count;
            expectedNumPassengersGettingOnTrainInTimeFrameList.Add(ingressEgressValueList[i].Item1);
            expectedNumPassengersGettingOffTrainInTimeFrameList.Add(ingressEgressValueList[i].Item2);
            count++;
        }

        numPassengersGottenOnTrainInTimeFrame = 0;
        numPassengersGottenOffTrainInTimeFrame = 0;
        curExpectedNumPassengersGettingOnTrainInTimeFrame = expectedNumPassengersGettingOnTrainInTimeFrameList[0];
        curExpectedNumPassengersGettingOffTrainInTimeFrame = expectedNumPassengersGettingOffTrainInTimeFrameList[0];
}

    private List<(int,int)> LoadHardCodedIngressEgressValues(int timeFramesPerDay)
    {
        List<(int, int)> ingressEgressValueList = new List<(int, int)>();
        for (int i = 0; i < timeFramesPerDay; i++)
        {
            ingressEgressValueList.Add((i+1, 2*(i+1)));
        }
        return ingressEgressValueList;
    }
}
