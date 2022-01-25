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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// The method to let us use gravity or not for this pawn
    /// </summary>
    public void UseGravity(bool state)
    {
        rb.useGravity = state;
    }

    /// <summary>
    /// The method that makes this object dynamic or kinematic
    /// </summary>
    public void IsKinematic(bool state)
    {
        rb.isKinematic = state;
    }

    /// <summary>
    /// The method that makes this object collidable or not
    /// </summary>
    public void EnableCollider(bool state)
    {
        c.enabled = state;
    }

    /// <summary>
    /// The method to be called play the sound od this object
    /// </summary>
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
    /// The command that request the server to play collision sounds
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdOnCollisionEnter(GameObject gameObject)
    {
        if (!gameObject.CompareTag("Pawn"))
        {
            SrvOnColllisionEnter();
        }
    }

    /// <summary>
    /// The server response to play collision sounds
    /// </summary>
    [Server]
    private void SrvOnColllisionEnter()
    {
        ClientOnCollisionEnter();
    }

    /// <summary>
    /// The function that let play collision sound of this object to all clients
    /// </summary>
    [ClientRpc]
    private void ClientOnCollisionEnter()
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