using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class UISidebarMenu : MonoBehaviour {

    //Instance Variables
    [SerializeField] Button _selectedButton;
    bool isDebugOn = false;
    string[] bufferButtonName = null;
    string buttonName = null;

    void Start() {
        if (_selectedButton != null) {
            buttonName = _selectedButton.ToString();
            bufferButtonName = buttonName.Split(' ');
            buttonName = bufferButtonName[0];

            if (isDebugOn == true) 
            {
                Debug.Log("Found Button Name");
            }

            _selectedButton.onClick.AddListener(LoadScene);

        }

        if (isDebugOn == true) {
            Debug.Log(buttonName);
            Debug.Log(_selectedButton);
        }
    }

    public void LoadMainMenu() {
        ScenesManager.instance.LoadMainMenu();
    }

    public void LoadScene()
    {
        if (isDebugOn == true) {
            Debug.Log("Loading Scene");
            Debug.Log(buttonName);
            Debug.Log("Loading Scene from found name in if statement");
        }
        ScenesManager.instance.LoadSceneFromString(buttonName);
    }
}
