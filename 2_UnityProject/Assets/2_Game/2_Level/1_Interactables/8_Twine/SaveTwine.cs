using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class SaveTwine : MonoBehaviour
{
    Interactable interactable;
    void  Start()
    {
        interactable.triggerEvent+=SaveToTwine;
    }

    void SaveToTwine(Movement movement)
    {

    }
}
