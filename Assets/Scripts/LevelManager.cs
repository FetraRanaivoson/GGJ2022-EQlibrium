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
    /// The next pawn to be spawned by the server to be used by the concerned player
    /// </summary>
    public GameObject nextPawn;

    /// <summary>
    /// The function to set the next randomized pawn
    /// </summary>
    public void SetNextPawn()
    {
        GameObject nextObject = GetRandomPawn();
        nextPawn = nextObject;
        lastPawn = nextObject;
    }

    /// <summary>
    /// The function that select randomized pawn from inventory
    /// </summary>
    /// <returns></returns>
    private GameObject GetRandomPawn()
    {
        int randomIndex = UnityEngine.Random.Range(0, pawnInventory.Count);
        return pawnInventory[randomIndex];
    }

 
}
