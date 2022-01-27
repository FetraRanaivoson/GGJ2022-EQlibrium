using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class LevelManager : NetworkBehaviour
{
    /// <summary>
    /// The list of pawn to be used by the level manager
    /// </summary>
    [SerializeField] public List<GameObject> pawnInventory;

    /// <summary>
    /// The last pawn spawned spawned by the server used by the concerned player
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

    internal void ResetLevel()
    {
        throw new NotImplementedException();
    }
}
