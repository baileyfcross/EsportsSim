using UnityEngine;

[System.Serializable]
public class Map : Object
{
    public string mapName;
    public Sprite mapThumbnail;
    public Sprite mapMinimap;

    // Map statistics
    public int timesPlayed = 0;
    public float averageRoundTime = 0f;

    // Optional: Side bias statistics
    public float tSideWinRate = 50f; // Percentage
}
