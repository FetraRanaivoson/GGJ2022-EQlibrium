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

        //  THIS WILL BE USEFUL LATER FOR BUTTON INTERACTIONS : NOT WORKING !!!!
        //restartButton.onClick.AddListener(() => CmdDestroyPlatform());
    }

    bool isPickedPlayers = false;

    private void Update()
    {
        if (!isPickedPlayers)
        {
            players = FindObjectsOfType<PlayerController>();
            if (players.Length == 2)
                isPickedPlayers = true;
        }

        if (restartButton.GetComponent<RestartButton>().isSelected)
        {
            //  Unpause will instantiate another platform automatically
            //  after all pawns are touching the ground
            players[0].OnUnPauseGame();

            //  Reset all scores to zero
            for (int i = 0; i < players.Length; i++)
            {
                //Debug.Log("inside " + i + " times!");
                players[i].SetScore(0);
            }

            //  Disable popup panel for all players
            CmdSetPopUpVisible(false);

            // Reset the bool to false to prevent this from always running
            restartButton.GetComponent<RestartButton>().isSelected = false;
        }

    }

    /// <summary>
    /// The command to set the popup visible
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdSetPopUpVisible(bool state)
    {
        RpcSetPopUpVisible(state);
    }

    /// <summary>
    /// The rpc that set the popup visible for all clients
    /// </summary>
    [ClientRpc]
    private void RpcSetPopUpVisible(bool state)
    {
        popUpPanel.SetActive(state);
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
