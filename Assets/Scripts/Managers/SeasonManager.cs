using System;
using System.Collections.Generic;
using UnityEngine;
using static ContractSystem;
using static MatchSimulationManager;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// <summary>
/// Manages season progression, save/load functionality, and timeline
/// </summary>
public class SeasonManager : MonoBehaviour
{
    [System.Serializable]
    public class SeasonData
    {
        public int seasonNumber;
        public int year;
        public DateTime seasonStart;
        public DateTime seasonEnd;
        public SeasonPhase currentPhase;
        public int daysPassed;
        public float gameSpeedMultiplier;
        public List<CSTeam> participatingTeams;
        public Tournament mainTournament;
        public List<Tournament> regionalTournaments;
    }

    [System.Serializable]
    public class Tournament
    {
        public string tournamentName;
        public TournamentType type;
        public DateTime startDate;
        public DateTime endDate;
        public List<CSTeam> participatingTeams;
        public CSTeam winner;
        public float prizePool;
        public List<MatchResult> matches;
        public TournamentStatus status;
    }

    public enum SeasonPhase { PreSeason, RegularSeason, Playoffs, Offseason }
    public enum TournamentType { Major, Regional, Online, Invitational }
    public enum TournamentStatus { Scheduled, InProgress, Completed }

    private SeasonData currentSeason;
    private List<SeasonData> seasonHistory = new();
    private string saveFilePath;

    private ContractSystem contractSystem;
    private MatchSimulationManager matchSimulationManager;
    private PlayerDevelopment playerDevelopment;

    private void Start()
    {
        contractSystem = Object.FindFirstObjectByType<ContractSystem>();
        matchSimulationManager = Object.FindFirstObjectByType<MatchSimulationManager>();
        playerDevelopment = Object.FindFirstObjectByType<PlayerDevelopment>();
        saveFilePath = Application.persistentDataPath + "/esports_sim_save.json";
    }

    public void StartNewSeason(int seasonNumber, int year, List<CSTeam> teams)
    {
        currentSeason = new SeasonData
        {
            seasonNumber = seasonNumber,
            year = year,
            seasonStart = DateTime.Now,
            seasonEnd = DateTime.Now.AddMonths(9), // 9 month season
            currentPhase = SeasonPhase.PreSeason,
            daysPassed = 0,
            gameSpeedMultiplier = 1.0f,
            participatingTeams = teams,
            mainTournament = new Tournament
            {
                tournamentName = $"CS:GO Championship Season {seasonNumber}",
                type = TournamentType.Major,
                startDate = DateTime.Now.AddMonths(2),
                endDate = DateTime.Now.AddMonths(6),
                participatingTeams = teams,
                matches = new List<MatchResult>(),
                status = TournamentStatus.Scheduled
            },
            regionalTournaments = new List<Tournament>()
        };

        Debug.Log($"Season {seasonNumber} started with {teams.Count} teams");
    }

    public void ProgressDay()
    {
        if (currentSeason == null) return;

        currentSeason.daysPassed++;

        // Update phase based on progression
        UpdateSeasonPhase();

        // Process daily events
        ProcessDailyEvents();

        // Update player development
        if (playerDevelopment != null)
        {
            playerDevelopment.UpdatePlayerStats();
        }

        // Check for contract expirations
        CheckContractExpirations();

        Debug.Log($"Day {currentSeason.daysPassed} progressed. Current Phase: {currentSeason.currentPhase}");
    }

    public void ProgressWeek()
    {
        for (int i = 0; i < 7; i++)
        {
            ProgressDay();
        }
    }

    public void ProgressMonth()
    {
        for (int i = 0; i < 30; i++)
        {
            ProgressDay();
        }

        // Process monthly salaries
        if (contractSystem != null)
        {
            contractSystem.ProcessMonthlySalaries();
        }
    }

    private void UpdateSeasonPhase()
    {
        float seasonProgress = (float)currentSeason.daysPassed /
            (float)(currentSeason.seasonEnd - currentSeason.seasonStart).TotalDays;

        if (seasonProgress < 0.2f)
        {
            currentSeason.currentPhase = SeasonPhase.PreSeason;
        }
        else if (seasonProgress < 0.7f)
        {
            currentSeason.currentPhase = SeasonPhase.RegularSeason;
        }
        else if (seasonProgress < 0.9f)
        {
            currentSeason.currentPhase = SeasonPhase.Playoffs;
        }
        else
        {
            currentSeason.currentPhase = SeasonPhase.Offseason;
        }
    }

    private void ProcessDailyEvents()
    {
        // Randomly generate events like injuries, drama, player form changes
        if (Random.value < 0.05f) // 5% chance per day
        {
            GenerateRandomEvent();
        }
    }

    private void GenerateRandomEvent()
    {
        if (currentSeason.participatingTeams.Count == 0) return;

        CSTeam randomTeam = currentSeason.participatingTeams[Random.Range(0, currentSeason.participatingTeams.Count)];
        List<CSPlayer> roster = randomTeam.GetActiveRoster();

        if (roster.Count == 0) return;

        CSPlayer randomPlayer = roster[Random.Range(0, roster.Count)];

        int eventType = Random.Range(0, 3);
        switch (eventType)
        {
            case 0: // Form change
                randomPlayer.consistency += Random.Range(-5, 6);
                Debug.Log($"{randomPlayer.playerName}'s form changed");
                break;
            case 1: // Minor injury
                randomPlayer.reflexes -= Random.Range(3, 8);
                Debug.Log($"{randomPlayer.playerName} suffered a minor injury");
                break;
            case 2: // Confidence boost
                randomPlayer.consistency += Random.Range(3, 8);
                Debug.Log($"{randomPlayer.playerName} gained confidence");
                break;
        }
    }

    private void CheckContractExpirations()
    {
        if (contractSystem == null) return;

        foreach (var team in currentSeason.participatingTeams)
        {
            List<PlayerContract> contracts = contractSystem.GetTeamContracts(team);
            foreach (var contract in contracts)
            {
                if (contract.IsExpiringSoon())
                {
                    Debug.Log($"Contract expiring soon: {contract.player.playerName} - {contract.GetRemainingMonths():F1} months left");
                }
            }
        }
    }

    public void SaveGame()
    {
        try
        {
            GameSaveData saveData = new()
            {
                seasonData = currentSeason,
                lastSaveTime = DateTime.Now
            };

            string json = JsonUtility.ToJson(saveData, true);
            System.IO.File.WriteAllText(saveFilePath, json);
            Debug.Log($"Game saved successfully to {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public void LoadGame()
    {
        try
        {
            if (!System.IO.File.Exists(saveFilePath))
            {
                Debug.LogWarning($"Save file not found at {saveFilePath}");
                return;
            }

            string json = System.IO.File.ReadAllText(saveFilePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

            currentSeason = saveData.seasonData;
            Debug.Log($"Game loaded successfully. Season: {currentSeason.seasonNumber}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
        }
    }

    public SeasonData GetCurrentSeason()
    {
        return currentSeason;
    }

    public void SetGameSpeed(float speedMultiplier)
    {
        if (currentSeason != null)
        {
            currentSeason.gameSpeedMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 5.0f);
            Time.timeScale = currentSeason.gameSpeedMultiplier;
        }
    }

    public float GetGameSpeed()
    {
        return currentSeason != null ? currentSeason.gameSpeedMultiplier : 1.0f;
    }
}

[System.Serializable]
public class GameSaveData
{
    public SeasonManager.SeasonData seasonData;
    public DateTime lastSaveTime;
}