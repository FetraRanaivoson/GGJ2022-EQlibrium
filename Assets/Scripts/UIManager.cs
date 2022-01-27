using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class UIManager : NetworkBehaviour
{
    public GameObject loadingScreen;
    public Image nextPawnImage;
    public TMP_Text timer;

    public TMP_Text p1Score;
    public TMP_Text p2Score;

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

    /// <summary>
    /// The function that displays the current timer
    /// </summary>
    public void DisplayTimer(float currentTime)
    {
        CmdDisplayTimer(currentTime);

    }
    [Command(requiresAuthority = false)]
    public void CmdDisplayTimer(float currentTime)
    {
        RpcDisplayTimer(currentTime);

    }
    [ClientRpc]
    public void RpcDisplayTimer(float currentTime)
    {
        timer.text = Mathf.Round(currentTime).ToString();

    }

    /// <summary>
    /// The function that fades the timer
    /// </summary>
    public void FadeTimer()
    {
        CmdFadeTimer();
    }
    [Command(requiresAuthority = false)]
    public void CmdFadeTimer()
    {
        RpcFadeTimer();

    }
    [ClientRpc]
    public void RpcFadeTimer()
    {
        timer.text = null;
    }

    /// <summary>
    /// The setter for p1/p2 scores
    /// </summary>
    public void SetScore(int score, string name)
    {
        if (name == "Player1")
            p1Score.text = score.ToString();
        else if (name == "Player2")
            p2Score.text = score.ToString();
    }

}
