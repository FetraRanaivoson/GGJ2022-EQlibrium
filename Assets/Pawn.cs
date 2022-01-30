using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(AudioSource))]

public class Pawn : NetworkBehaviour
{
    Rigidbody rb;
    AudioSource audioSource;
    public AudioClip[] placedClip;
    public Sprite sprite;
    public Collider c;

    public PreviewTrigger previewTrigger;
    LevelManager levelManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        levelManager = FindObjectOfType<LevelManager>();
    }

    /// <summary>
    /// The method to let us use gravity or not for this pawn
    /// </summary>
    [ClientRpc]
    public void UseGravity(bool state)
    {
        rb.useGravity = state;
    }

    /// <summary>
    /// The method that makes this object dynamic or kinematic
    /// </summary>
    [ClientRpc]
    public void IsKinematic(bool state)
    {
        rb.isKinematic = state;
    }

    /// <summary>
    /// The method that makes this object collidable or not
    /// </summary>
    [ClientRpc]
    public void EnableCollider(bool state)
    {
        c.enabled = state;
    }

    /// <summary>
    /// The method to be called play the sound od this object
    /// </summary>
    [ClientRpc]
    public void OnPlacedSound()
    {
        audioSource.clip = placedClip[0];
        audioSource.Play();
    }


    private void OnCollisionEnter(Collision collision)
    {
        CmdOnCollisionEnter(collision.gameObject);
    }

    /// <summary>
    /// The method to get the sprite of this pawn
    /// </summary>
    /// <returns></returns>
    public Sprite GetDisplayImage()
    {
        return sprite;
    }

    /// <summary>
    /// The command on collision
    /// </summary>
    //[Command(requiresAuthority = false)]
    public void CmdOnCollisionEnter(GameObject gameObject)
    {
        if (gameObject.CompareTag("Ground"))
        {
            SrvDestroyMe();
            return;
        }

        else if (!gameObject.CompareTag("Pawn") || !gameObject.CompareTag("Ground"))
        {
            SrvOnColllisionEnterSound();
        }


    }

    /// <summary>
    /// Auto destruction
    /// </summary>
    [Server]
    private void SrvDestroyMe()
    {
        levelManager.CmdRemovePawn(this.gameObject);
        NetworkServer.Destroy(this.gameObject);
        //RpcDestroyMe();   
    }


    /// <summary>
    /// The server response to play collision sounds
    /// </summary>
    //[Server]////////////////////////////////////////////////////////
    private void SrvOnColllisionEnterSound()
    {
        CmdOnCollisionEnterSound();
    }



    /// <summary>
    /// The function that let play collision sound of this object to all clients
    /// </summary>
    [Command(requiresAuthority = false)]///////////////////////////////////
    private void CmdOnCollisionEnterSound()
    {
        RpcOnCollisionEnterSound();
    }

    [ClientRpc]
    private void RpcOnCollisionEnterSound()
    {
        audioSource.clip = placedClip[UnityEngine.Random.Range(0, placedClip.Length)];
        audioSource.Play();
    }


    /// <summary>
    /// The torque function for this pawn
    /// </summary>
    public void Torque(Vector2 moveDir, float force, float deltaTime)
    {
        transform.Rotate(moveDir.y * force, moveDir.x * force, 0);
    }
}