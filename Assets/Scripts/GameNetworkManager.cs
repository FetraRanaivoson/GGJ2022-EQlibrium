using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameNetworkManager : NetworkManager
{
    [SerializeField] List<PlayerController> players = new List<PlayerController>();
    LevelManager levelManager;
    UIManager UIManager;
    DeadZone deadZone;
    TimerManager timerManager;

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        // Grab the player
        PlayerController player = conn.identity.GetComponent<PlayerController>();

        // Set the name
        player.SetDisplayName($"Player{numPlayers}");

        //Add to list
        players.Add(player);

        //In case 1 player exists
        if (NetworkServer.active && players.Count == 1)
        {
            players[0].OnWaitingForOpponent();
            players[0].OnPauseGame();

            levelManager = FindObjectOfType<LevelManager>();
            UIManager = FindObjectOfType<UIManager>();
            deadZone = FindObjectOfType<DeadZone>();
            timerManager = FindObjectOfType<TimerManager>();
        }
        // In case 2 players exist
        if (NetworkServer.active && players.Count > 1)
        {
            players[0].OnUnPauseGame();
            players[0].OnNotWaitingForOpponent();

            levelManager.InstantiatePlatform();
        }

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
    bool timerStarts = false;


    private void Update()
    {
        if (NetworkServer.active && players.Count > 1)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null && NetworkServer.active)
                    players[i].SetOpponents(players);
            }
        
    
           

            // Wait until everything settle
            if (timerStarts)
            {
                return;
            }

            // Then randomize next pawn
            if (shouldRandomizeNextPawn && players.Count == 2)
            {
                if (levelManager != null)
                {
                    //levelManager.ResetLevel(false);
                    levelManager.SetNextPawn();
                    shouldRandomizeNextPawn = false;

                    levelManager.DisplayNextPawn();
                }
                // TO DO: shouldn't be only for one and then rpc?
                //for (int i = 0; i < players.Count; i++)
                //{
                //players[i].DisplayNextPawn(levelManager.nextPawn[0]);

                //}
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
            if (players.Count == 2)
            {
                if (players[nextTurn].placedPawn[0])
                {
                    //  Temporarily deactivate
                    players[nextTurn].SrvSetTurn(false);
                    //  Wait ?
                    timerStarts = true;
                    StartCoroutine(WaitToSettle());
                }
            }
        }
    }

    /// <summary>
    /// The logic when waiting for every physics to settle
    /// </summary>
    IEnumerator WaitToSettle()
    {
        timerManager.SrvStartTimer(true);
        timerManager.SrvTimerEnds(false);

        while (!timerManager.timerEnds)
        {
            // Live display the timer
            UIManager.DisplayTimer(timerManager.currentTime);

            // In the meantime, check if top table is inside dead zone
            if (deadZone.isTouched)
            {
                // To prevent going here many times
                deadZone.isTouched = false;

                // Reset timer
                timerManager.SrvStartTimer(false);
                timerManager.SrvTimerEnds(true);
                UIManager.FadeTimer();

                //  Increase player score
                if (nextTurn == 0)
                {
                    players[nextTurn + 1].SetScore();
                }
                else
                {
                    players[nextTurn - 1].SetScore();
                }

                //  Delete the platform
                levelManager.DestroyPlatform();

                // Wait for everything to be destroyed then spawn another platform
                StartCoroutine(WaitForDestruction());

                //  To let us randomize pawn again
                timerStarts = false;
                shouldRandomizeNextPawn = true;

                // Set the next turn
                players[nextTurn].SrvSetTurn(false);
                if (nextTurn == 0)
                    nextTurn++;
                else
                    nextTurn--;

                yield break;
            }
            yield return null;
        }
        //  In case timer ends without touching the dead zone
        // Reset timer
        timerManager.SrvStartTimer(false);
        timerManager.SrvTimerEnds(true);
        UIManager.FadeTimer();

        //  To let us randomize pawn again
        timerStarts = false;
        shouldRandomizeNextPawn = true;

        // To prevent going here many times
        deadZone.isTouched = false;

        // Set the next turn
        players[nextTurn].SrvSetTurn(false);
        if (nextTurn == 0)
            nextTurn++;
        else
            nextTurn--;


    }

    IEnumerator WaitForDestruction()
    {
        do
        {
            yield return null;
        } while (levelManager.pawnInstances.Count > 1);
        yield return new WaitForSeconds(1.5f);
        levelManager.InstantiatePlatform();
    }
}
