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
    /// <param name="hitPoint"></param>
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
