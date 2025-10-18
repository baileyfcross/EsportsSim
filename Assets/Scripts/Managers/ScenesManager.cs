using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager instance;

    private void Awake() {
        instance = this; 
    }

    public enum Scene 
    { 
        MainMenu,
        Profile,
        Roster,
        Tactics,
        Development,
        Scouting,
        Finances,
        Schedule,
        JobCenter,
        Settings

    }

    public void LoadScene(Scene scene) 
    {
        SceneManager.LoadScene(scene.ToString());
    }

    public void LoadSceneFromString(String scene)
    {
        int foundSceneIndex = (int)Enum.Parse(typeof(Scene), scene);
        SceneManager.LoadScene(foundSceneIndex);
    }

    public void LoadNewGame() 
    {
        SceneManager.LoadScene(Scene.Profile.ToString());
    }

    public void LoadNextScene() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(Scene.MainMenu.ToString());
    }

    public void LoadSettings()
    {
        SceneManager.LoadScene(Scene.Settings.ToString());
    }

}
