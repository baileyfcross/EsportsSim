using UnityEngine;

[System.Serializable]
public class CountryData
{
    public string countryName;
    public string countryCode; // e.g., "US", "SE", "BR"
    public Sprite countryFlag;

    // Optional: Add weight for player distribution
    public int playerPopularity = 1; // How common players from this country are
}
