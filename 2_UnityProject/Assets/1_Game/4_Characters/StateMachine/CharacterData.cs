using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.AI;

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
        navMeshHandler = gameObject.GetComponent<NavMeshHandler>();
        animator = gameObject.GetComponentInChildren<Animator>();

        if (!gameObject.TryGetComponent (out audioListener))
            audioListener = gameObject.AddComponent<AudioListener>();
            

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
    public  NavMeshHandler navMeshHandler;
    public Animator animator;
    public CharacterType CharacterType{get{return movement.characterType;}}
    public CharacterState currentState;
    public CinemachineVirtualCamera virtualCamera;
    public CharacterOxygenData characterOxygenData;
    public GameObject roomFadeRigidBody;
    public CharacterData other;
    public CharacterState lastState;
    public WSUI_Element oxygenBar;

    public AudioListener audioListener;
    public bool raisedLowOxygenEvent = false;
    public bool raisedChargingEvent = false;
    public float elapsedTime;
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