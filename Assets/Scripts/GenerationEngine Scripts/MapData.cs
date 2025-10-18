using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

[CreateAssetMenu(fileName = "CSMapData", menuName = "CounterStrikeSim/Map Data")]
public class MapData : ScriptableObject
{
    public List<Map> activeDutyMaps = new List<Map>();
    public List<string> playerPositionNames = new List<string>(); // Common callout positions
}
