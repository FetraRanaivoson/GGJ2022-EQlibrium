using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RestartButton : MonoBehaviour, IPointerDownHandler
{

    /// <summary>
    /// Is this restart button selected?
    /// </summary>
    public bool isSelected = false;


    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log(this.gameObject.name + " Was Clicked.");
        isSelected = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("The mouse click was released");
        isSelected = false;
    }
}
