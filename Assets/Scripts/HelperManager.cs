using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HelperManager : NetworkBehaviour
{
    public GameObject helperPrefab;
    [SerializeField] public List<GameObject> helpers;

    Vector3 lastMousePos = Vector3.zero;
    Vector3 currentMousePos = Vector3.zero;

    /// <summary>
    /// The function to be called when instantiating an helper
    /// </summary>
    public void Highlight(Vector3 point, Vector3 mousePos)
    {
        CmdInstantiateHelper(point, mousePos);
    }

    /// <summary>
    /// The function that deletes an helper
    /// </summary>
    [Server]
    private void SrvDeleteHelper(GameObject helper)
    {
        helpers.Remove(helper);
        NetworkServer.Destroy(helper);
    }

    /// <summary>
    /// The command to delete the last helper (basically the only visible) from external code
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdDeleteHelper()
    {
        if (!NetworkClient.ready)
            SrvDeleteHelper(helpers[helpers.Count - 1]);
    }

    /// <summary>
    /// The request command to instantiate the helper
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdInstantiateHelper(Vector3 point, Vector3 mousePos)
    {
        currentMousePos = mousePos;
        // Check if the mouse changes its position then instantiate the helper
        if (currentMousePos != lastMousePos)
        {
            SrvInstantiateHelper(point);
            lastMousePos = currentMousePos;
        }

        // Then, keep only the last one created 
        for (int i = 0; i < helpers.Count; i++)
        {
            if (i != helpers.Count - 1 && helpers.Count > 1 && helpers[i] != null)
            {
                SrvDeleteHelper(helpers[i]);
            }
        }
        // Delete last helper after a few second
        //StartCoroutine(SrvDeleteHelperAfter(2.5f));
    }


    /// <summary>
    /// The server response to instantiate the helper
    /// </summary>
    [Server]
    public void SrvInstantiateHelper(Vector3 point)
    {
        GameObject helper = Instantiate(helperPrefab, point, Quaternion.identity);
        NetworkServer.Spawn(helper);
        helpers.Add(helper);
    }

    /// <summary>
    /// The coroutine that let delete the last helper on the scene 
    /// </summary>
    /// <returns></returns>
    IEnumerator SrvDeleteHelperAfter(float second)
    {
        yield return new WaitForSeconds(second);
        if (helpers.Count > 0 && helpers[helpers.Count - 1] != null)
        {
            SrvDeleteHelper(helpers[helpers.Count - 1]);
        }
    }


}
