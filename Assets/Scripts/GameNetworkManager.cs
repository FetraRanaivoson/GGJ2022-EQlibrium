using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class GameNetworkManager : NetworkManager
{
    [SerializeField]List<PlayerController> players;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        // Grab the player
        PlayerController player = conn.identity.GetComponent<PlayerController>();

        // Set the name
        player.SetDisplayName($"Player {numPlayers}");

        //Add to list
        players.Add(player);

        // In case 1 player exists
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

    }

    private void Update()
    {
        //  On the shared UI, display the next object to be placed
        
        //  If player 1 or 2 is turn:
        //  Get its mouse position on screen

        //  Spawn the next object to be placed where the mouse position of that player is
    
        // Update in Player Controller:
            // Screen point to ray: on click and if it is a table, place the base of the object
            // at the clicked position in 3d view
            // SYNC pawn position over the network
        
        //Wait to settle
        
        //If the table collides underneath, then the current player loses. 

            //Display win / lose popup and let be a restart or quit button

        //Else, switch player

    }
}
