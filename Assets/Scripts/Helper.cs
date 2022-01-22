using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Helper : NetworkBehaviour
{
    bool isCollidingPawn = false;
    public Material material;
    Color initialColor;
    Color initialEmissionColor;

    private void Awake()
    {
        initialColor = material.GetColor("_Color");
        initialEmissionColor = material.GetColor("_EmissionColor");
    }

}
