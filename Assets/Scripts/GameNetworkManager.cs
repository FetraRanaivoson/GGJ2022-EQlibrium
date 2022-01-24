using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class GameNetworkManager : NetworkManager
{
    [SerializeField] List<PlayerController> players;
    LevelManager levelManager;

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
    }

    public override void OnServerChangeScene(string newSceneName)
    {
    }


    int nextTurn = 0;
    bool shouldRandomizePawn = true;

    private void Update()
    {
        if (shouldRandomizePawn)
        {
            if (levelManager != null)
            {
                levelManager.SrvRandomizePawn(true);
                shouldRandomizePawn = false;
            }
        }

        if (players.Count > 0)
        {
            if (players[nextTurn] != null)
            {
                players[nextTurn].CmdSetTurn(true);
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
