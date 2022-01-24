using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIManager : NetworkBehaviour
{
    public GameObject loadingScreen;
    public Image nextPawnImage;

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

    /// <summary>
    /// 
    /// </summary>
    public void DisplayNextPawn(GameObject nextPawn)
    {
        nextPawnImage.sprite = nextPawn.GetComponent<Pawn>().GetDisplayImage();
        spriteSynced = nextPawn.GetComponent<Pawn>().GetDisplayImage();
    }

    private Sprite spriteSynced;
    private void Update()
    {
        if (!hasAuthority)
        {
            nextPawnImage.sprite = spriteSynced;
            return;
        }
    }
}
