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

    /// <summary>
    /// The system of the platform composed by the base [0], the mid[1] and the top[2]
    /// </summary>
    [SerializeField] public readonly SyncList<GameObject> system = new SyncList<GameObject>();

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
    [Command(requiresAuthority =false)]
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

    private void Awake()
    {
        table = FindObjectOfType<Table>();
        timerManager = FindObjectOfType<TimerManager>();
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
    public int nextPawnIndex = 0;

    /// <summary>
    /// The next pawn to be spawned by the server to be used by the concerned player
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


}
