using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TimerManager : NetworkBehaviour
{
    /// <summary>
    /// The maximum number of second to wait after a player has placed a pawn
    /// </summary>
    public float MAX_TIME;

    /// <summary>
    /// Is this timer ended?
    /// </summary>
    [SyncVar]
    [SerializeField] public bool timerEnds = true;

    /// <summary>
    /// The boolean that let starts this timer
    /// </summary>
    [SyncVar]
    [SerializeField] private bool startTimer = false;

    /// <summary>
    /// The current timer value if it has started
    /// </summary>
    [SyncVar]
    [SerializeField] public float currentTime = 0;


    void Update()
    {
        if (startTimer)
        {
            CmdUpdateTimer();
        }
        else
        {
            CmdSetCurrentTime(0);
        }
    }

    /// <summary>
    /// The function that updates the timer
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdUpdateTimer()
    {
        if (currentTime >= MAX_TIME)
        {
            CmdSetCurrentTime(0);
            CmdSetTimerEnds(true);
        }
        else
        {
            currentTime += Time.deltaTime;
            CmdSetCurrentTime(currentTime);
        }
    }

    /// <summary>
    /// Start this timer
    /// </summary>
    [Server]
    public void SrvStartTimer(bool state)
    {
        startTimer = state;
    }

    /// <summary>
    /// Set if this timer ends or not
    /// </summary>
    [Server]
    public void SrvTimerEnds(bool state)
    {
        timerEnds = state;
    }

    /// <summary>
    /// 
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdSetTimerEnds(bool state)
    {
        SrvTimerEnds(state);
    }

    /// <summary>
    /// 
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdSetCurrentTime(float value)
    {
        currentTime = value;
    }



}
