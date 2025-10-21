using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Handles player experience, skill progression, and career events
/// </summary>
public class PlayerDevelopment : MonoBehaviour
{
    [System.Serializable]
    public class PlayerStats
    {
        public CSPlayer player;
        public int totalMatches;
        public int matchesWon;
        public float averageRating;
        public float totalKills;
        public float totalDeaths;
        public float careerKDA;
        public int totalMapWins;
        public Dictionary<string, float> mapStats;
        public PlayerForm currentForm;
        public int injuryDayRemaining;
        public float morale;
        public float experience;
        public PlayerCareerPhase careerPhase;
    }

    public enum PlayerForm { Exceptional, Excellent, Good, Average, Poor, Terrible }
    public enum PlayerCareerPhase { Rising, Peak, Declining, Veteran, Retired }

    private Dictionary<CSPlayer, PlayerStats> playerStats = new();
    private List<CareerEvent> careerHistory = new();

    [System.Serializable]
    public class CareerEvent
    {
        public CSPlayer player;
        public string eventDescription;
        public DateTime eventDate;
        public EventType eventType;
        public float impact; // How much it affects stats
    }

    public enum EventType { Injury, Recovery, Award, Championship, Transfer, Demotion, Promotion, Retirement }

    private void Start()
    {
        InitializeEmptyStats();
    }

    private void InitializeEmptyStats()
    {
        // Find all GameObjects with CSPlayer component and initialize their stats
        Component[] allPlayerComponents = (Component[])GameObject.FindObjectsByType(
            typeof(CSPlayer),
            FindObjectsSortMode.None
        );
        foreach (var comp in allPlayerComponents)
        {
            CSPlayer player = comp as CSPlayer;
            if (player != null && !playerStats.ContainsKey(player))
            {
                playerStats[player] = new PlayerStats
                {
                    player = player,
                    totalMatches = 0,
                    matchesWon = 0,
                    averageRating = 1.0f,
                    totalKills = 0,
                    totalDeaths = 1,
                    careerKDA = 1.0f,
                    totalMapWins = 0,
                    mapStats = new Dictionary<string, float>(),
                    currentForm = PlayerForm.Average,
                    injuryDayRemaining = 0,
                    morale = 75f,
                    experience = 0f,
                    careerPhase = PlayerCareerPhase.Rising
                };
            }
        }
    }

    public void RecordMatchPerformance(CSPlayer player, MatchSimulationManager.PlayerPerformance performance)
    {
        if (!playerStats.ContainsKey(player))
        {
            playerStats[player] = new PlayerStats
            {
                player = player,
                totalMatches = 0,
                matchesWon = 0,
                averageRating = 1.0f,
                totalKills = 0,
                totalDeaths = 1,
                careerKDA = 1.0f,
                mapStats = new Dictionary<string, float>()
            };
        }

        PlayerStats stats = playerStats[player];
        stats.totalMatches++;
        stats.totalKills += performance.kills;
        stats.totalDeaths += Mathf.Max(1, performance.deaths);
        stats.averageRating = (stats.averageRating * (stats.totalMatches - 1) + performance.ratingPerformance) / stats.totalMatches;

        // Update form based on rating
        UpdatePlayerForm(stats, performance.ratingPerformance);

        // Award experience
        stats.experience += performance.ratingPerformance * 10f;

        // Update player attributes slightly based on performance
        ImprovePlayerSkills(player, performance);

        // Recalculate KDA
        stats.careerKDA = stats.totalKills / stats.totalDeaths;

        Debug.Log($"{player.playerName} - Rating: {performance.ratingPerformance:F2}, KDA: {stats.careerKDA:F2}");
    }

    public void UpdatePlayerStats()
    {
        foreach (var kvp in playerStats)
        {
            PlayerStats stats = kvp.Value;

            // Handle injuries
            if (stats.injuryDayRemaining > 0)
            {
                stats.injuryDayRemaining--;
                if (stats.injuryDayRemaining == 0)
                {
                    RecoverFromInjury(kvp.Key);
                }
            }

            // Natural morale decay
            stats.morale = Mathf.Clamp(stats.morale - 0.1f, 0, 100);

            // Form changes based on performance trends
            UpdateFormDecay(stats);

            // Check for career phase progression
            UpdateCareerPhase(stats);
        }
    }

    private void UpdatePlayerForm(PlayerStats stats, float recentRating)
    {
        if (recentRating > 1.5f)
            stats.currentForm = PlayerForm.Exceptional;
        else if (recentRating > 1.2f)
            stats.currentForm = PlayerForm.Excellent;
        else if (recentRating > 1.0f)
            stats.currentForm = PlayerForm.Good;
        else if (recentRating > 0.8f)
            stats.currentForm = PlayerForm.Average;
        else if (recentRating > 0.5f)
            stats.currentForm = PlayerForm.Poor;
        else
            stats.currentForm = PlayerForm.Terrible;
    }

    private void UpdateFormDecay(PlayerStats stats)
    {
        // Form gradually decays if player isn't playing
        switch (stats.currentForm)
        {
            case PlayerForm.Exceptional:
                if (Random.value < 0.1f) stats.currentForm = PlayerForm.Excellent;
                break;
            case PlayerForm.Excellent:
                if (Random.value < 0.05f) stats.currentForm = PlayerForm.Good;
                break;
            case PlayerForm.Good:
                if (Random.value < 0.02f) stats.currentForm = PlayerForm.Average;
                break;
            case PlayerForm.Average:
                // Stable
                break;
            case PlayerForm.Poor:
                if (Random.value < 0.1f) stats.currentForm = PlayerForm.Average;
                break;
        }
    }

    private void ImprovePlayerSkills(CSPlayer player, MatchSimulationManager.PlayerPerformance performance)
    {
        // Small incremental improvements based on performance
        float improvementMultiplier = performance.ratingPerformance / 100f;

        // High performers get better
        if (performance.ratingPerformance > 1.2f)
        {
            player.aim += (int)(Random.Range(0.1f, 0.3f) * improvementMultiplier);
            player.reflexes += (int)(Random.Range(0.1f, 0.2f) * improvementMultiplier);
            player.consistency += (int)(Random.Range(0.05f, 0.15f) * improvementMultiplier);
        }

        // Clamp values to 100
        player.aim = (int)Mathf.Min(player.aim, 100f);
        player.reflexes = (int)Mathf.Min(player.reflexes, 100f);
        player.consistency = (int)Mathf.Min(player.consistency, 100f);
    }

    public void CauseInjury(CSPlayer player, int daysOut = 14)
    {
        if (!playerStats.ContainsKey(player))
            return;

        PlayerStats stats = playerStats[player];
        stats.injuryDayRemaining = daysOut;

        // Temporarily reduce reflexes and aim
        player.reflexes *= (int)0.8f;
        player.aim *= (int)0.8f;

        CareerEvent careerEvent = new()
        {
            player = player,
            eventDescription = $"Injured - Out for {daysOut} days",
            eventDate = DateTime.Now,
            eventType = EventType.Injury,
            impact = -0.2f
        };
        careerHistory.Add(careerEvent);

        Debug.Log($"{player.playerName} has been injured and will be out for {daysOut} days");
    }

    private void RecoverFromInjury(CSPlayer player)
    {
        // Restore stats
        if (playerStats.ContainsKey(player))
        {
            CareerEvent careerEvent = new()
            {
                player = player,
                eventDescription = "Recovered from injury",
                eventDate = DateTime.Now,
                eventType = EventType.Recovery,
                impact = 0.1f
            };
            careerHistory.Add(careerEvent);

            playerStats[player].morale += 10f;
            Debug.Log($"{player.playerName} has recovered from injury");
        }
    }

    private void UpdateCareerPhase(PlayerStats stats)
    {
        // Determine career phase based on experience and age
        float ageImpact = Mathf.Abs(stats.player.age - 25) * 0.01f;

        if (stats.experience < 500)
            stats.careerPhase = PlayerCareerPhase.Rising;
        else if (stats.experience < 2000)
            stats.careerPhase = PlayerCareerPhase.Peak;
        else if (stats.player.age > 30)
            stats.careerPhase = PlayerCareerPhase.Declining;
        else if (stats.player.age > 35)
            stats.careerPhase = PlayerCareerPhase.Veteran;

        // Reduce skills if in declining phase
        if (stats.careerPhase == PlayerCareerPhase.Declining || stats.careerPhase == PlayerCareerPhase.Veteran)
        {
            stats.player.reflexes *= (int)0.995f;
            stats.player.consistency *= (int)0.99f;
        }
    }

    public PlayerStats GetPlayerStats(CSPlayer player)
    {
        return playerStats.ContainsKey(player) ? playerStats[player] : null;
    }

    public void AwardAchievement(CSPlayer player, string achievement)
    {
        if (!playerStats.ContainsKey(player))
            return;

        playerStats[player].morale += 15f;

        CareerEvent careerEvent = new()
        {
            player = player,
            eventDescription = achievement,
            eventDate = DateTime.Now,
            eventType = EventType.Award,
            impact = 0.1f
        };
        careerHistory.Add(careerEvent);

        Debug.Log($"{player.playerName} achieved: {achievement}");
    }

    public List<CareerEvent> GetPlayerCareerHistory(CSPlayer player)
    {
        List<CareerEvent> playerHistory = new();
        foreach (var careerEvent in careerHistory)
        {
            if (careerEvent.player == player)
            {
                playerHistory.Add(careerEvent);
            }
        }
        return playerHistory;
    }

    public float GetPlayerExperience(CSPlayer player)
    {
        return playerStats.ContainsKey(player) ? playerStats[player].experience : 0f;
    }

    public PlayerForm GetPlayerCurrentForm(CSPlayer player)
    {
        return playerStats.ContainsKey(player) ? playerStats[player].currentForm : PlayerForm.Average;
    }
}