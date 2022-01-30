using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Music : NetworkBehaviour
{
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// The way the server plays the background music
    /// </summary>
    [Server]
    public void SrvPlayBackground()
    {
        RpcPlayBackground();
    }

    /// <summary>
    /// The way the server tells all clients to play the background music
    /// </summary>
    [ClientRpc]
    private void RpcPlayBackground()
    {
        audioSource.Play();
    }
}
