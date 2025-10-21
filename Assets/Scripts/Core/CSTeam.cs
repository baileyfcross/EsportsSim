using System.Collections.Generic;
using UnityEngine;

public class CSTeam
{
    public string name;         // "Natus Vincere", "FaZe Clan"
    public string tag;          // "NAVI", "FaZe" 
    public Sprite logo;
    public Country country;     // Or region like "Europe" for international teams
    public int worldRanking;    // HLTV-style ranking

    // Team composition - standard 5-player roster + subs
    public List<CSPlayer> activeRoster = new();
    public List<CSPlayer> benchPlayers = new();
    public CSCoach coach;

    // Team roles - which player plays what role
    public Dictionary<PlayerRole, CSPlayer> roleAssignments = new();

    // Team attributes
    public int reputation;
    public int fanbase;
    public int finances;
    public int facilities; // Training quality

    // Map pool strengths (0-100 rating per map)
    public Dictionary<string, int> mapPoolStrength = new();

    // Team playing style
    public float aggressionLevel;    // 0-100, passive to aggressive
    public float tacticalDepth;      // 0-100, loose to structured
    public float awpDependence;      // 0-100, rifle-heavy to AWP-centric
    public float adaptability;       // 0-100, rigid to fluid
    public float teamChemistry;        // Overall synergy rating

    // Recent results
    public List<CSMatch> recentMatches = new();

    public float CalculateTeamStrength()
    {
        // Algorithm to calculate overall team strength
        // Consider individual skills, roles, synergy, map pool
        return 0f; // Placeholder
    }

    public void AssignPlayerRoles()
    {
        // Logic to determine optimal role distribution
        // Consider player preferred roles and stats
    }

    public void UpdateMapPoolStrengths()
    {
        // Recalculate map strengths based on player proficiencies
    }

    public List<CSPlayer> GetActiveRoster()
    {
        return activeRoster;
    }

    public void AddPlayerToRoster(CSPlayer player)
    {
        if (!activeRoster.Contains(player))
        {
            activeRoster.Add(player);
        }
    }

    public void RemovePlayerFromRoster(CSPlayer player)
    {
        if (activeRoster.Contains(player))
        {
            activeRoster.Remove(player);
        }
    }
}
