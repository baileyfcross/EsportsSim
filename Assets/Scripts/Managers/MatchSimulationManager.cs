using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Simulates Counter-Strike matches based on player skills, team composition, and map performance
/// </summary>
public class MatchSimulationManager : MonoBehaviour
{
    [System.Serializable]
    public class MatchResult
    {
        public Team teamA;
        public Team teamB;
        public int scoreTeamA;
        public int scoreTeamB;
        public Team winner;
        public List<PlayerPerformance> playerPerformances;
        public string map;
        public int totalRounds;
        public List<RoundResult> roundResults;
        public System.DateTime matchDate;
    }

    [System.Serializable]
    public class PlayerPerformance
    {
        public CSPlayer player;
        public int kills;
        public int deaths;
        public int assists;
        public float headShotPercentage;
        public int plantCount;
        public int defuseCount;
        public float utilityDamage;
        public int roundsMVP;
        public float ratingPerformance; // HLTV 2.0 style rating
    }

    [System.Serializable]
    public class RoundResult
    {
        public int roundNumber;
        public Team winnerTeam;
        public int teamAScore;
        public int teamBScore;
        public string result; // "T Win", "CT Win", "Draw"
        public List<PlayerRoundStat> playerStats;
    }

    [System.Serializable]
    public class PlayerRoundStat
    {
        public CSPlayer player;
        public int kills;
        public int deaths;
        public int damage;
        public bool wasMVP;
        public bool plantedBomb;
        public bool defusedBomb;
    }

    private float mapAdvantageA = 0f;
    private float mapAdvantageB = 0f;

    public MatchResult SimulateMatch(Team teamA, Team teamB, string map, int maxRounds = 30)
    {
        MatchResult result = new()
        {
            teamA = teamA,
            teamB = teamB,
            map = map,
            totalRounds = maxRounds,
            matchDate = System.DateTime.Now,
            playerPerformances = new List<PlayerPerformance>(),
            roundResults = new List<RoundResult>()
        };

        // Calculate map advantages
        CalculateMapAdvantages(teamA, teamB, map);

        int scoreA = 0;
        int scoreB = 0;
        Dictionary<CSPlayer, PlayerPerformance> playerStats = new();

        // Initialize player performance tracking
        foreach (var player in teamA.GetActiveRoster())
        {
            playerStats[player] = new PlayerPerformance { player = player };
        }
        foreach (var player in teamB.GetActiveRoster())
        {
            playerStats[player] = new PlayerPerformance { player = player };
        }

        // Simulate rounds
        for (int round = 0; round < maxRounds; round++)
        {
            RoundResult roundResult = SimulateRound(teamA, teamB, round, playerStats, map);
            result.roundResults.Add(roundResult);

            if (roundResult.winnerTeam == teamA)
                scoreA++;
            else if (roundResult.winnerTeam == teamB)
                scoreB++;

            roundResult.teamAScore = scoreA;
            roundResult.teamBScore = scoreB;

            // Check if match is decided (first to 16)
            if (scoreA > maxRounds / 2 || scoreB > maxRounds / 2)
            {
                break;
            }
        }

        result.scoreTeamA = scoreA;
        result.scoreTeamB = scoreB;
        result.winner = scoreA > scoreB ? teamA : teamB;

        // Calculate final player performances
        foreach (var kvp in playerStats)
        {
            CalculateRating(kvp.Value);
            result.playerPerformances.Add(kvp.Value);
        }

        return result;
    }

    private void CalculateMapAdvantages(Team teamA, Team teamB, string map)
    {
        // Analyze team performance on specific maps
        mapAdvantageA = 1.0f;
        mapAdvantageB = 1.0f;

        // This would be expanded with actual map stats from team performance history
        // For now, we'll use a base value
        var mapDataObj = Resources.Load($"Maps/{map}");
        Map mapData = MapData.toMap(mapDataObj);
        if (mapData != null)
        {
            // Adjust based on team map pool preferences (would need additional data)
        }
    }

    private RoundResult SimulateRound(Team teamA, Team teamB, int roundNumber,
        Dictionary<CSPlayer, PlayerPerformance> playerStats, string map)
    {
        RoundResult result = new()
        {
            roundNumber = roundNumber,
            playerStats = new List<PlayerRoundStat>()
        };

        List<CSPlayer> rosterA = teamA.GetActiveRoster();
        List<CSPlayer> rosterB = teamB.GetActiveRoster();

        // Determine round economy and side (T/CT)
        bool isTeamAT = (roundNumber < 15);
        Team tSide = isTeamAT ? teamA : teamB;
        Team ctSide = isTeamAT ? teamB : teamA;

        List<CSPlayer> tPlayers = isTeamAT ? rosterA : rosterB;
        List<CSPlayer> ctPlayers = isTeamAT ? rosterB : rosterA;

        // Simulate round outcome based on:
        // - Team skill levels
        // - Economy state
        // - Map control
        // - Individual player performance

        float teamASkillFactor = CalculateTeamSkill(rosterA);
        float teamBSkillFactor = CalculateTeamSkill(rosterB);

        float tSideBonus = isTeamAT ? mapAdvantageA : mapAdvantageB;
        float ctSideBonus = isTeamAT ? mapAdvantageB : mapAdvantageA;

        // Calculate win probability
        float tWinProbability = (teamASkillFactor * tSideBonus) /
                              (teamASkillFactor * tSideBonus + teamBSkillFactor * ctSideBonus);

        bool tWinsRound = Random.value < tWinProbability;

        // Simulate individual player stats for the round
        SimulatePlayerRoundStats(tWinsRound ? tPlayers : ctPlayers,
                                tWinsRound ? ctPlayers : tPlayers,
                                playerStats, result, tWinsRound);

        result.winnerTeam = tWinsRound ? tSide : ctSide;
        result.result = tWinsRound ? "T Win" : "CT Win";

        return result;
    }

    private float CalculateTeamSkill(List<CSPlayer> roster)
    {
        if (roster.Count == 0) return 1f;

        float totalSkill = 0f;
        foreach (var player in roster)
        {
            totalSkill += (player.aim + player.gameIntelligence + player.consistency +
                          player.reflexes + player.utilityUsage) / 5f;
        }

        return totalSkill / roster.Count;
    }

    private void SimulatePlayerRoundStats(List<CSPlayer> winningPlayers,
        List<CSPlayer> losingPlayers, Dictionary<CSPlayer, PlayerPerformance> playerStats,
        RoundResult roundResult, bool tWinsRound)
    {
        foreach (var player in winningPlayers)
        {
            int kills = Random.Range(0, 3); // Weighted toward winning team
            if (player.aim > 75) kills += Random.Range(0, 2);

            int deaths = Random.Range(0, 1);
            int damage = kills * 35 + Random.Range(5, 25);

            PlayerRoundStat stat = new()
            {
                player = player,
                kills = kills,
                deaths = deaths,
                damage = damage,
                wasMVP = Random.value < 0.25f,
                plantedBomb = tWinsRound && Random.value < 0.3f,
                defusedBomb = false
            };

            if (stat.wasMVP)
            {
                playerStats[player].roundsMVP++;
            }

            playerStats[player].kills += kills;
            playerStats[player].deaths += deaths;
            roundResult.playerStats.Add(stat);
        }

        foreach (var player in losingPlayers)
        {
            int kills = Random.Range(0, 2);
            int deaths = Random.Range(1, 3);
            int damage = kills * 35 + Random.Range(5, 15);

            PlayerRoundStat stat = new()
            {
                player = player,
                kills = kills,
                deaths = deaths,
                damage = damage,
                wasMVP = Random.value < 0.1f,
                plantedBomb = false,
                defusedBomb = !tWinsRound && Random.value < 0.15f
            };

            if (stat.wasMVP)
            {
                playerStats[player].roundsMVP++;
            }

            playerStats[player].kills += kills;
            playerStats[player].deaths += deaths;
            roundResult.playerStats.Add(stat);
        }
    }

    private void CalculateRating(PlayerPerformance performance)
    {
        // HLTV 2.0 style rating calculation
        float kills = performance.kills;
        float deaths = performance.deaths;
        float rounds = 16f; // Approximation

        if (deaths == 0) deaths = 1;

        // Simplified HLTV 2.0 Rating
        performance.ratingPerformance = (kills + performance.assists * 0.3f) /
                                       Mathf.Max(1, deaths / rounds);

        // Adjust for consistency and other factors
        float playerSkillAverage = (performance.player.aim +
                                   performance.player.consistency +
                                   performance.player.reflexes) / 3f;

        performance.ratingPerformance *= (playerSkillAverage / 100f);
    }
}

/// <summary>
/// Placeholder for Team class - implement based on your actual Team structure
/// </summary>
public class Team
{
    public string teamName;
    public List<CSPlayer> roster;

    public List<CSPlayer> GetActiveRoster()
    {
        return roster.Take(5).ToList(); // Get starting 5
    }
}
