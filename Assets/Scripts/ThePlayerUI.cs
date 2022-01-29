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

    void Start()
    {
        mainMenuButton.onClick.AddListener(NetworkManager.singleton.StopHost);
    }


    //The pop up message to show for the game result
    public void ShowPopUp(string message, bool activatePopUpButtons)
    {
        Debug.Log("popup show");
        popUpPanel.SetActive(true);
        popUpMessage.text = message;

        ActivatePopUpButtons(activatePopUpButtons);
    }

    public void ActivatePopUpButtons(bool activate)
    {
        if (activate)
        {
            mainMenuButton.interactable = true;
        }
        if (!activate)
        {
            mainMenuButton.interactable = false;
        }
    }
}
