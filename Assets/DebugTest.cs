using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTest : MonoBehaviour
{
    public void OnCollisionEnter(Collision collision)
    {
        GameObject go = new GameObject("P");
        go.transform.position = collision.contacts[0].point;
        Debug.LogError(collision.collider.bounds);
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
