using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class LevelManager : NetworkBehaviour
{
    [SerializeField] public List<GameObject> pawnInventory;

    [SerializeField] public readonly SyncList<bool> randomizePawn = new SyncList<bool>() { true };
    
    [Server]
    public void SrvRandomizePawn(bool state)
    {
        randomizePawn[0] = true;
    }

    public GameObject lastPawn;
    public GameObject GetNextObject()
    {
        if(!randomizePawn[0])
        {
            return lastPawn;
        }
        GameObject nextObject = GetRandomPawn();
        lastPawn = nextObject;
        return nextObject;
    }

    private GameObject GetRandomPawn()
    {
        int randomIndex = UnityEngine.Random.Range(0, pawnInventory.Count);
        return pawnInventory[randomIndex];
    }
}
