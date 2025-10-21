using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSUIManager : MonoBehaviour
{
    [Header("Main Screen References")]
    public GameObject mainMenuPanel;
    public GameObject teamManagementPanel;
    public GameObject tournamentPanel;
    public GameObject matchDayPanel;
    public GameObject playerScoutingPanel;
    public GameObject transferNegotiationPanel;
    public GameObject strategyPanel;

    [Header("Player Information")]
    public GameObject playerProfilePrefab;
    public Transform playerListContainer;

    [Header("CS-Specific UI")]
    public GameObject mapSelectionPanel;
    public GameObject stratCreatorPanel;
    public GameObject matchHistoryPanel;
    public GameObject tournamentBracketPanel;
    public GameObject worldRankingsPanel;

    [Header("Navigation")]
    public Button continueButton; // Advance to next day

    // References to specific UI elements
    public Text currentDateText;
    public Text teamBudgetText;
    public Text upcomingMatchText;
    public Text worldRankingText;

    // CS HUD elements for match simulation
    public GameObject matchSimulationPanel;
    public Image minimapImage;
    public Text roundTimerText;
    public Text scoreText;
    public Text economyText;
    public GameObject killfeedPanel;

    // Manager for tab-based navigation (like in FM)
    public TabNavigationManager tabManager;

    private void Start()
    {
        SetupEventListeners();
        UpdateAllDisplays();
    }

    public void ShowPlayerProfile(CSPlayer player)
    {
        // Instantiate and populate CS player profile with data
        GameObject profile = Instantiate(playerProfilePrefab, playerListContainer);
        CSPlayerProfileUI profileUI = profile.GetComponent<CSPlayerProfileUI>();
        profileUI.PopulateProfile(player);
    }

    public void UpdateTeamRoster(List<CSPlayer> players)
    {
        // Clear and rebuild the roster display
        ClearContainer(playerListContainer);

        foreach (CSPlayer player in players)
        {
            ShowPlayerProfile(player);
        }
    }

    public void DisplayMatchResult(CSMatch match)
    {
        // Show post-match analysis and statistics
        matchDayPanel.SetActive(true);

        // Populate with match data
        if (match.isCompleted)
        {
            // Show each map result
            foreach (CSMapResult mapResult in match.mapResults)
            {
                DisplayMapResult(mapResult);
            }

            // Show match MVP
            CSPlayer mvp = DetermineMVP(match);
            DisplayMatchMVP(mvp);

            // Show highlight clips
            DisplayMatchHighlights(match);
        }
    }

    private void DisplayMapResult(CSMapResult mapResult)
    {
        // Show map score and key stats
        // Create a panel for this map result
    }

    private CSPlayer DetermineMVP(CSMatch match)
    {
        // Determine match MVP based on stats
        // Placeholder implementation - would calculate based on overall performance
        return match.teamA.activeRoster[0];
    }

    private void DisplayMatchMVP(CSPlayer mvp)
    {
        // Show the MVP player and their stats
    }

    private void DisplayMatchHighlights(CSMatch match)
    {
        // Show key moments from the match
        foreach (CSMapResult mapResult in match.mapResults)
        {
            foreach (HighlightMoment highlight in mapResult.highlights)
            {
                // Create UI for this highlight
                CreateHighlightDisplay(highlight);
            }
        }
    }

    private void CreateHighlightDisplay(HighlightMoment highlight)
    {
        // Create UI element for a highlight moment
    }

    public void ShowStrategyCreator(Map selectedMap)
    {
        // Open the strategy creation tool for a specific map
        stratCreatorPanel.SetActive(true);

        // Set up the map-specific elements
        // e.g., Load minimap image, set up drag points for utility/positions
        if (selectedMap != null && selectedMap.mapMinimap != null)
        {
            // Load the map minimap for strategy visualization
        }
    }

    public void DisplayWorldRankings(List<RankedTeam> rankings)
    {
        // Show current world rankings
        worldRankingsPanel.SetActive(true);

        // Create UI for each ranked team
        foreach (RankedTeam rankedTeam in rankings)
        {
            CreateRankingEntry(rankedTeam);
        }
    }

    private void CreateRankingEntry(RankedTeam rankedTeam)
    {
        // Create a UI entry showing team rank, points, movement
    }

    public void ShowTournamentBracket(Tournament tournament)
    {
        // Display tournament structure (bracket or groups)
        tournamentBracketPanel.SetActive(true);

        if (tournament.format == TournamentFormat.SingleElimination ||
            tournament.format == TournamentFormat.DoubleElimination)
        {
            CreateEliminationBracket(tournament);
        }
        else if (tournament.format == TournamentFormat.Swiss)
        {
            CreateSwissFormat(tournament);
        }
        else if (tournament.format == TournamentFormat.GroupsToPlayoffs)
        {
            CreateGroupStageDisplay(tournament);
        }
    }

    private void CreateEliminationBracket(Tournament tournament)
    {
        // Create visual bracket for elimination tournaments
    }

    private void CreateSwissFormat(Tournament tournament)
    {
        // Create UI for Swiss system
    }

    private void CreateGroupStageDisplay(Tournament tournament)
    {
        // Create group tables and standings
    }

    public void SetupEventListeners()
    {
        continueButton.onClick.AddListener(() =>
        {
            CSTeamManager manager = UnityEngine.Object.FindFirstObjectByType<CSTeamManager>();
            manager.AdvanceDay();
            UpdateAllDisplays();
        });

        // Setup other UI event listeners
    }

    public void UpdateAllDisplays()
    {
        CSTeamManager manager = UnityEngine.Object.FindFirstObjectByType<CSTeamManager>();

        // Update date and basic info
        currentDateText.text = manager.currentDate.ToString("dd MMM yyyy");
        teamBudgetText.text = "$" + manager.budget.ToString("N0");

        // Check for upcoming match
        CSMatch nextMatch = FindNextScheduledMatch();
        if (nextMatch != null)
        {
            upcomingMatchText.text = $"Next Match: {nextMatch.teamA.name} vs {nextMatch.teamB.name}";
        }
        else
        {
            upcomingMatchText.text = "No upcoming matches";
        }

        // Update world ranking display
        worldRankingText.text = "#" + manager.managedTeam.worldRanking;

        // Update other displays
        UpdateTeamRoster(manager.managedTeam.activeRoster);
    }

    private CSMatch FindNextScheduledMatch()
    {
        CSTeamManager manager = UnityEngine.Object.FindFirstObjectByType<CSTeamManager>();

        // Find closest upcoming match date
        CSMatch nextMatch = null;
        DateTime closestDate = DateTime.MaxValue;

        foreach (CSMatch match in manager.upcomingMatches)
        {
            if (match.scheduledDate > manager.currentDate && match.scheduledDate < closestDate)
            {
                closestDate = match.scheduledDate;
                nextMatch = match;
            }
        }

        return nextMatch;
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    // Live match simulation UI
    public void StartMatchSimulationView(CSMatch match)
    {
        matchSimulationPanel.SetActive(true);

        // Set up the simulation view
        UpdateScoreDisplay(0, 0);

        // Show the current map
        UpdateMinimapDisplay(match.mapsToPlay[0]);

        // Show starting player positions
        ShowPlayerPositionsOnMap(match);
    }

    private void UpdateScoreDisplay(int teamAScore, int teamBScore)
    {
        scoreText.text = $"{teamAScore} - {teamBScore}";
    }

    private void UpdateMinimapDisplay(Map map)
    {
        // Set the minimap image to the current map
        // This would need actual map minimap sprites
        if (map != null && map.mapMinimap != null)
        {
            minimapImage.sprite = map.mapMinimap;
        }
    }

    private void ShowPlayerPositionsOnMap(CSMatch match)
    {
        // Create dots on minimap for player positions
    }

    public void AddKillfeedEntry(CSPlayer killer, CSPlayer victim, string weaponUsed)
    {
        // Add a kill to the killfeed UI
        GameObject killfeedEntry = new("KillfeedEntry");
        killfeedEntry.transform.SetParent(killfeedPanel.transform, false);

        // Create killfeed text element
        Text killfeedText = killfeedEntry.AddComponent<Text>();
        killfeedText.text = $"{killer.nickName} [{weaponUsed}] {victim.nickName}";

        // Make entries fade out and move up
        Destroy(killfeedEntry, 5f);
    }
}

// Specialized UI components
[System.Serializable]
public class CSPlayerProfileUI : MonoBehaviour
{
    public Text playerNameText;
    public Text nicknameText;
    public Text roleText;
    public Text nationalityText;
    public Text ageText;
    public Image playerImage;

    [Header("CS Stat Display")]
    public Slider aimSlider;
    public Slider reactionTimeSlider;
    public Slider positioningSlider;
    public Slider utilityUsageSlider;
    public Slider clutchSlider;

    [Header("Performance Stats")]
    public Text ratingText;
    public Text kprText; // Kills per round
    public Text headspotText;
    public Text adrText; // Average damage per round

    public void PopulateProfile(CSPlayer player)
    {
        playerNameText.text = $"{player.firstName} {player.lastName}";
        nicknameText.text = player.nickName;
        roleText.text = player.preferredRole.ToString();
        nationalityText.text = player.nationality;
        ageText.text = player.age.ToString();

        if (player.playerPhoto != null)
        {
            playerImage.sprite = player.playerPhoto;
        }

        // Set skill sliders
        aimSlider.value = player.aim / 20f;
        reactionTimeSlider.value = player.reactionTime / 20f;
        positioningSlider.value = player.positioning / 20f;
        utilityUsageSlider.value = player.utilityUsage / 20f;
        clutchSlider.value = player.clutchAbility / 20f;

        // Set performance stats
        ratingText.text = player.averageHLTVRating.ToString("F2");
        kprText.text = player.killsPerRound.ToString("F2");
        headspotText.text = player.headshotPercentage.ToString("F1") + "%";
        adrText.text = player.averageDamagePerRound.ToString("F1");
    }
}