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
    public UIManager uiManager;
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
        uiManager = FindObjectOfType<UIManager>();
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
        uiManager.OnLoadingScreen();
    }

    /// <summary>
    /// The event that happen when not waiting for opponent to join anymore
    /// </summary>
    public void OnNotWaitingForOpponent()
    {
        uiManager.OnNotLoadingScreen();
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
    /// The synced variable that helps to know if this player has turn
    /// </summary>
    [SerializeField] public readonly SyncList<bool> hasTurn = new SyncList<bool> { false };

    /// <summary>
    /// The way the server set the turn for a player
    /// </summary>
    [Server]
    public void SrvSetTurn(bool state)
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
    [Command(requiresAuthority = false)]
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
            //PlacePawn(hit.point + Vector3.up * 1.2f);
            PlacePawn(hit.point + Vector3.up * 1.2f);
            CmdPlacedPawn(true);
            CmdIncrementtNextPawnIndex();
        }
    }
    /// <summary>
    /// The command to be called by this player after placing a pawn to increment the next pawn index.
    /// That index is crucial because we need to know what to "really" spawn on the table next among our 
    /// lists of pawn "pawnInstances". And we always need the last one, that's we always need to increment the index.
    /// </summary>
    [Command]
    public void CmdIncrementtNextPawnIndex()
    {
        RpcIncrementtNextPawnIndex();
    }
    /// <summary>
    /// The command sent to all clients to increment the next pawn index
    /// </summary>
    [ClientRpc]
    private void RpcIncrementtNextPawnIndex()
    {
        levelManager.nextPawnIndex++;
    }

    /// <summary>
    /// The function to be called when placing a pawn
    /// </summary>
    private void PlacePawn(Vector3 hitPoint)
    {
        CmdPlacePawn(hitPoint);
    }

    /// <summary>
    /// The command requested on the server in order to place a pawn
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdPlacePawn(Vector3 hitPoint)
    {
        SrvPlacePawn(hitPoint);
    }

    /// <summary>
    /// The server response to place a pawn: instantiating and spawning then alert all clients 
    /// </summary>
    [Server]
    public void SrvPlacePawn(Vector3 hitPoint)
    {
        //GameObject pawnObj = Instantiate(pawnCylinderPrefab, hitPoint, Quaternion.identity);
        //GameObject pawnObj = Instantiate(levelManager.nextPawn[0], hitPoint, Quaternion.identity);
        //GameObject pawnObj = Instantiate(gos[0], hitPoint, Quaternion.identity);
        pawnInstances[levelManager.nextPawnIndex].transform.position = hitPoint;
        //NetworkServer.Spawn(pawnObj);
        //RpcToggleGravity(pawnObj, true);
        RpcIsKinematic(pawnInstances[levelManager.nextPawnIndex], false);
        RpcToggleGravity(pawnInstances[levelManager.nextPawnIndex], true);
        RpcOnPlacingPawnSound(pawnInstances[levelManager.nextPawnIndex]);
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
    /// The dynamic configuration to be made for the spawned object to all clients
    /// </summary>
    [ClientRpc]
    public void RpcIsKinematic(GameObject pawnObj, bool state)
    {
        pawnObj.GetComponent<Pawn>().IsKinematic(state);
    }

    /// <summary>
    /// The alert sound to be sent to all clients when a pawn is placed
    /// </summary>
    [ClientRpc]
    private void RpcOnPlacingPawnSound(GameObject pawnObj)
    {
        soundManager.OnPlacingPawn(pawnObj);
    }

    /// <summary>
    /// The function to be called by the network manager to display next pawn 
    /// </summary>
    public void DisplayNextPawn(GameObject nextPawn)
    {
        //uiManager.DisplayNextPawn(gameObject);
       SrvDisplayNextPawn(nextPawn);
        
    }

    /// <summary>
    /// The handler run by the server to instantiate the next pawn.
    /// WITHOUT INSTANTIATING THE NEXT PAWN, THERE'S NO WAY TO GET HIS COMPONENT PAWN ON CLIENTS
    /// WE WILL NEED THE COMPONENT PAWN TO GET THE DISPLAY IMAGE OF THE PAWN
    /// WHOSE VALUE IS THEN APPLIED TO THE NEXT PAWN DISPLAY IMAGE OF THE UI MANAGER
    /// </summary>
    [Server]
    private void SrvDisplayNextPawn(GameObject nextPawn)
    {
        GameObject pawnInstance = Instantiate(nextPawn, Vector3.up * 1500, Quaternion.identity);
        NetworkServer.Spawn(pawnInstance);
        pawnInstances.Add(pawnInstance);

        CmdDisplayNextPawn(pawnInstance);
    }

    /// <summary>
    /// The list of every spawned pawns from the very beginning.
    /// </summary>
    [SerializeField] public readonly SyncList<GameObject> pawnInstances = new SyncList<GameObject>() {};
    

    /// <summary>
    /// The command run by this player for all cients when the pawn is spawned: kinematic set, gravity set, ui display set.
    /// </summary>
    [Command(requiresAuthority =false)]
    private void CmdDisplayNextPawn(GameObject inst)
    {
        RpcIsKinematic(inst, true);
        RpcToggleGravity(inst, false);
        RpcDisplayNextPawn(inst);
    }


    /// <summary>
    /// The function that tells the manager to display the next pawn on the ui for all clients
    /// Without the paramater GameObject to be an INSTANCE, this would be impossible.
    /// Prefab doesn't work
    /// </summary>
    [ClientRpc]
    private void RpcDisplayNextPawn(GameObject inst)
    {
        uiManager.DisplayNextPawn(inst);
    }


}
