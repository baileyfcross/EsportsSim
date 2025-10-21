using UnityEngine;

/// <summary>
/// Main game manager - runs in single persistent scene
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core Systems")]
    public MatchSimulationManager matchSimulation;
    public ContractSystem contractSystem;
    public TransferMarket transferMarket;
    public SeasonManager seasonManager;
    public PlayerDevelopment playerDevelopment;
    public UIManager uiController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAllSystems();
    }

    private void InitializeAllSystems()
    {
        matchSimulation = GetComponent<MatchSimulationManager>();
        contractSystem = GetComponent<ContractSystem>();
        transferMarket = GetComponent<TransferMarket>();
        seasonManager = GetComponent<SeasonManager>();
        playerDevelopment = GetComponent<PlayerDevelopment>();
        uiController = GetComponent<UIManager>();

        Debug.Log("All game systems initialized");
    }

    public MatchSimulationManager GetMatchSimulation() => matchSimulation;
    public ContractSystem GetContractSystem() => contractSystem;
    public TransferMarket GetTransferMarket() => transferMarket;
    public SeasonManager GetSeasonManager() => seasonManager;
    public PlayerDevelopment GetPlayerDevelopment() => playerDevelopment;
    public UIManager GetUIController() => uiController;
}