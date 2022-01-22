using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
    }

    /// <summary>
    /// The method to let us use gravity or not for this pawn
    /// </summary>
    public void UseGravity(bool state)
    {
        rb.useGravity = state;
    }

}