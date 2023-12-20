using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUI : MonoBehaviour
{
    [SerializeField]GameObject oxygenBarHolder;
    OxygenBar oxygenBar;

    void  Awake()
    {
        oxygenBar = oxygenBarHolder.GetComponentInChildren<OxygenBar>();

        if (oxygenBar==null)
            Debug.LogWarning("Missing Oxygenbar on CharacterUI");
    }

    public OxygenBar GetOxygenBar()
    {
        return oxygenBar;
    }
}
