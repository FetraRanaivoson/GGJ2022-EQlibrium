using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIManager : MonoBehaviour
{
    public GameObject loadingScreen;
    public Image nextPawnImage;

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
    /// The method to get any pawn object be displayed on the UI
    /// </summary>
    public void DisplayNextPawn(GameObject gameObject)
    {
        //Debug.Log(obj);
        nextPawnImage.sprite = gameObject.GetComponent<Pawn>().GetDisplayImage();
    }

}
