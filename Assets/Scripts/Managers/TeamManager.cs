using System;
using System.Collections.Generic;
using UnityEngine;

public class CSTeamManager : MonoBehaviour
{
    public CSTeam managedTeam;
    public DateTime currentDate;
    public int budget;
    public int salaryBudget;
    public List<CSPlayer> scoutedPlayers = new();
    public List<CSStrat> teamStrats = new();
    public Dictionary<string, CSDefaultSetup> mapDefaults = new();
    public List<PracticeSession> weeklyPracticeSchedule = new();
    public List<CSManagementObjective> currentObjectives = new();
    public List<CSMatch> upcomingMatches = new();
    public List<Tournament> registeredTournaments = new();

    // Management functions
    public void AdvanceDay()
    {
        currentDate = currentDate.AddDays(1);
        CheckForScheduledMatches();
        RunPlayerTraining();
        ProcessTeamChemistry();
        CheckForTransferOffers();
        UpdateOrgConfidence();
        UpdateFanSatisfaction();
    }

    public void SetTeamLineup(List<CSPlayer> startingLineup, Dictionary<PlayerRole, CSPlayer> roleAssignments)
    {
        // Validate and set the active lineup and roles
        if (startingLineup.Count != 5)
        {
            Debug.LogError("CS teams must have exactly 5 players in starting lineup");
            return;
        }

        managedTeam.activeRoster = startingLineup;
        managedTeam.roleAssignments = roleAssignments;

        // Recalculate team strengths
        managedTeam.UpdateMapPoolStrengths();
    }

    public void NegotiatePlayerContract(CSPlayer player, int offeredSalary, int years)
    {
        // Attempt to sign or re-sign a player
        float chanceOfAccepting = CalculateContractAcceptanceProbability(player, offeredSalary, years);

        if (UnityEngine.Random.Range(0f, 100f) < chanceOfAccepting)
        {
            // Player accepts
            SignPlayer(player, offeredSalary, years);
        }
        else
        {
            // Player rejects or counters
            GenerateCounterOffer(player);
        }
    }

    public void CreateTeamStrat(Map map, string stratName, CSTeamSide side, string description)
    {
        // Create a new team strategy/execute
        CSStrat strat = new()
        {
            map = map,
            name = stratName,
            side = side,
            description = description
        };

        // Add player positions and utility usage

        teamStrats.Add(strat);
    }

    public void ScoutPlayer(CSPlayer player)
    {
        // Reveal more information about a potential signing
        // Initially player stats are partially hidden/uncertain
        scoutedPlayers.Add(player);
    }

    public void ScheduleScrim(CSTeam opponent, DateTime date)
    {
        // Set up practice match against another team
        CSMatch scrimMatch = new CSMatch
        {
            teamA = managedTeam,
            teamB = opponent,
            scheduledDate = date,
            format = MatchFormat.BO1 // Typical for scrims
        };

        // Add to upcoming matches
        upcomingMatches.Add(scrimMatch);
    }

    public void SetPracticeSchedule(List<PracticeSession> weeklySchedule)
    {
        // Set the team's weekly practice routine
        weeklyPracticeSchedule = weeklySchedule;
    }

    public void RegisterForTournament(Tournament tournament)
    {
        // Attempt to join a tournament (may need qualification)
        if (tournament.invitedTeams.Contains(managedTeam))
        {
            // Automatically in if invited
            registeredTournaments.Add(tournament);
        }
        else
        {
            // Need to go through qualifiers
            // Logic for tournament qualification system
        }
    }

    public PreMatchPlan CreateMatchPlan(CSMatch match)
    {
        // Prepare strategy for an upcoming match
        PreMatchPlan plan = new PreMatchPlan
        {
            match = match
        };

        // Set map preferences, strats, etc.

        return plan;
    }

    private float CalculateContractAcceptanceProbability(CSPlayer player, int offeredSalary, int years)
    {
        // Calculate if player will accept based on:
        // - Team ranking vs player skill
        // - Offered salary vs market value
        // - Contract length
        // - Player's relationship with organization
        // - Team's recent performance

        float baseChance = 50f;

        // Adjust based on salary
        int expectedSalary = player.marketValue;
        float salaryFactor = (float)offeredSalary / expectedSalary;
        baseChance += (salaryFactor - 1) * 30f; // +30% if offering double expected salary

        // Adjust based on team ranking
        int rankDiff = 20 - managedTeam.worldRanking; // Higher ranked = better chance
        baseChance += rankDiff * 1.5f;

        // Adjust based on years (players might prefer shorter or longer contracts)
        if (years > 2)
        {
            baseChance -= 5f * (years - 2); // Penalty for long contracts
        }

        return Mathf.Clamp(baseChance, 5f, 95f); // Always some chance of yes/no
    }

    private void SignPlayer(CSPlayer player, int salary, int years)
    {
        // Add player to team and update finances
        player.salary = salary;
        player.contractYearsRemaining = years;

        if (!managedTeam.activeRoster.Contains(player) && !managedTeam.benchPlayers.Contains(player))
        {
            // New signing
            managedTeam.benchPlayers.Add(player);
        }

        // Update team finances
        salaryBudget -= salary;
        budget -= 0; // Potential transfer fee logic
    }

    private void GenerateCounterOffer(CSPlayer player)
    {
        // Player/agent provides counter offer
        int desiredSalary = Mathf.RoundToInt(player.marketValue * UnityEngine.Random.Range(1.1f, 1.3f));
        int desiredYears = UnityEngine.Random.Range(1, 3);

        // Present counter offer to player (via UI)
    }

    private void RunPlayerTraining()
    {
        // Based on practice schedule, improve player skills slightly
        foreach (CSPlayer player in managedTeam.activeRoster)
        {
            // Find today's practice focus
            PracticeSession todaySession = GetTodaysPracticeSession();
            if (todaySession == null) return;

            // Improve relevant skills based on practice focus
            switch (todaySession.focus)
            {
                case PracticeFocus.AimTraining:
                    player.aim += (int)UnityEngine.Random.Range(0.01f, 0.05f);
                    break;
                case PracticeFocus.UtilityLineups:
                    player.utilityUsage += (int)UnityEngine.Random.Range(0.01f, 0.05f);
                    break;
                case PracticeFocus.StratPractice:
                    // Improve team synergy
                    break;
                case PracticeFocus.MapStudy:
                    // Improve map knowledge
                    if (todaySession.targetMap != null)
                    {
                        if (player.mapProficiency.ContainsKey(todaySession.targetMap.mapName))
                        {
                            player.mapProficiency[todaySession.targetMap.mapName] +=
                                (int)UnityEngine.Random.Range(0.01f, 0.05f);
                        }
                    }
                    break;
                case PracticeFocus.VODReview:
                    player.gamesense += (int)UnityEngine.Random.Range(0.01f, 0.03f);
                    break;
            }

            // Cap stats at maximum
            ClampPlayerStats(player);
        }
    }

    private PracticeSession GetTodaysPracticeSession()
    {
        // Find the practice session for today based on day of week
        int dayOfWeek = (int)currentDate.DayOfWeek;
        foreach (PracticeSession session in weeklyPracticeSchedule)
        {
            if (session.dayOfWeek == dayOfWeek)
            {
                return session;
            }
        }
        return null; // No practice today
    }

    private void ClampPlayerStats(CSPlayer player)
    {
        // Ensure all stats are within valid range (1-20)
        player.aim = Mathf.Clamp(player.aim, 1, 20);
        player.reactionTime = Mathf.Clamp(player.reactionTime, 1, 20);
        player.positioning = Mathf.Clamp(player.positioning, 1, 20);
        player.utilityUsage = Mathf.Clamp(player.utilityUsage, 1, 20);
        player.clutchAbility = Mathf.Clamp(player.clutchAbility, 1, 20);
        player.consistency = Mathf.Clamp(player.consistency, 1, 20);
        player.mentalFortitude = Mathf.Clamp(player.mentalFortitude, 1, 20);
        player.gamesense = Mathf.Clamp(player.gamesense, 1, 20);
        player.movementSkill = Mathf.Clamp(player.movementSkill, 1, 20);
    }

    private void CheckForScheduledMatches()
    {
        // Check if any matches are scheduled for today
        foreach (CSMatch match in upcomingMatches)
        {
            if (match.scheduledDate.Date == currentDate.Date)
            {
                // Simulate or prepare for match
                Debug.Log($"Match today: {match.teamA.name} vs {match.teamB.name}");
            }
        }
    }

    private void ProcessTeamChemistry()
    {
        // Update team chemistry based on time playing together, results, etc.
        if (managedTeam != null)
        {
            // Chemistry improves slowly over time
            managedTeam.teamChemistry += 0.1f;
            managedTeam.teamChemistry = Mathf.Clamp(managedTeam.teamChemistry, 0, 100);
        }
    }

    private void CheckForTransferOffers()
    {
        // Check if any other teams are interested in your players
        // Or if there are players available on the market
    }

    private void UpdateOrgConfidence()
    {
        // Update organization confidence based on recent results
        // Confidence affects budget and job security
    }

    private void UpdateFanSatisfaction()
    {
        // Update fan satisfaction based on results and team changes
    }
}

[System.Serializable]
public class PracticeSession
{
    public int dayOfWeek; // 0 = Sunday, 1 = Monday, etc.
    public PracticeFocus focus;
    public int durationHours;
    public Map targetMap; // For map-specific practice
    public CSStrat targetStrat; // For strat practice
}

public enum PracticeFocus
{
    AimTraining,
    UtilityLineups,
    StratPractice,
    VODReview,
    MapStudy,
    Scrimmage,
    TeamBuilding,
    Rest
}

[System.Serializable]
public class CSStrat
{
    public string name; // "A Split", "B Execute", etc.
    public Map map;
    public CSTeamSide side; // T or CT
    public string description;
    public List<PlayerPosition> playerPositions = new List<PlayerPosition>();
    public List<UtilityUsage> utilitySequence = new List<UtilityUsage>();
    public int successRate; // Tracked success rate in matches
    public int timesUsed;
}

public enum CSTeamSide
{
    Terrorist,
    CounterTerrorist
}

[System.Serializable]
public class PlayerPosition
{
    public string positionName; // "A Main", "Palace", etc.
    public PlayerRole preferredRole; // Who should play this position
    public Vector2 mapPosition; // For UI visualization
}

[System.Serializable]
public class UtilityUsage
{
    public CSGrenadeType grenadeType;
    public string purpose; // "Smoke CT", "Flash site entry", etc.
    public Vector2 throwPosition;
    public Vector2 landPosition;
    public int throwTiming; // Seconds into the execute
}

public enum CSGrenadeType
{
    Smoke,
    Flash,
    HE,
    Molotov,
    Decoy
}

[System.Serializable]
public class CSDefaultSetup
{
    public Map map;
    public CSTeamSide side;
    public List<PlayerPosition> defaultPositions = new List<PlayerPosition>();
    public List<string> responsibilities = new List<string>(); // What each player watches/controls
}

[System.Serializable]
public class CSManagementObjective
{
    public string description;
    public CSObjectiveType type;
    public int targetValue;
    public int currentValue;
    public int rewardMoney;
    public float rewardOrgConfidence;

    public enum CSObjectiveType
    {
        TournamentPlacement,
        PlayerDevelopment,
        TeamRanking,
        FinancialTarget,
        FanGrowth
    }
}

[System.Serializable]
public class PreMatchPlan
{
    public CSMatch match;

    // Map veto strategy
    public List<Map> mapsToVeto = new List<Map>();
    public List<Map> mapPreferences = new List<Map>();

    // Strat selection per map
    public Dictionary<Map, List<CSStrat>> selectedStrats = new Dictionary<Map, List<CSStrat>>();

    // Focus on specific opponents
    public List<CSPlayer> targetPlayers = new List<CSPlayer>();
}