using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Table : NetworkBehaviour
{
    public Transform InitialTransform;

    private void Awake()
    {
        InitialTransform = this.transform;
    }

    ///// <summary>
    ///// The setter for this transform
    ///// </summary>
    //[Command(requiresAuthority = false)]
    //public void InitializeTransform()
    //{
    //    RpcInitializeTransform();
    //}

    //[ClientRpc]
    //public void RpcInitializeTransform()
    //{
    //    //GetComponent<Collider>().enabled = false;
    //    //GetComponent<Rigidbody>().isKinematic = true;
    //    //GetComponent<Rigidbody>().useGravity = false;
    //    this.transform.position = InitialTransform.position;
    //    this.transform.rotation = Quaternion.identity;
    //}

    //[Command(requiresAuthority =false)]
    //public void CmdEnable(bool coll, bool isKinematic, bool gravity)
    //{
    //    RpcCmdEnable(coll, isKinematic, gravity);

    //}
    //[ClientRpc]
    //public void RpcCmdEnable(bool coll, bool isKinematic, bool gravity)
    //{
    //    GetComponent<Collider>().enabled = coll;
    //    GetComponent<Rigidbody>().isKinematic = isKinematic;
    //    GetComponent<Rigidbody>().useGravity = gravity;
    //}
}
