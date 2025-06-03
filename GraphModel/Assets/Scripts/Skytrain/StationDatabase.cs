using System.Collections.Generic;
using UnityEngine;

public static class StationDatabase
{
    public static readonly List<StationData> CanadaLineStations = new()
    {
        new StationData("Marine Drive", 49.20954787392407f, -123.11707077626738f),
        new StationData("Langara - 49th Avenue", 49.22638691960162f, -123.11632486084643f),
        new StationData("Oakridge - 41st Avenue", 49.23346681076931f, -123.11604067532821f),
        new StationData("King Edward", 49.24912287838039f, -123.11533610484715f),
        new StationData("Broadway - City Hall", 49.262773467883484f, -123.11478218139136f),
        new StationData("Olympic Village", 49.26634023249435f, -123.11519667899394f),
        new StationData("Yaletown - Roundhouse", 49.2743622548375f, -123.1218903777964f),
        new StationData("Vancouver City Center", 49.28173470745285f, -123.11928408921445f),
        new StationData("Waterfront - Canada", 49.28566576739238f, -123.1133392689008f),
    };

    public static readonly List<StationData> ExpoLineStations = new()
    {
        new StationData("Joyce - Collingwood", 49.23839380523484f, -123.03180671710777f),
        new StationData("29th Avenue", 49.24424259217731f, -123.04594067398634f),
        new StationData("Nanaimo", 49.248272210616825f, -123.05587151633279f),
        new StationData("Commercial - Broadway", 49.26293626736842f, -123.06845389883729f),
        new StationData("Main Street - Science World", 49.27317791187965f, -123.10060690768856f),
        new StationData("Stadium - Chinatown", 49.279441691877636f, -123.10956479630316f),
        new StationData("Granville", 49.28363768680059f, -123.11640402719512f),
        new StationData("Burrard", 49.28586014917365f, -123.11997233644202f),
        new StationData("Waterfront - Expo", 49.286075484422376f, -123.11173815504947f),
    };

    public static readonly List<StationData> MillenniumLineStations = new()
    {
        new StationData("Rupert", 49.26076477791514f, -123.03282383181416f),
        new StationData("Renfew", 49.25889291082948f, -123.04530845874768f),
        new StationData("Commercial - Broadway", 49.26293626736842f, -123.06845389883729f),
        new StationData("VCC - Clark", 49.2657831984991f, -123.07896225243121f),
    };


    public static List<StationData> GetLinesFromStation(string lineName)
    {
        switch (lineName)
        {
            case "Canada Line":
                return CanadaLineStations;
            case "Expo Line":
                return ExpoLineStations;
            case "Millennium Line":
                return MillenniumLineStations;
            default:
                return null;
        }
    }
}

public class StationData
{
    public string Name;
    public float Latitude;
    public float Longitude;

    public StationData(string name, float lat, float lon)
    {
        Name = name;
        Latitude = lat;
        Longitude = lon;
    }
}

