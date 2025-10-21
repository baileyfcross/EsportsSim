using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerNameData", menuName = "EsportsSim/Name Data")]
public class PlayerNameData : ScriptableObject
{
    public List<string> firstNames = new();
    public List<string> lastNames = new();
    public List<string> nicknamePrefixes = new();
    public List<string> nicknameSuffixes = new();
    public List<CountryData> countries = new();
}