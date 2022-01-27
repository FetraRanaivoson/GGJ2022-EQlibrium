using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    public bool isTouched = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Top"))
        {
            isTouched = true;
        }
    }
}
