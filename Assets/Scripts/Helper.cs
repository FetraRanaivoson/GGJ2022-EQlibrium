using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Collider))]

public class Helper : NetworkBehaviour
{
    public Material material;
    Color initialColor;
    Color initialEmissionColor;

    public GameObject colliderHelper;

    public bool isCollidingPawn = false;

    private void Awake()
    {
        initialColor = material.GetColor("_Color");
        initialEmissionColor = material.GetColor("_EmissionColor");
    }


}
