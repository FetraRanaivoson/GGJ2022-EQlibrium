using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Mirror;
using System;

public class PlayerController : NetworkBehaviour
{
    CinemachineFreeLook cameraFreeLook;
    PlayerUI playerUI;
    SharedUI sharedUI;

    /// <summary>
    /// The name that identifies this player on the network
    /// </summary>
    [SyncVar] public string displayName = "No name";

    /// <summary>
    /// The command run by server to set this player's name
    /// </summary>
    [Server]
    public void SetDisplayName(string name)
    {
        displayName = name;
    }

    private void Awake()
    {
        cameraFreeLook = FindObjectOfType<CinemachineFreeLook>();
        sharedUI = FindObjectOfType<SharedUI>();
    }


    void Start()
    {
        PickCamera();
    }


    /// <summary>
    /// The method to pick the scene camera for each player
    /// </summary>
    private void PickCamera()
    {
        if (isLocalPlayer)
        {
            cameraFreeLook.Follow = transform;
            cameraFreeLook.LookAt = transform;
            playerUI = FindObjectOfType<PlayerUI>();
        }
    }

    /// <summary>
    /// The event that happen when the game is unpaused
    /// </summary>
    [ClientRpc]
    public void OnUnPauseGame()
    {
        Time.timeScale = 1;
    }

    /// <summary>
    /// The event that happen when the game is paused
    /// </summary>
    [ClientRpc]
    public void OnPauseGame()
    {
        Time.timeScale = 0;
    }

    /// <summary>
    /// The event that happen when waiting for opponent to join
    /// </summary>
    public void OnWaitingForOpponent()
    {
        sharedUI.OnLoadingScreen();
    }

    /// <summary>
    /// The event that happen when not waiting for opponent to join anymore
    /// </summary>
    public void OnNotWaitingForOpponent()
    {
        sharedUI.OnNotLoadingScreen();
    }


    [ClientCallback]
    void Update()
    {

    }


}
