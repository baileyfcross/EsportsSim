using System;
using System.Collections.Generic;
using UnityEngine;

public class CSLeagueSystem : MonoBehaviour
{
    public List<TournamentOrganizer> organizers = new();
    public List<Tournament> allTournaments = new();

    private Dictionary<string, TournamentOrganizer> organizerMap = new();

    private void Start()
    {
        InitializeOrganizers();
    }

    private void InitializeOrganizers()
    {
        // Setup tournament organizers
        organizers.Add(new TournamentOrganizer { name = "ESL", prestige = 9 });
        organizers.Add(new TournamentOrganizer { name = "BLAST", prestige = 9 });
        organizers.Add(new TournamentOrganizer { name = "PGL", prestige = 8 });

        foreach (var org in organizers)
        {
            organizerMap[org.name] = org;
        }
    }

    public void CreateTournament(string name, TournamentTier tier, string organizerName,
        int prizePool, TournamentFormat format, DateTime startDate, DateTime endDate)
    {
        Tournament tournament = new Tournament
        {
            name = name,
            tier = tier,
            organizerName = organizerName,
            prizePool = prizePool,
            format = format,
            startDate = startDate,
            endDate = endDate,
            currentStage = TournamentStage.NotStarted
        };

        allTournaments.Add(tournament);

        if (organizerMap.ContainsKey(organizerName))
        {
            organizerMap[organizerName].AddTournament(tournament);
        }

        Debug.Log($"Tournament created: {name} by {organizerName}");
    }

    public TournamentOrganizer GetOrganizer(string name)
    {
        return organizerMap.ContainsKey(name) ? organizerMap[name] : null;
    }
}



[System.Serializable]
public class TournamentOrganizer
{
    public string name;
    public int prestige;

    [SerializeField]
    private List<string> tournamentIds = new(); // Store GUIDs instead

    public void AddTournament(Tournament tournament)
    {
        if (!tournamentIds.Contains(tournament.TournamentId))
            tournamentIds.Add(tournament.TournamentId);
    }
}

[System.Serializable]
public class Tournament
{
    public string name; // "ESL Pro League Season 20"
    public TournamentTier tier;

    // SOLUTION 1: Use only the organizer's NAME/ID, not the full reference
    public string organizerName; // Store name instead of full object reference

    // SOLUTION 2: Use [SerializeReference] for polymorphic serialization
    // This avoids circular dependencies
    // public TournamentOrganizer organizer; // REMOVE THIS

    public int prizePool;
    public List<CSTeam> invitedTeams = new();
    public List<CSTeam> qualifiedTeams = new();
    public TournamentFormat format;
    public List<Map> mapPool = new();
    public DateTime startDate;
    public DateTime endDate;
    public string location;
    public bool isLAN;

    public TournamentStage currentStage;
    public List<CSMatch> scheduledMatches = new();
    public List<CSMatch> completedMatches = new();

    public CSTeam champion;
    public CSTeam runnerUp;
    public List<CSTeam> semifinals = new();

    [SerializeField]
    private string tournamentId; // Unique GUID

    public string TournamentId => tournamentId;
    public Tournament()
    {
        tournamentId = System.Guid.NewGuid().ToString();
    }
    public void GenerateTournamentSchedule()
    {
        // TODO Implementation
    }

    public void AdvanceTournament()
    {
        // TODO Implementation
    }

    public Dictionary<CSTeam, int> CalculateTeamPoints()
    {
        return new Dictionary<CSTeam, int>();
    }
}

[System.Serializable]
public enum TournamentTier
{
    Major,
    STier,
    ATier,
    BTier,
    CTier
}

[System.Serializable]
public enum TournamentFormat
{
    Swiss,
    GSLGroups,
    RoundRobin,
    DoubleElimination,
    SingleElimination,
    GroupsToPlayoffs
}

[System.Serializable]
public enum TournamentStage
{
    NotStarted,
    GroupStage,
    Quarterfinals,
    Semifinals,
    GrandFinal,
    Completed
}

[System.Serializable]
public class CSMatch
{
    public CSTeam teamA;
    public CSTeam teamB;
    public DateTime scheduledDate;
    public bool isCompleted;
    public List<CSMapResult> mapResults = new();
    public MatchFormat format;
}

[System.Serializable]
public enum MatchFormat
{
    BO1,
    BO3,
    BO5
}

[System.Serializable]
public class CSMapResult
{
    public Map map;
    public int teamAScore;
    public int teamBScore;
    public int teamAT_Score;
    public int teamACT_Score;
    public int teamBT_Score;
    public int teamBCT_Score;
    public CSTeam winner;
    public bool overtimePlayed;
    public int overtimeRounds;

    public Dictionary<CSPlayer, PlayerMapStats> playerStats = new();
    public List<HighlightMoment> highlights = new();
}

[System.Serializable]
public class PlayerMapStats
{
    public CSPlayer player;
    public int kills;
    public int deaths;
    public float rating;
}

[System.Serializable]
public class HighlightMoment
{
    public int roundNumber;
    public HighlightType type;
    public string description;
    public float timestamp;
}

public enum HighlightType
{
    Ace,
    Clutch,
    DoubleKill,
    Headshot,
    PlantBomb,
    DefuseBomb
}

[System.Serializable]
public class RankedTeam
{
    public CSTeam team;
    public int rank;
    public int points;
    public int change; // Movement from previous rank
}
