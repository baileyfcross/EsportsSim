using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CSMapData", menuName = "CounterStrikeSim/Map Data")]
public class MapData : ScriptableObject
{
    public List<Map> activeDutyMaps = new();
    public List<string> playerPositionNames = new(); // Common callout positions

    public static Map toMap(Object mapData)
    {
        return mapData as Map;
    }

}