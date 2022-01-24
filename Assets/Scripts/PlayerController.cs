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
    SoundManager soundManager;
    HelperManager helperManager;
    LevelManager levelManager;

    public GameObject pawnCylinderPrefab;
    public GameObject pawnCubePrefab;

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
        soundManager = FindObjectOfType<SoundManager>();
        helperManager = FindObjectOfType<HelperManager>();
        levelManager = FindObjectOfType<LevelManager>();
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

    /// <summary>The synced variable that list all 
    /// existing players in the world
    /// </summary>
    [SyncVar]
    [SerializeField]
    private List<PlayerController> Allplayers = new List<PlayerController>();

    /// <summary>The server method that gives to ALL players 
    /// the list of all available players in the world 
    /// including this player</summary>
    [Server]
    public void SetOpponents(List<PlayerController> playerControllers)
    {
        Allplayers = playerControllers;
    }

    /// <summary> Method to check if there is at least 2 players are 
    /// present in the world
    /// </summary>
    private bool allPlayersReady => Allplayers.Count > 1 && Allplayers[1] != null;

    /// <summary> 
    /// This player method to get his opponent 
    /// </summary>
    public PlayerController GetOpponent()
    {
        if (allPlayersReady)
        {
            return isServer ? Allplayers[1] : Allplayers[0]; //0:server 1:client
        }
        else
        {
            return this; //Compiler Happy 
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

    public bool IsThinking { get; internal set; }

    private bool canPlace = false;

    /// <summary>
    /// 
    /// </summary>
    [SerializeField] public readonly SyncList<bool> hasTurn = new SyncList<bool> { false };

    /// <summary>
    /// 
    /// </summary>
    [Server]
    public void CmdSetTurn(bool state)
    {
        hasTurn[0] = state;
    }

    /// <summary>
    /// The way the server check if this player has placed a pawn
    /// </summary>
    [SerializeField] public readonly SyncList<bool> placedPawn = new SyncList<bool> { false };

    /// <summary>
    /// The command to be call by the player to tell the server he has placed a pawn
    /// </summary>
    [Command(requiresAuthority =false)]
    public void CmdPlacedPawn(bool state)
    {
        placedPawn[0] = state;
    }


    [ClientCallback]
    void Update()
    {
        if (!hasAuthority)
        {
            return;
        }

        if (!NetworkClient.ready)
        {
            return;
        }

        if (!NetworkClient.active)
        {
            return;
        }

        CmdSetMousePos(this.MousePosition);


        // If this player has not turn
        if (!hasTurn[0])
        {
            CmdPlacedPawn(false);
            return;
        }


        // The logic to place a pawn
        Ray ray = Camera.main.ScreenPointToRay(mousePos);


        // While the user is holding the mouse right button, place a preview (only ont the table)
        if (Input.GetMouseButton(1))
        {
            if (!Physics.Raycast(ray, out RaycastHit hitPreview, Mathf.Infinity, LayerMask.GetMask("Table")))
            {
                canPlace = false;
                return;
            }
            helperManager.Highlight(hitPreview.point + Vector3.up * .1f, mousePos);
            canPlace = true;
        }

        // On release right mouse button
        if (Input.GetMouseButtonUp(1) && canPlace)
        {
            helperManager.CmdDeleteHelper();
            //if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }
            //Ray ray = Camera.main.ScreenPointToRay(mousePos);
            //Debug.DrawRay(ray.origin, ray.direction * 50, Color.red);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Table")))
            {
                return;
            }
            PlacePawn(hit.point + Vector3.up * 1.2f);
            CmdPlacedPawn(true);
        }
    }

    /// <summary>
    /// The function to be called when placing a pawn
    /// </summary>
    private void PlacePawn(Vector3 hitPoint)
    {
        CmdPlacePawn( hitPoint);
    }

    /// <summary>
    /// The command requested on the server in order to place a pawn
    /// </summary>
    [Command]
    private void CmdPlacePawn( Vector3 hitPoint)
    {
        if (!NetworkClient.ready)
        {
            return;
        }
        SrvPlacePawn(hitPoint);
    }

    /// <summary>
    /// The server response to place a pawn: instantiating and spawning then alert all clients 
    /// </summary>
    [Server]
    public void SrvPlacePawn(Vector3 hitPoint)
    {
        //TO DO RANDOMIZE THIS
        //GameObject pawnObj = Instantiate(pawnCylinderPrefab, hitPoint, Quaternion.identity);
        GameObject pawnObj = Instantiate(levelManager.GetNextObject(), hitPoint, Quaternion.identity);
        NetworkServer.Spawn(pawnObj);
        RpcToggleGravity(pawnObj, true);
        RpcOnPlacingPawnSound(pawnObj);
    }

    /// <summary>
    /// The gravity configuration to be made for the spawned object to all clients
    /// </summary>
    [ClientRpc]
    public void RpcToggleGravity(GameObject pawnObj, bool state)
    {
        pawnObj.GetComponent<Pawn>().UseGravity(state);
    }

    /// <summary>
    /// The alert sound to be sent to all clients when a pawn is placed
    /// </summary>
    [ClientRpc]
    private void RpcOnPlacingPawnSound(GameObject pawnObj)
    {
        soundManager.OnPlacingPawn(pawnObj);
    }

}
