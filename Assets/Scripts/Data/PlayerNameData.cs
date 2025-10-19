using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerNameData", menuName = "EsportsSim/Name Data")]
public class PlayerNameData : ScriptableObject
{
    public List<string> firstNames = new List<string>();
    public List<string> lastNames = new List<string>();
    public List<string> nicknamePrefixes = new List<string>();
    public List<string> nicknameSuffixes = new List<string>();
    public List<CountryData> countries = new List<CountryData>();
}