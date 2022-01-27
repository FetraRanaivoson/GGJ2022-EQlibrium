using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewTrigger : MonoBehaviour
{
    public bool isTriggeringPawn = false;

    GameObject parent;

    private void Awake()
    {
        parent = transform.parent.gameObject;
    }

    private void OnTriggerStay(Collider other)
    {
        CmdOnTriggerEnterPreview(other.gameObject);
    }

    public void CmdOnTriggerEnterPreview(GameObject triggered)
    {
        if(triggered.gameObject == parent)
        {
            isTriggeringPawn = false;
            return;
        }
        if (triggered.gameObject.CompareTag("Preview"))
        {
            SrvOnTriggerEnterPreview();
        }
    }

    private void SrvOnTriggerEnterPreview()
    {
        ClientOnTriggerEnterPreview();
    }

    private void ClientOnTriggerEnterPreview()
    {
        isTriggeringPawn = true;
    }


    private void OnTriggerExit(Collider other)
    {
        CmdOnTriggerExitPreview(other.gameObject);
    }

    private void CmdOnTriggerExitPreview(GameObject triggered)
    {
        if (triggered.gameObject.CompareTag("Preview"))
        {
            SrvOnTriggerExitPreview();
        }
    }

    private void SrvOnTriggerExitPreview()
    {
        ClientOnTriggerExitPreview();
    }

    private void ClientOnTriggerExitPreview()
    {
        isTriggeringPawn = false;
    }

    //private void Update()
    //{
    //    //isTriggeringPawn = false;

    //    //RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, 50, LayerMask.GetMask("Pawn"));
    //    //if (hits.Length > 0)
    //    //{
    //    //    //Debug.Log("hitting " + hits[0].collider.gameObject.name);
    //    //    isTriggeringPawn = true;
    //    //}

    //}



}
