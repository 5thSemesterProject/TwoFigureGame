using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public enum Characters
{
    Man,
    Woman
}

public class CharacterData
{
    public CharacterData(GameObject obj)
    {
        gameObject = obj;
        movement = gameObject.GetComponent<Movement>();
        animator = gameObject.GetComponentInChildren<Animator>();

        var rigidbodysOnCharacter = gameObject.GetComponentsInChildren<Rigidbody>();
        foreach (var rigidbody in rigidbodysOnCharacter)
        {
            if (rigidbody.gameObject.layer == 9)
            {
                roomFadeRigidBody = rigidbody.gameObject;
            }
        }
    }

    public GameObject gameObject;
    public Movement movement;
    public Animator animator;
    public CharacterState currentState;
    public CinemachineVirtualCamera virtualCamera;
    public OxygenData oxygenData;
    public GameObject roomFadeRigidBody;
    public CharacterData other;
    public CharacterState lastState;
    public WSUI_Element oxygenBar;


}

public class WomanData : CharacterData
{
    public WomanData(GameObject obj) : base(obj)
    {
    }
}

public class ManData : CharacterData
{
    public ManData(GameObject obj) : base(obj)
    {
    }
}