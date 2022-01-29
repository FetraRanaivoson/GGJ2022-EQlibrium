using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameNetworkManager : NetworkManager
{
    /// <summary>
    /// The list of all players
    /// </summary>
    [SerializeField] List<PlayerController> players = new List<PlayerController>();

    /// <summary>
    /// The class responsible of the level which is composed by the 3 elements of the table
    /// </summary>
    LevelManager levelManager;

    /// <summary>
    /// The class responsible for updating the UI: timer, score, pawn display
    /// </summary>
    UIManager UIManager;

    /// <summary>
    /// The dead zone class represented by a box collider down the table
    /// </summary>
    DeadZone deadZone;

    /// <summary>
    /// The class responsible of the timer
    /// </summary>
    TimerManager timerManager;

    /// <summary>
    /// 0 means player 1, 1 means player 2
    /// </summary>
    int nextTurn = 0;

    /// <summary>
    /// Should we randomize the another pawn?
    /// </summary>
    bool shouldRandomizeNextPawn = true;

    /// <summary>
    /// Did the timer in the timer manager started
    /// </summary>
    bool timerStarts = false;

    /// <summary>
    /// The max score needed to win the game
    /// </summary>
    int MAX_SCORE = 2;

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

    bool aPlayerWin = false;

    private void Update()
    {
        if (NetworkServer.active && players.Count > 1)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i] != null && NetworkServer.active)
                    players[i].SetOpponents(players);

                // Reaches max score?
                if (players[i].score == MAX_SCORE)
                {
                    aPlayerWin = true;
                    players[i].SrvShowPopUp("You win!");
                    if (i == 0)
                        players[i + 1].SrvShowPopUp("You lose");
                    else
                        players[i - 1].SrvShowPopUp("You lose");
                    players[0].OnPauseGame();
                }
            }

            //if (aPlayerWin)
            //{
            //    if (NetworkServer.active)
            //    {
                    
            //    }
            //}


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
                    levelManager.SetNextPawn();
                    shouldRandomizeNextPawn = false;
                    levelManager.DisplayNextPawn();
                }
            }

            //  Setting turn
            if (players.Count > 0)
            {
                if (players[nextTurn] != null)
                {
                    players[nextTurn].SrvSetTurn(true);
                }
            }

            if (players.Count == 2)
            {
                //  We need to constantly verify on update if the table falls
                if (deadZone.isTouched)
                {
                    OnTableFalling();
                }
                if (players[nextTurn].placedPawn[0])
                {
                    //  Temporarily deactivate
                    players[nextTurn].SrvSetTurn(false);
                    //  Wait ?
                    timerStarts = true;
                    StartCoroutine(WaitTableToSettle());
                }
            }
        }
    }

    /// <summary>
    /// The logic when the table has fallen
    /// </summary>
    private void OnTableFalling()
    {
        // To prevent going here many times
        deadZone.isTouched = false;

        // Reset timer
        timerManager.SrvStartTimer(false);
        timerManager.SrvTimerEnds(true);
        UIManager.FadeTimer();

        //  Increase player score
        SetScore();

        //  Delete the platform
        levelManager.DestroyPlatform();

        //  TODO: destroy pawns
        // Wait for everything to be destroyed then spawn another platform
        StartCoroutine(WaitForDestructionOnTheGround());

        //  To let us randomize pawn again
        timerStarts = false;
        shouldRandomizeNextPawn = true;

        // Set the next turn
        ChangeTurn();
    }

    /// <summary>
    /// The logic when waiting for every physics to settle
    /// </summary>
    IEnumerator WaitTableToSettle()
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
                SetScore();

                //  Delete the platform
                levelManager.DestroyPlatform();

                // Wait for everything to be destroyed then spawn another platform
                StartCoroutine(WaitForDestructionOnTheGround());

                //  To let us randomize pawn again
                timerStarts = false;
                shouldRandomizeNextPawn = true;

                // Set the next turn
                ChangeTurn();

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
        ChangeTurn();
    }

    /// <summary>
    /// The function that changes the score
    /// </summary>
    private void SetScore()
    {
        if (nextTurn == 0)
        {
            players[nextTurn + 1].SetScore();
        }
        else
        {
            players[nextTurn - 1].SetScore();
        }
    }

    /// <summary>
    /// The function that changes a turn
    /// </summary>
    private void ChangeTurn()
    {
        players[nextTurn].SrvSetTurn(false);
        if (nextTurn == 0)
            nextTurn++;
        else
            nextTurn--;
    }

    /// <summary>
    /// To be used after deleting the existing platform, this is
    /// the coroutine that waits until all pawns are destroyed in order
    /// to instantiate a new platform
    /// </summary>
    IEnumerator WaitForDestructionOnTheGround()
    {
        do
        {
            yield return null;
        } while (levelManager.pawnInstances.Count > 1);
        yield return new WaitForSeconds(1.5f);
        levelManager.InstantiatePlatform();
    }
}
