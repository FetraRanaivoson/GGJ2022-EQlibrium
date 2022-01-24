using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class GameNetworkManager : NetworkManager
{
    [SerializeField] List<PlayerController> players = new List<PlayerController>();
    LevelManager levelManager;
    UIManager UIManager;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        // Grab the player
        PlayerController player = conn.identity.GetComponent<PlayerController>();

        // Set the name
        player.SetDisplayName($"Player {numPlayers}");

        //Add to list
        players.Add(player);

        //In case 1 player exists
        if (NetworkServer.active && players.Count == 1)
        {
            players[0].OnWaitingForOpponent();
            players[0].OnPauseGame();
        }
        // In case 2 players exist
        if (NetworkServer.active && players.Count > 1)
        {
            players[0].OnUnPauseGame();
            players[0].OnNotWaitingForOpponent();
        }
        levelManager = FindObjectOfType<LevelManager>();
        UIManager = FindObjectOfType<UIManager>();
    }

    public override void OnServerChangeScene(string newSceneName)
    {
    }

    /// <summary>
    /// 0 means player 1, 1 means player 2
    /// </summary>
    int nextTurn = 0;

    /// <summary>
    /// Randomize once until the current player ends its turn
    /// </summary>
    bool shouldRandomizeNextPawn = true;

    private void Update()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] != null && NetworkServer.active)
                players[i].SetOpponents(players);
        }

        if (shouldRandomizeNextPawn)
        {
            if (levelManager != null)
            {
                levelManager.SetNextPawn();
                shouldRandomizeNextPawn = false;
            }
        }


        // Display next pawn
        if (UIManager != null && levelManager != null)
        {
            UIManager.DisplayNextPawn(levelManager.nextPawn);
        }


        //  Setting turn
        if (players.Count > 0)
        {
            if (players[nextTurn] != null)
            {
                players[nextTurn].SrvSetTurn(true);
            }
        }

        // Change turn handler
        if (players.Count > 1)
        {
            if (players[nextTurn].placedPawn[0])
            {
                players[nextTurn].SrvSetTurn(false);
                if (nextTurn == 0)
                    nextTurn++;
                else
                    nextTurn--;
                shouldRandomizeNextPawn = true;
            }
        }





        //for (int i = 0; i < players.Count; i++)
        //{
        //    if (players[i].IsThinking)
        //    {
        //        levelManager.RandomizePawn = false;
        //    }
        //}

    }
}
