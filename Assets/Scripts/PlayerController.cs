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

    public GameObject pawnPrefab;

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

    /// <summary>
    /// The current mouse position of this player
    /// </summary>
    [SyncVar] public Vector3 mousePos = Vector3.zero;

    /// <summary>
    /// The handler for synchronizing this player's current mouse position
    /// </summary>
    [Server]
    public void SetMousePos(Vector3 mousePos)
    {
        this.mousePos = mousePos;
    }

    /// <summary>
    /// The method to be called by this player to synchronize his mouse position
    /// value across the network
    /// </summary>
    /// <param name="mousePos"></param>
    [Command(requiresAuthority = false)]
    public void CmdSetMousePos(Vector3 mousePos)
    {
        SetMousePos(mousePos);
    }

    /// <summary>
    /// The property that returns the mouse position of this player but NOT SYNCED
    /// </summary>
    /// <returns></returns>
    private Vector3 MousePosition => Mouse.current.position.ReadValue(); 


    [ClientCallback]
    void Update()
    {
        if (!hasAuthority)
        {
            return;
        }

        CmdSetMousePos(this.MousePosition);


        if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        Debug.DrawRay(ray.origin, ray.direction * 50, Color.red);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Table")))
        {
            return;
        }

        PlacePawn(hit.point + Vector3.up * .35f);
    }

    private void PlacePawn(Vector3 hitPoint)
    {
        CmdPlacePawn(hitPoint);
    }

    [Command]
    private void CmdPlacePawn(Vector3 hitPoint)
    {
        SrvPlacePawn(hitPoint);
    }

    [Server]
    public void SrvPlacePawn(Vector3 hitPoint)
    {
        //TO DO RANDOMIZE THIS
        GameObject pawnObj = Instantiate(pawnPrefab, hitPoint, Quaternion.identity);
        NetworkServer.Spawn(pawnObj);
        RpcToggleGravity(pawnObj, true);
    }

    [ClientRpc]
    public void RpcToggleGravity(GameObject pawnObj, bool state)
    {
        pawnObj.GetComponent<Pawn>().UseGravity(true);
    }
}
