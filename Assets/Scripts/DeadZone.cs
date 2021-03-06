using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    public bool isTouched = false;

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject == null)
            return;
        if (other.CompareTag("Top"))
        {
            isTouched = true;
        }
    }
}
