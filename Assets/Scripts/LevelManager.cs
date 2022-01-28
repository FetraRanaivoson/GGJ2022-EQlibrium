using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class LevelManager : NetworkBehaviour
{
    public GameObject basePrefab;
    public GameObject midPrefab;
    public GameObject topPrefab;

    UIManager UIManager;
    private void Awake()
    {
        table = FindObjectOfType<Table>();
        timerManager = FindObjectOfType<TimerManager>();
        UIManager = FindObjectOfType<UIManager>();
    }

    /// <summary>
    /// The system of the platform composed by the base [0], the mid[1] and the top[2]
    /// </summary>
    [SerializeField] public readonly SyncList<GameObject> system = new SyncList<GameObject>();

    /// <summary>
    /// 
    /// </summary>
    [SerializeField] public readonly SyncList<GameObject> pawnInstances = new SyncList<GameObject>();

    /// <summary>
    /// 
    /// </summary>
    [SerializeField] public GameObject nextPawnObject => pawnInstances[pawnInstances.Count - 1];


    /// <summary>
    /// The way the server instantiates the platform of the game
    /// </summary>
    public void InstantiatePlatform()
    {
        CmdInstantiatePlatform();
    }

    /// <summary>
    /// The way the server destroys the platform of the game
    /// </summary>
    public void DestroyPlatform()
    {
        CmdDestroyPlatform();
    }

    /// <summary>
    /// Destroy the platform
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdDestroyPlatform()
    {
        for (int i = 0; i < system.Count; i++)
        {
            SrvDestroySystem(system[i]);
        }
        ClearSystem();
    }

    /// <summary>
    /// The function run by the server to destroy the system composed by the base, mid and top
    /// </summary>
    [Server]
    private void SrvDestroySystem(GameObject gameObject)
    {
        //system.Remove(gameObject);
        NetworkServer.Destroy(gameObject);
    }

    /// <summary>
    /// 
    /// </summary>
    [Command(requiresAuthority = false)]
    private void ClearSystem()
    {
        system.Clear();
    }





    /// <summary>
    /// Instantiate the platform
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdInstantiatePlatform()
    {
        InstantiateBase();
        InstantiateMid();
        InstantiateTop();
    }

   

    /// <summary>
    ///   Instantiate the mid
    /// </summary>
    private void InstantiateMid()
    {
        GameObject mid = Instantiate(midPrefab);
        NetworkServer.Spawn(mid);
        system.Add(mid);
    }


    /// <summary>
    ///  Instantiate the top
    /// </summary>
    private void InstantiateTop()
    {
        GameObject top = Instantiate(topPrefab);
        NetworkServer.Spawn(top);
        system.Add(top);

    }

    /// <summary>
    /// Instantiate the base
    /// </summary>
    private void InstantiateBase()
    {
        GameObject bottom = Instantiate(basePrefab);
        NetworkServer.Spawn(bottom);
        system.Add(bottom);
    }


    /// <summary>
    /// The table where the players place pawn
    /// </summary>
    Table table;
    TimerManager timerManager;

    /// <summary>
    /// The function to be called by the network manager to display next pawn
    /// </summary>
    
    public void DisplayNextPawn()
    {
        CmdDisplayNextPawn();
    }

    /// <summary>
    /// 
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdDisplayNextPawn()
    {
        SrvDisplayNextPawn();
        
    }

    /// <summary>
    /// The handler run by the server to instantiate the next pawn.
    /// WITHOUT INSTANTIATING THE NEXT PAWN, THERE'S NO WAY TO GET HIS COMPONENT PAWN ON CLIENTS
    /// WE WILL NEED THE COMPONENT PAWN TO GET THE DISPLAY IMAGE OF THE PAWN
    /// WHOSE VALUE IS THEN APPLIED TO THE NEXT PAWN DISPLAY IMAGE OF THE UI MANAGER
    /// </summary>
    [Server]
    private void SrvDisplayNextPawn()
    {
        GameObject pawnInstance = Instantiate(nextPawn[0], Vector3.up * 1500, Quaternion.identity);
        NetworkServer.Spawn(pawnInstance);
        pawnInstances.Add(pawnInstance);

        SrvOnDisplayNextPawn(nextPawnObject);
    }

    /// <summary>
    /// The command run for all cients when the pawn is spawned: collider set, kinematic set, gravity set, ui display set.
    /// </summary>
    //[Server]
    private void SrvOnDisplayNextPawn(GameObject inst)
    {
        RpcEnableCollider(inst, false);
        RpcIsKinematic(inst, true);
        RpcToggleGravity(inst, false);
        RpcDisplayNextPawn(inst);
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
    /// The dynamic configuration to be made for the spawned object to all clients
    /// </summary>
    //[ClientRpc]
    public void RpcIsKinematic(GameObject pawnObj, bool state)
    {
        pawnObj.GetComponent<Pawn>().IsKinematic(state);
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
    /// The function that tells the manager to display the next pawn on the ui for all clients
    /// Without the paramater GameObject to be an INSTANCE, this would be impossible.
    /// Prefab doesn't work
    /// </summary>
    //[ClientRpc]
    private void RpcDisplayNextPawn(GameObject inst)
    {
        UIManager.DisplayNextPawn(inst);
    }


    /// <summary>
    /// The variable that lets us know that the level was reset
    /// </summary>
    [SyncVar]
    [SerializeField] public bool isLevelReset = false;

    /// <summary>
    /// The way for the server to set isLevelReset
    /// </summary>
    [Server]
    public void SrvResetLevel(bool state)
    {
        isLevelReset = state;
    }

    /// <summary>
    ///  The way for this manager to set isLevelReset
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdResetLevel(bool state)
    {
        SrvResetLevel(state);
    }

 

    /// <summary>
    /// The list of pawn to be used by the level manager
    /// </summary>
    [SerializeField] public List<GameObject> pawnInventory;

    /// <summary>
    /// The last pawn spawned by the server used by the concerned player
    /// </summary>
    public GameObject lastPawn;

    /// <summary>
    /// The next pawn's index useful for the player when he wants to spawn the next pawn
    /// displayed on the ui.
    /// Having this variable helps a lot because it's easier to get it and it is unique in the game so
    /// no headache when syncing this variable
    /// </summary>
    //public int nextPawnIndex = 0;

    /// <summary>
    /// The next pawn to be spawned by the server to be used by the concerned player
    /// . Nb: this is a prefab
    /// </summary>
    [SerializeField] public readonly SyncList<GameObject> nextPawn = new SyncList<GameObject>() { null };

    /// <summary>
    /// The function to set the next randomized pawn
    /// </summary>
    [Command(requiresAuthority = false)]
    public void SetNextPawn()
    {
        GameObject nextObject = GetRandomPawn();
        nextPawn[0] = nextObject;
        //Debug.Log("next pawn is: " + nextPawn[0].name);
        lastPawn = nextObject;
    }

    /// <summary>
    /// The function that select randomized pawn from inventory
    /// </summary>
    private GameObject GetRandomPawn()
    {
        int randomIndex = UnityEngine.Random.Range(0, pawnInventory.Count);
        return pawnInventory[randomIndex];
    }




    /// <summary>
    /// Avoid null index
    /// </summary>
    [Command(requiresAuthority =false)]
    public void CmdRemovePawn(GameObject pawn)
    {
        pawnInstances.Remove(pawn);
    }


    /// <summary>
    /// The command to clean the pawn list run on all clients
    /// </summary>
    //[Server]

    [ClientRpc]
    private void RpcRemoveFromList(GameObject pawn)
    {
        pawnInstances.Remove(pawn);
    }


}
