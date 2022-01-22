using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedUI : MonoBehaviour
{
    public GameObject loadingScreen;


    void Start()
    {
        
    }


    /// <summary>
    /// The event that happen on the shared UI when the game is unpaused
    /// </summary>
    public void OnUnPauseGame()
    {
        loadingScreen.SetActive(true);
    }

    /// <summary>
    /// The event that happen on the shared UI on loading screen
    /// </summary>
    public void OnLoadingScreen()
    {
        loadingScreen.SetActive(true);
    }

    /// <summary>
    /// The event that happen on the shared UI on finish loading screen
    /// </summary>
    public void OnNotLoadingScreen()
    {
        loadingScreen.SetActive(false);
    }
}
