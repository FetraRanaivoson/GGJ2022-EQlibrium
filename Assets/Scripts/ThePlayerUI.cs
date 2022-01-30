using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class ThePlayerUI : NetworkBehaviour
{
    public GameObject popUpPanel;
    public TMP_Text popUpMessage;
    public Button mainMenuButton;
    public Button restartButton;

    LevelManager levelManager;
    PlayerController[] players = new PlayerController[2];

    private void Awake()
    {
        levelManager = FindObjectOfType<LevelManager>();
    }

    void Start()
    {
        mainMenuButton.onClick.AddListener(NetworkManager.singleton.StopHost);

        //  THIS WILL BE USEFUL LATER FOR BUTTON INTERACTIONS
            restartButton.onClick.AddListener(()=>CmdDestroyPlatform());
    }


    /// <summary>
    /// 
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdDestroyPlatform()
    {
        //  Unpause?

        //  Instantiate new platform
        SrvInstantiatePlatform();

        //  Reset all scores to zero
        players = FindObjectsOfType<PlayerController>();
        for (int i = 0; i < players.Length; i++)
        {
            players[i].SetScore(0);
        }

        //  Disable popup panel for all players
        RpcSetPopUpVisible(false);

    }

    /// <summary>
    /// 
    /// </summary>
    [ClientRpc]
    private void RpcSetPopUpVisible(bool state)
    {
        popUpPanel.SetActive(state);
    }

    /// <summary>
    /// 
    /// </summary>
    [Server]
    private void SrvInstantiatePlatform()
    {
        levelManager.InstantiatePlatform();
        foreach (var item in players)
        {

        }

    }


    public void OnClick()
    {
        //Debug.Log("clicked button");
    }


    //The pop up message to show for the game result
    public void ShowPopUp(string message, bool canActivatePopUpButtons)
    {
        //Debug.Log("popup show");
        popUpPanel.SetActive(true);
        popUpMessage.text = message;

        ActivatePopUpButtons(canActivatePopUpButtons);
    }

    public void ActivatePopUpButtons(bool canActivatePopUpButtons)
    {
        if (canActivatePopUpButtons)
        {
            mainMenuButton.interactable = true;
            restartButton.interactable = true;
        }
        if (!canActivatePopUpButtons)
        {
            mainMenuButton.interactable = false;
            restartButton.interactable = false;
        }
    }
}
