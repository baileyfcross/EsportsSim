using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class CSPlayer
{
    // Basic info
    public string firstName;
    public string lastName;
    public string nickName;
    public int age;
    public string nationality;
    public Sprite playerPhoto;

    // CS-specific core skills (1-20 scale)
    public int aim;                // Accuracy and precision
    public int reactionTime;       // How quickly they respond to visual stimuli
    public int positioning;        // Proper angle holding and map awareness
    public int utilityUsage;       // Effectiveness with grenades
    public int clutchAbility;      // Performance in 1vX situations
    public int consistency;        // Performance stability
    public int mentalFortitude;    // Handling pressure/tilt
    public int gamesense;          // Reading the game state and opponents
    public int movementSkill;      // Strafing, counter-strafing, bhopping

    // Role-specific skills
    public int awpSkill;           // Sniper ability
    public int rifleSkill;         // Rifle mastery (AK/M4)
    public int pistolSkill;        // Pistol rounds effectiveness
    public int leadershipAbility;  // IGL potential
    public int siteAnchorAbility;  // Holding bombsites as CT
    public int entryFragging;      // First contact ability as T
    public int lurking;            // Flanking and timing

    // Map proficiencies (0-20 per map)
    public Dictionary<string, int> mapProficiency = new Dictionary<string, int>();

    // Career/Contract info
    public int salary;
    public int contractYearsRemaining;
    public int marketValue;
    public List<string> previousTeams = new List<string>();
    public int yearsActive;

    // Personality traits
    public int teamwork;
    public int workEthic;
    public int temperament;       // Tiltability (low is bad, high is good)

    // CS-specific performance metrics
    public float averageHLTVRating;
    public float headshotPercentage;
    public float killsPerRound;
    public float averageDamagePerRound;
    public float flashAssistsPerMatch;
    public float clutchesWonPercentage;

    // Primary role
    public PlayerRole preferredRole;

    // Method to generate random CS player with realistic distributions
    public static CSPlayer GenerateRandomPlayer(string[] firstNames, string[] lastNames, string[] nationalities, string[] activeMaps)
    {
        CSPlayer player = new CSPlayer();

        // Generate basic info
        player.firstName = firstNames[UnityEngine.Random.Range(0, firstNames.Length)];
        player.lastName = lastNames[UnityEngine.Random.Range(0, lastNames.Length)];
        player.nickName = GenerateNickname(); // You'll implement this
        player.age = Mathf.FloorToInt(NormalDistribution(16, 35, 22, 4)); // CS players can start younger
        player.nationality = nationalities[UnityEngine.Random.Range(0, nationalities.Length)];

        // Generate core skills (bell curve distribution)
        player.aim = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.reactionTime = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.positioning = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.utilityUsage = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.clutchAbility = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.consistency = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.mentalFortitude = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.gamesense = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.movementSkill = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));

        // Generate role skills
        player.awpSkill = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 4));
        player.rifleSkill = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.pistolSkill = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.leadershipAbility = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 4));
        player.siteAnchorAbility = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.entryFragging = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        player.lurking = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));

        // Generate map proficiency
        foreach (string map in activeMaps)
        {
            player.mapProficiency[map] = Mathf.RoundToInt(NormalDistribution(1, 20, 10, 3));
        }

        // Determine preferred role based on highest stats
        player.preferredRole = DeterminePreferredRole(player);

        // Generate performance metrics
        player.averageHLTVRating = NormalDistribution(0.5f, 1.5f, 1.0f, 0.2f);
        player.headshotPercentage = NormalDistribution(30f, 75f, 48f, 8f);
        player.killsPerRound = NormalDistribution(0.5f, 1.0f, 0.7f, 0.1f);
        player.averageDamagePerRound = NormalDistribution(60f, 95f, 75f, 10f);

        return player;
    }

    // Helper for realistic bell-curve stat generation
    private static float NormalDistribution(float min, float max, float mean, float standardDeviation)
    {
        float u1 = 1.0f - UnityEngine.Random.value;
        float u2 = 1.0f - UnityEngine.Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        float randNormal = mean + standardDeviation * randStdNormal;
        return Mathf.Clamp(randNormal, min, max);
    }

    // Determine player's natural role based on stats
    private static PlayerRole DeterminePreferredRole(CSPlayer player)
    {
        Dictionary<PlayerRole, float> roleScores = new Dictionary<PlayerRole, float>();

        roleScores[PlayerRole.AWPer] = player.awpSkill * 1.5f + player.reactionTime * 0.8f + player.consistency * 0.7f;
        roleScores[PlayerRole.IGL] = player.leadershipAbility * 1.5f + player.gamesense * 1.2f + player.utilityUsage * 0.5f;
        roleScores[PlayerRole.EntryFragger] = player.entryFragging * 1.5f + player.aim * 0.8f + player.reactionTime * 0.8f;
        roleScores[PlayerRole.Support] = player.utilityUsage * 1.5f + player.teamwork * 1.0f + player.positioning * 0.8f;
        roleScores[PlayerRole.Lurker] = player.lurking * 1.5f + player.clutchAbility * 1.0f + player.gamesense * 0.8f;
        roleScores[PlayerRole.Rifler] = player.rifleSkill * 1.2f + player.consistency * 0.8f + player.positioning * 0.8f;

        // Find highest score
        PlayerRole bestRole = PlayerRole.Rifler;
        float highestScore = 0;

        foreach (var role in roleScores)
        {
            if (role.Value > highestScore)
            {
                highestScore = role.Value;
                bestRole = role.Key;
            }
        }

        return bestRole;
    }

    private static string GenerateNickname()
    {
        // Implement CS-style nickname generation
        // Examples: "s1mple", "dev1ce", "Stewie2K", "NiKo", etc.
        string[] prefixes = new string[] { "x", "s1", "pro", "neo", "dark", "fast", "crisp", "aim", "head" };
        string[] suffixes = new string[] { "god", "shot", "hunter", "Ace", "2k", "X", "killer", "HS", "master" };

        if (UnityEngine.Random.value > 0.5f)
        {
            return prefixes[UnityEngine.Random.Range(0, prefixes.Length)] +
                  suffixes[UnityEngine.Random.Range(0, suffixes.Length)];
        }
        else
        {
            // Single word nicknames
            string[] singleWords = new string[] { "blade", "hunter", "guardian", "snax", "forest", "device", "dupreeh" };
            return singleWords[UnityEngine.Random.Range(0, singleWords.Length)];
        }
    }
}

public enum PlayerRole
{
    AWPer,
    IGL,          // In-Game Leader
    EntryFragger,
    Support,
    Lurker,
    Rifler       // General rifler
}