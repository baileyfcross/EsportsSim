using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ContractSystem;
using Object = UnityEngine.Object;

/// <summary>
/// Centralized UI Manager for all pages and displays
/// Integrates core game systems with UI display
/// </summary>
public class UIManager : MonoBehaviour
{
    public enum UIPage
    {
        Dashboard,
        TeamManagement,
        Contracts,
        TransferMarket,
        MatchResults,
        SeasonCalendar,
        PlayerDevelopment,
        Strategy,
        WorldRankings,
        Tournament
    }

    [Header("Main Screen Panels")]
    public GameObject dashboardPanel;
    public GameObject teamManagementPanel;
    public GameObject contractsPanel;
    public GameObject transferMarketPanel;
    public GameObject matchResultsPanel;
    public GameObject seasonCalendarPanel;
    public GameObject playerDevelopmentPanel;
    public GameObject strategyPanel;
    public GameObject worldRankingsPanel;
    public GameObject tournamentPanel;

    [Header("Navigation Buttons")]
    public Button dashboardButton;
    public Button teamButton;
    public Button contractsButton;
    public Button transferButton;
    public Button matchesButton;
    public Button seasonButton;
    public Button playersButton;
    public Button strategyButton;
    public Button rankingsButton;
    public Button saveButton;
    public Button loadButton;
    public Button continueButton; // Advance to next day

    [Header("Player Information")]
    public GameObject playerProfilePrefab;
    public Transform playerListContainer;

    [Header("Shared Data Container")]
    public Transform dataContainer;
    public GameObject itemPrefab;

    [Header("Global UI Elements")]
    public Text currentDateText;
    public Text teamBudgetText;
    public Text upcomingMatchText;
    public Text worldRankingText;

    [Header("CS-Specific UI")]
    public GameObject mapSelectionPanel;
    public GameObject stratCreatorPanel;
    public GameObject matchHistoryPanel;
    public GameObject tournamentBracketPanel;
    public GameObject matchSimulationPanel;
    public Image minimapImage;
    public Text roundTimerText;
    public Text scoreText;
    public Text economyText;
    public GameObject killfeedPanel;

    [Header("Match Display")]
    public Text matchTitleText;
    public Text matchDateText;
    public Text mapNameText;
    public Text teamANameText;
    public Text teamBNameText;
    public Text teamAScoreText;
    public Text teamBScoreText;

    [Header("Contract Display")]
    public Text totalBudgetText;
    public Text spentOnSalariesText;
    public Text availableBudgetText;
    public Image budgetProgressBar;

    // Manager for tab-based navigation
    public TabNavigationManager tabManager;

    // Core system references
    private MatchSimulationManager matchSimulation;
    private ContractSystem contractSystem;
    private TransferMarket transferMarket;
    private SeasonManager seasonManager;
    private PlayerDevelopment playerDevelopment;
    private CSTeamManager teamManager;

    private UIPage currentPage = UIPage.Dashboard;
    private Dictionary<UIPage, GameObject> pageMap;

    private void Start()
    {
        InitializeReferences();
        SetupPageMap();
        SetupNavigationButtons();
        UpdateAllDisplays();
        ShowPage(UIPage.Dashboard);
    }

    private void InitializeReferences()
    {
        matchSimulation = Object.FindFirstObjectByType<MatchSimulationManager>();
        contractSystem = Object.FindFirstObjectByType<ContractSystem>();
        transferMarket = Object.FindFirstObjectByType<TransferMarket>();
        seasonManager = Object.FindFirstObjectByType<SeasonManager>();
        playerDevelopment = Object.FindFirstObjectByType<PlayerDevelopment>();
        teamManager = Object.FindFirstObjectByType<CSTeamManager>();
    }

    private void SetupPageMap()
    {
        pageMap = new Dictionary<UIPage, GameObject>
        {
            { UIPage.Dashboard, dashboardPanel },
            { UIPage.TeamManagement, teamManagementPanel },
            { UIPage.Contracts, contractsPanel },
            { UIPage.TransferMarket, transferMarketPanel },
            { UIPage.MatchResults, matchResultsPanel },
            { UIPage.SeasonCalendar, seasonCalendarPanel },
            { UIPage.PlayerDevelopment, playerDevelopmentPanel },
            { UIPage.Strategy, strategyPanel },
            { UIPage.WorldRankings, worldRankingsPanel },
            { UIPage.Tournament, tournamentPanel }
        };
    }

    private void SetupNavigationButtons()
    {
        dashboardButton.onClick.AddListener(() => ShowPage(UIPage.Dashboard));
        teamButton.onClick.AddListener(() => ShowPage(UIPage.TeamManagement));
        contractsButton.onClick.AddListener(() => ShowPage(UIPage.Contracts));
        transferButton.onClick.AddListener(() => ShowPage(UIPage.TransferMarket));
        matchesButton.onClick.AddListener(() => ShowPage(UIPage.MatchResults));
        seasonButton.onClick.AddListener(() => ShowPage(UIPage.SeasonCalendar));
        playersButton.onClick.AddListener(() => ShowPage(UIPage.PlayerDevelopment));
        strategyButton.onClick.AddListener(() => ShowPage(UIPage.Strategy));
        rankingsButton.onClick.AddListener(() => ShowPage(UIPage.WorldRankings));

        saveButton.onClick.AddListener(() => SaveGame());
        loadButton.onClick.AddListener(() => LoadGame());

        continueButton.onClick.AddListener(() => AdvanceDay());
    }

    public void ShowPage(UIPage page)
    {
        // Hide all pages first
        foreach (var panelEntry in pageMap)
        {
            if (panelEntry.Value != null)
                panelEntry.Value.SetActive(false);
        }

        // Show selected page
        if (pageMap.ContainsKey(page) && pageMap[page] != null)
        {
            pageMap[page].SetActive(true);
            currentPage = page;

            // Load page-specific data
            LoadPageData(page);
        }
    }

    private void LoadPageData(UIPage page)
    {
        switch (page)
        {
            case UIPage.Dashboard:
                DisplayDashboard();
                break;
            case UIPage.TeamManagement:
                DisplayTeamManagement();
                break;
            case UIPage.Contracts:
                DisplayContracts();
                break;
            case UIPage.TransferMarket:
                DisplayTransferMarket();
                break;
            case UIPage.MatchResults:
                DisplayMatchResults();
                break;
            case UIPage.SeasonCalendar:
                DisplaySeasonCalendar();
                break;
            case UIPage.PlayerDevelopment:
                DisplayPlayerDevelopment();
                break;
            case UIPage.Strategy:
                DisplayStrategy();
                break;
            case UIPage.WorldRankings:
                DisplayWorldRankings();
                break;
            case UIPage.Tournament:
                DisplayTournament();
                break;
        }
    }

    // PAGE-SPECIFIC DISPLAY METHODS

    private void DisplayDashboard()
    {
        ClearContainer(dataContainer);

        if (seasonManager?.GetCurrentSeason() == null)
            return;

        SeasonManager.SeasonData season = seasonManager.GetCurrentSeason();

        // Display season info
        CreateInfoPanel("Season Information",
            $"Season {season.seasonNumber} - {season.year}\n" +
            $"Phase: {season.currentPhase}\n" +
            $"Days Passed: {season.daysPassed}\n" +
            $"Date: {season.seasonStart.AddDays(season.daysPassed):dd MMM yyyy}");

        // Display upcoming matches
        CreateInfoPanel("Upcoming Matches", "Load from tournament system");

        // Display team summary
        if (teamManager != null)
        {
            CreateInfoPanel("Team Status",
                $"Team: {teamManager.managedTeam.name}\n" +
                $"Rank: #{teamManager.managedTeam.worldRanking}\n" +
                $"Budget: ${teamManager.budget:N0}");
        }
    }

    private void DisplayTeamManagement()
    {
        ClearContainer(playerListContainer);

        if (teamManager?.managedTeam == null)
            return;

        List<CSPlayer> roster = teamManager.managedTeam.GetActiveRoster();

        foreach (var player in roster)
        {
            ShowPlayerProfile(player);
        }
    }

    private void DisplayContracts()
    {
        ClearContainer(dataContainer);

        if (contractSystem == null || teamManager?.managedTeam == null)
            return;

        ContractSystem.TeamBudget budget = contractSystem.GetTeamBudget(teamManager.managedTeam);

        if (budget != null)
        {
            // Display budget overview
            totalBudgetText.text = "$" + budget.totalBudget.ToString("N0");
            spentOnSalariesText.text = "$" + budget.spentOnSalaries.ToString("N0");
            availableBudgetText.text = "$" + budget.GetAvailableBudget().ToString("N0");

            // Update budget progress bar
            float spentPercentage = budget.spentOnSalaries / budget.totalBudget;
            budgetProgressBar.fillAmount = Mathf.Clamp01(spentPercentage);

            if (spentPercentage > 0.9f)
                budgetProgressBar.color = Color.red;
            else if (spentPercentage > 0.7f)
                budgetProgressBar.color = Color.yellow;
            else
                budgetProgressBar.color = Color.green;
        }

        // Display contracts
        List<PlayerContract> contracts = contractSystem.GetTeamContracts(teamManager.managedTeam);
        foreach (var contract in contracts)
        {
            DisplayContractEntry(contract);
        }
    }

    private void DisplayTransferMarket()
    {
        ClearContainer(dataContainer);

        if (transferMarket == null)
            return;

        transferMarket.CheckExpiredListings();
        List<TransferMarket.TransferListing> listings = transferMarket.GetActiveListings();

        foreach (var listing in listings)
        {
            DisplayTransferListing(listing);
        }
    }

    private void DisplayMatchResults()
    {
        ClearContainer(dataContainer);
        CreateInfoPanel("Match Results", "Display recent match outcomes and statistics");
    }

    private void DisplaySeasonCalendar()
    {
        ClearContainer(dataContainer);

        if (seasonManager?.GetCurrentSeason() == null)
            return;

        SeasonManager.SeasonData season = seasonManager.GetCurrentSeason();

        float progressPercent = (float)season.daysPassed / (float)(season.seasonEnd - season.seasonStart).TotalDays;

        CreateInfoPanel("Season Calendar",
            $"Phase: {season.currentPhase}\n" +
            $"Progress: {(progressPercent * 100):F0}%\n" +
            $"Days Passed: {season.daysPassed}\n" +
            $"Start: {season.seasonStart:dd MMM yyyy}\n" +
            $"End: {season.seasonEnd:dd MMM yyyy}");
    }

    private void DisplayPlayerDevelopment()
    {
        ClearContainer(playerListContainer);

        if (teamManager?.managedTeam == null || playerDevelopment == null)
            return;

        List<CSPlayer> roster = teamManager.managedTeam.GetActiveRoster();

        foreach (var player in roster)
        {
            PlayerDevelopment.PlayerStats stats = playerDevelopment.GetPlayerStats(player);
            if (stats != null)
            {
                DisplayPlayerStatsEntry(player, stats);
            }
        }
    }

    private void DisplayStrategy()
    {
        ClearContainer(dataContainer);
        CreateInfoPanel("Strategy Creator", "Map-based strategy and positioning system");
    }

    private void DisplayWorldRankings()
    {
        ClearContainer(dataContainer);
        CreateInfoPanel("World Rankings", "Global team rankings and statistics");
    }

    private void DisplayTournament()
    {
        if (seasonManager?.GetCurrentSeason()?.mainTournament != null)
        {
            SeasonManager.Tournament tournament = seasonManager.GetCurrentSeason().mainTournament;
            ShowTournamentBracket(tournament);
        }
    }

    // HELPER DISPLAY METHODS

    public void ShowPlayerProfile(CSPlayer player)
    {
        GameObject profile = Instantiate(playerProfilePrefab, playerListContainer);
        CSPlayerProfileUI profileUI = profile.GetComponent<CSPlayerProfileUI>();
        profileUI.PopulateProfile(player);
    }

    public void UpdateTeamRoster(List<CSPlayer> players)
    {
        ClearContainer(playerListContainer);

        foreach (CSPlayer player in players)
        {
            ShowPlayerProfile(player);
        }
    }

    private void DisplayContractEntry(PlayerContract contract)
    {
        GameObject item = Instantiate(itemPrefab, dataContainer);
        Text[] texts = item.GetComponentsInChildren<Text>();

        if (texts.Length >= 4)
        {
            texts[0].text = contract.player.playerName;
            texts[1].text = "$" + contract.monthlySalary.ToString("N0") + "/mo";
            texts[2].text = "Expires: " + contract.contractEnd.ToString("dd MMM yyyy");
            texts[3].text = contract.IsExpiringSoon() ? "⚠ EXPIRING SOON" : "Active";
        }
    }

    private void DisplayTransferListing(TransferMarket.TransferListing listing)
    {
        GameObject item = Instantiate(itemPrefab, dataContainer);
        Text[] texts = item.GetComponentsInChildren<Text>();

        if (texts.Length >= 4)
        {
            texts[0].text = listing.player.playerName;
            texts[1].text = "From: " + listing.currentTeam.name;
            texts[2].text = "Ask: $" + listing.askingPrice.ToString("N0");
            texts[3].text = listing.GetDaysRemaining() + " days | " + listing.offers.Count + " offers";
        }
    }

    private void DisplayPlayerStatsEntry(CSPlayer player, PlayerDevelopment.PlayerStats stats)
    {
        GameObject item = Instantiate(itemPrefab, playerListContainer);
        Text[] texts = item.GetComponentsInChildren<Text>();

        if (texts.Length >= 4)
        {
            texts[0].text = player.playerName;
            texts[1].text = $"Form: {stats.currentForm} | Morale: {stats.morale:F0}%";
            texts[2].text = $"Rating: {stats.averageRating:F2} | KDA: {stats.careerKDA:F2}";
            texts[3].text = $"Matches: {stats.totalMatches} | Experience: {stats.experience:F0}";
        }
    }

    private void CreateInfoPanel(string title, string content)
    {
        GameObject panel = Instantiate(itemPrefab, dataContainer);
        Text[] texts = panel.GetComponentsInChildren<Text>();

        if (texts.Length >= 2)
        {
            texts[0].text = title;
            texts[1].text = content;
        }
    }

    public void UpdateAllDisplays()
    {
        if (teamManager == null)
            return;

        // Update date and basic info
        currentDateText.text = teamManager.currentDate.ToString("dd MMM yyyy");
        teamBudgetText.text = "$" + teamManager.budget.ToString("N0");

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
        worldRankingText.text = "#" + teamManager.managedTeam.worldRanking;
    }

    private CSMatch FindNextScheduledMatch()
    {
        if (teamManager == null)
            return null;

        CSMatch nextMatch = null;
        DateTime closestDate = DateTime.MaxValue;

        foreach (CSMatch match in teamManager.upcomingMatches)
        {
            if (match.scheduledDate > teamManager.currentDate && match.scheduledDate < closestDate)
            {
                closestDate = match.scheduledDate;
                nextMatch = match;
            }
        }

        return nextMatch;
    }

    // MATCH SIMULATION UI

    public void DisplayMatchResult(MatchSimulationManager.MatchResult matchResult)
    {
        matchSimulationPanel.SetActive(true);

        matchTitleText.text = $"{matchResult.teamA.name} vs {matchResult.teamB.name}";
        matchDateText.text = matchResult.matchDate.ToString("dd MMM yyyy HH:mm");
        mapNameText.text = $"Map: {matchResult.map}";

        teamANameText.text = matchResult.teamA.name;
        teamBNameText.text = matchResult.teamB.name;
        teamAScoreText.text = matchResult.scoreTeamA.ToString();
        teamBScoreText.text = matchResult.scoreTeamB.ToString();
    }

    public void ShowStrategyCreator(Map selectedMap)
    {
        stratCreatorPanel.SetActive(true);

        if (selectedMap != null && selectedMap.mapMinimap != null)
        {
            // Load the map minimap for strategy visualization
        }
    }

    public void DisplayWorldRankings(List<RankedTeam> rankings)
    {
        worldRankingsPanel.SetActive(true);

        foreach (RankedTeam rankedTeam in rankings)
        {
            CreateRankingEntry(rankedTeam);
        }
    }

    private void CreateRankingEntry(RankedTeam rankedTeam)
    {
        // Create a UI entry showing team rank, points, movement
    }

    public void ShowTournamentBracket(SeasonManager.Tournament tournament)
    {
        tournamentBracketPanel.SetActive(true);

        // Display tournament structure
        CreateInfoPanel(tournament.tournamentName,
            $"Status: {tournament.status}\n" +
            $"Prize Pool: ${tournament.prizePool:N0}\n" +
            $"Teams: {tournament.participatingTeams.Count}\n" +
            $"Start: {tournament.startDate:dd MMM yyyy}\n" +
            $"End: {tournament.endDate:dd MMM yyyy}");
    }

    public void StartMatchSimulationView(CSMatch match)
    {
        matchSimulationPanel.SetActive(true);

        UpdateScoreDisplay(0, 0);
        UpdateMinimapDisplay(match.mapsToPlay[0]);
        ShowPlayerPositionsOnMap(match);
    }

    private void UpdateScoreDisplay(int teamAScore, int teamBScore)
    {
        scoreText.text = $"{teamAScore} - {teamBScore}";
    }

    private void UpdateMinimapDisplay(Map map)
    {
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
        GameObject killfeedEntry = new GameObject("KillfeedEntry");
        killfeedEntry.transform.SetParent(killfeedPanel.transform, false);

        Text killfeedText = killfeedEntry.AddComponent<Text>();
        killfeedText.text = $"{killer.nickName} [{weaponUsed}] {victim.nickName}";

        Destroy(killfeedEntry, 5f);
    }

    private void AdvanceDay()
    {
        if (teamManager != null)
        {
            teamManager.AdvanceDay();
            UpdateAllDisplays();
            LoadPageData(currentPage); // Refresh current page
        }
    }

    private void SaveGame()
    {
        if (seasonManager != null)
        {
            seasonManager.SaveGame();
            Debug.Log("Game saved!");
        }
    }

    private void LoadGame()
    {
        if (seasonManager != null)
        {
            seasonManager.LoadGame();
            Debug.Log("Game loaded!");
            ShowPage(UIPage.Dashboard);
        }
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}

// Specialized UI components (keeping your existing code)
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
    public Text kprText;
    public Text headshotText;
    public Text adrText;

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

        aimSlider.value = player.aim / 20f;
        reactionTimeSlider.value = player.reactionTime / 20f;
        positioningSlider.value = player.positioning / 20f;
        utilityUsageSlider.value = player.utilityUsage / 20f;
        clutchSlider.value = player.clutchAbility / 20f;

        ratingText.text = player.averageHLTVRating.ToString("F2");
        kprText.text = player.killsPerRound.ToString("F2");
        headshotText.text = player.headshotPercentage.ToString("F1") + "%";
        adrText.text = player.averageDamagePerRound.ToString("F1");
    }
}