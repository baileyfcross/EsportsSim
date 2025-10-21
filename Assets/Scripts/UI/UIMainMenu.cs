using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    //Instance Variables
    [SerializeField] Button _startGame;
    [SerializeField] Button _settingsButton;
    [SerializeField] Button _quitGame;
    bool isDebugOn = false;

    void Start()
    {
        _startGame.onClick.AddListener(StartGame);
        _settingsButton.onClick.AddListener(LoadSettings);
        _quitGame.onClick.AddListener(QuitGame);
    }

    private void StartGame()
    {
        if (isDebugOn == true)
        {
            Debug.Log("Entering StartGame from listener");
        }
        ScenesManager.instance.LoadNewGame();
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    private void LoadSettings()
    {
        ScenesManager.instance.LoadSettings();
    }
}
