using Cinemachine;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    CinemachineFreeLook cameraFreeLook;
    UIManager uiManager;
    SoundManager soundManager;
    HelperManager helperManager;
    LevelManager levelManager;
    ThePlayerUI playerUI;

    Vector2 moveDir;
    public float torqueAmount;

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

        if (isLocalPlayer)
        {
            playerUI = FindObjectOfType<ThePlayerUI>();
        }

    }

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

    /// <summary>
    /// The score of this player
    /// </summary>
    [SyncVar(hook = nameof(OnUpdateScore))] public int score = 0;

    /// <summary>
    /// The command run by server to set this player's score
    /// </summary>
    [Server]
    public void SetScore()
    {
        score++;
    }
    [Server]
    public void SetScore(int score)
    {
        this.score = score;
    }

    // The hook attribute can be used to specify a function
    // to be called when the SyncVar changes value on the client.
    private void OnUpdateScore(int oldScore, int newScore)
    {
        uiManager.SetScore(newScore, displayName);
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
        }
    }

    /// <summary>The synced variable that list all 
    /// existing players in the world
    /// </summary>
    [SyncVar]
    [SerializeField]
    private List<PlayerController> Allplayers = new List<PlayerController>();

    /// <summary>
    /// The command run by the server to show pop up on this player
    /// </summary>
    [Server]
    public void SrvShowPopUp(string message)
    {
        TargetShowPopUp(message);
    }

    /// <summary>
    /// The response to show popup message 
    /// as directed by the server ONLY FOR THIS MACHINE
    /// </summary>
    [TargetRpc]
    private void TargetShowPopUp(string message)
    {
        bool canActivatePopUpButtons = false;
        if (isServer)
            canActivatePopUpButtons = true;

        playerUI.ShowPopUp(message, canActivatePopUpButtons);
    }

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
            // Do not use live preview if we don't hit the top table
            if (!Physics.Raycast(ray, out RaycastHit hitPreview, Mathf.Infinity, LayerMask.GetMask("Table")))
            {
                canPlace = false;
                return;
            }

            if (hitPreview.collider.gameObject == null)
                return;

            canPlace = true;
            helperManager.Highlight(hitPreview.point + Vector3.up * .1f, mousePos);

            //Get the move direction from keyboard and torque the next pawn
            //CmdTorque(moveDir, pawnInstances[levelManager.nextPawnIndex], torqueAmount, Time.deltaTime);
            CmdTorque(moveDir, levelManager.pawnInstances[levelManager.pawnInstances.Count - 1], torqueAmount, Time.deltaTime);

            //if (pawnInstances[levelManager.nextPawnIndex].GetComponent<Pawn>().previewTrigger.isTriggeringPawn)
            if (levelManager.pawnInstances.Count == 0)
                return;

            if (levelManager.pawnInstances[levelManager.pawnInstances.Count - 1].GetComponent<Pawn>().previewTrigger.isTriggeringPawn)
            {
                helperManager.CmdDeleteHelper();
                CmdOnPlayerFailedToPlacePawn(levelManager.pawnInstances[levelManager.pawnInstances.Count - 1]);
                canPlace = false;
            }

            // Live preview the pawn
            if (canPlace)
                CmdLivePreviewPawn(hitPreview.point);
            else
                CmdLivePreviewPawn(hitPreview.point + Vector3.up * 1.5f);
        }

        // On release right mouse button
        if (Input.GetMouseButtonUp(1) && canPlace && !levelManager.isLevelReset)
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
            PlacePawn(hit.point + Vector3.up * .95f);
            CmdPlacedPawn(true);
        }
    }


    /// <summary>
    /// The function to be called when placing a pawn
    /// </summary>
    private void PlacePawn(Vector3 hitPoint)
    {
        CmdPlacePawn(hitPoint);
    }

    /// <summary>
    /// The server response to place a pawn: instantiating and spawning then alert all clients 
    /// </summary>
    [Server]
    public void SrvPlacePawn(Vector3 hitPoint)
    {
        //pawnInstances[levelManager.nextPawnIndex].transform.position = hitPoint;
        levelManager.pawnInstances[levelManager.pawnInstances.Count - 1].transform.position = hitPoint;

        //NetworkServer.Spawn(pawnObj);
        //RpcToggleGravity(pawnObj, true);

        //RpcEnableCollider(pawnInstances[levelManager.nextPawnIndex], true);
        RpcEnableCollider(levelManager.pawnInstances[levelManager.pawnInstances.Count - 1], true);

        //RpcIsKinematic(pawnInstances[levelManager.nextPawnIndex], false);
        RpcIsKinematic(levelManager.pawnInstances[levelManager.pawnInstances.Count - 1], false);

        //RpcToggleGravity(pawnInstances[levelManager.nextPawnIndex], true);
        RpcToggleGravity(levelManager.pawnInstances[levelManager.pawnInstances.Count - 1], true);

        //RpcOnPlacingPawnSound(pawnInstances[levelManager.nextPawnIndex]);
        RpcOnPlacingPawnSound(levelManager.pawnInstances[levelManager.pawnInstances.Count - 1]);

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
    /// The gravity configuration to be made for the spawned object to all clients
    /// </summary>
    //[ClientRpc]
    public void RpcToggleGravity(GameObject pawnObj, bool state)
    {
        pawnObj.GetComponent<Pawn>().UseGravity(state);
    }
    /// <summary>
    /// The dynamic configuration to be made for the spawned object to all clients
    /// </summary>
    //[ClientRpc]
    public void RpcIsKinematic(GameObject pawnObj, bool state)
    {
        pawnObj.GetComponent<Pawn>().IsKinematic(state);
    }
    /// <summary>
    /// The collider configuration to be made for the spawned object to all clients
    /// </summary>
    //[ClientRpc]
    public void RpcEnableCollider(GameObject pawnObj, bool state)
    {
        pawnObj.GetComponent<Pawn>().EnableCollider(state);
    }

    /// <summary>
    /// The alert sound to be sent to all clients when a pawn is placed
    /// </summary>
    //[ClientRpc]
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

        //SrvDisplayNextPawn(nextPawn);

    }

    /// <summary>
    /// The handler run by the server to instantiate the next pawn.
    /// WITHOUT INSTANTIATING THE NEXT PAWN, THERE'S NO WAY TO GET HIS COMPONENT PAWN ON CLIENTS
    /// WE WILL NEED THE COMPONENT PAWN TO GET THE DISPLAY IMAGE OF THE PAWN
    /// WHOSE VALUE IS THEN APPLIED TO THE NEXT PAWN DISPLAY IMAGE OF THE UI MANAGER
    /// </summary>
    //[Server]
    //private void SrvDisplayNextPawn(GameObject nextPawn)
    //{
    //    GameObject pawnInstance = Instantiate(nextPawn, Vector3.up * 1500, Quaternion.identity);
    //    NetworkServer.Spawn(pawnInstance);
    //    pawnInstances.Add(pawnInstance);
    //    //AddPawn(pawnInstance);

    //    CmdDisplayNextPawn(pawnInstance);
    //}

    //[ClientRpc]
    //public void AddPawn(GameObject pawn)
    //{
    //    pawnInstances.Add(pawn);
    //}

    /// <summary>
    /// The list of every spawned pawns from the very beginning.
    /// </summary>
    //[SerializeField] public readonly SyncList<GameObject> pawnInstances = new SyncList<GameObject>() { };


    /// <summary>
    /// The command run by this player for all cients when the pawn is spawned: collider set, kinematic set, gravity set, ui display set.
    /// </summary>
    //[Command(requiresAuthority = false)]
    //private void CmdDisplayNextPawn(GameObject inst)
    //{
    //    RpcEnableCollider(inst, false);
    //    RpcIsKinematic(inst, true);
    //    RpcToggleGravity(inst, false);
    //    RpcDisplayNextPawn(inst);
    //}

    /// <summary>
    /// The command to reset the pawn state and location to where it was if this player failed to place the pawn 
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdOnPlayerFailedToPlacePawn(GameObject inst)
    {
        RpcEnableCollider(inst, false);
        RpcIsKinematic(inst, true);
        RpcToggleGravity(inst, false);
    }

    //[ClientRpc]
    //private void RpcResetPawnTransform(GameObject inst, float height)
    //{
    //    inst.transform.position = Vector3.up * height;
    //}

    /// <summary>
    /// The function that tells the manager to display the next pawn on the ui for all clients
    /// Without the paramater GameObject to be an INSTANCE, this would be impossible.
    /// Prefab doesn't work
    /// </summary>
    //[ClientRpc]
    //private void RpcDisplayNextPawn(GameObject inst)
    //{
    //    uiManager.DisplayNextPawn(inst);
    //}

    /// <summary>
    /// Receive inputs from keyboard WASD
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveDir = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// The command that let this player to torque a pawn on all instances
    /// </summary>
    [Command]
    public void CmdTorque(Vector2 moveDir, GameObject pawn, float torqueAmount, float deltaTime)
    {
        RpcTorque(moveDir, pawn, torqueAmount, deltaTime);
    }

    /// <summary>
    /// The handler that tells all clients to torque this pawn
    /// </summary>
    [ClientRpc]
    public void RpcTorque(Vector2 moveDir, GameObject pawn, float torqueAmount, float deltaTime)
    {
        pawn.GetComponent<Pawn>().Torque(moveDir, torqueAmount, deltaTime);
    }

    /// <summary>
    /// The command run by the server to preview the pawn on all instances
    /// </summary>
    [Command]
    public void CmdLivePreviewPawn(Vector3 hitPreview)
    {
        RpcLivePreviewPawn(hitPreview);
    }

    /// <summary>
    /// The handler that tells all clients to preview the pawn only all instances
    /// </summary>
    [ClientRpc]
    public void RpcLivePreviewPawn(Vector3 hitPreview)
    {
        levelManager.pawnInstances[levelManager.pawnInstances.Count - 1].transform.position = hitPreview + Vector3.up * .95f;
    }

}
