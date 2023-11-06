using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterManager : MonoBehaviour
{
    private CharacterState currentState;

    void Start()
    {
        currentState = new SetUpState();
        CharacterState.playerInput = GetComponent<PlayerInput>();
    }


    void Update()
    {
        currentState = currentState.UpdateState();
    }
}

abstract class CharacterState
{
    public static PlayerInput playerInput;

    public abstract CharacterState UpdateState();
}

class SetUpState : CharacterState
{
    public override CharacterState UpdateState()
    {
        return new IdleState();
    }
}

class IdleState : CharacterState
{
    public IdleState()
    {
        playerInput.currentActionMap.actionTriggered += Respond;
    }

    private void Respond(InputAction.CallbackContext context)
    {
        
        Debug.Log(context.action);
    }

    public override CharacterState UpdateState()
    {
        return this;
    }
}
