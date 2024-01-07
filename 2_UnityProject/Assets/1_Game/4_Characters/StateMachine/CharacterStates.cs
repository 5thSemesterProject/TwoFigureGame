using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

class SetUpState : CharacterState
{
    public SetUpState(CharacterData characterData) : base(characterData) { }

    public override CharacterState SpecificStateUpdate()
    {
        if (characterData is ManData)
            return new AIState(characterData);
        return new IdleState(characterData);
    }
}

class IdleState : CharacterState
{

    public IdleState(CharacterData characterData) : base(characterData)
    {
        characterData.movement.MovePlayerFromCamera(Vector2.zero, 0);
        
        if (characterData.audioListener == null)
            characterData.audioListener = characterData.gameObject.AddComponent<AudioListener>();

        //Allo to walk on pressureplates
        characterData.navMeshHandler.AllowPressurePlateWalking(true);
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
            return new AIState(characterData);

        Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
        if (inputVector.magnitude > 0)
            return SwitchState(new MoveState(characterData));



        if (characterData.movement.interactable != null && CharacterManager.customInputMaps.InGame.Action.triggered)
            //characterData.movement.interactable.TriggerByPlayer();
            Debug.Log ("");

        return this;
    }
}

class MoveState : CharacterState
{
    public MoveState(CharacterData data) : base(data)
    {

    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
            return new AIState(characterData);

        if (characterData.movement.interactable != null && CharacterManager.customInputMaps.InGame.Action.triggered)
            characterData.movement.interactable.Trigger(characterData.movement);

        Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
        Vector2 MovementVector = characterData.movement.MovePlayerFromCamera(inputVector);
        if (MovementVector.magnitude <= 0)
            return SwitchState(new IdleState(characterData));


        return this;

    }
}

class CrawlState : CharacterState
{   
    
    public CrawlState(CharacterData data) : base(data)
    {
        characterData.movement.StartTraversing(characterData.movement.interactable, TraversalType.Crawl, 2);
        handleInteractables = false;
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
            return new AIState(characterData);

        if (characterData.movement.traversalRoutine == null)
            return new IdleState(characterData);

        return this;

    }
}

class JumpOverState : CharacterState
{
    public JumpOverState(CharacterData data) : base(data)
    {
        characterData.movement.StartTraversing(characterData.movement.interactable, TraversalType.JumpOver, 1.5f);
        handleInteractables = false;
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
            return new AIState(characterData);

        if (characterData.movement.traversalRoutine == null)
            return new IdleState(characterData);

        return this;

    }
}

class MoveObjectState : CharacterState
{
    private MoveBox movableObject;
    private Transform previousParent;

    public MoveObjectState(CharacterData data) : base(data)
    {
        //Disable Move
        characterData.movement.TerminateMove();

        //Disable ability to interact with other objects
        handleInteractables = false;

        //Get the NoveObject Script if it exists
        if (data.movement.interactable.GetComponent<MoveBox>() != null)
        {
            movableObject = data.movement.interactable.GetComponent<MoveBox>();
            movableObject.currentMover = characterData.movement;
        }
        else
        {
            Debug.LogWarning("Interactable activated MoveObject State but is not MoveObject!");
        }

        //Parent Character To Box
        previousParent = characterData.gameObject.transform.parent;
        characterData.gameObject.transform.parent = movableObject.transform;

        //Lerp Player to handle
        characterData.movement.LerpPlayerTo(movableObject.playerHandlePosition, true, 0.2f);

        //Animate
        characterData.animator.SetBool("MoveBox", true);
    }

    public override CharacterState SpecificStateUpdate()
    {
        //Enter AI State if triggered
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
        {
            //Unparent Character
            characterData.gameObject.transform.parent = previousParent;

            return new AIState(characterData);
        }

        //Return to idle if player released the input
        if (CharacterManager.customInputMaps.InGame.Action.phase == InputActionPhase.Waiting)
        {
            //Unparent Character
            characterData.gameObject.transform.parent = previousParent;

            //Stop lerp to handle
            characterData.movement.StopLerp();

            //Animator
            characterData.animator.SetBool("MoveBox", false);

            return new IdleState(characterData);
        }

        Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
        Vector2 gameWorldVector = VectorHelper.Convert3To2(Camera.main.transform.forward).normalized * inputVector.y + VectorHelper.Convert3To2(Camera.main.transform.right).normalized * inputVector.x;
        float moveDir = movableObject.MoveWithObject(gameWorldVector);
        characterData.animator.SetFloat("PushPull", moveDir, 0.1f, Time.deltaTime);
        switch (moveDir)
        {
            case -1:
                //Trigger Backwards Movement
                break;
            case 0:
                //Trigger Idle
                break;
            case 1:
                //Trigger Forward Movement
                break;
            default:
                Debug.LogWarning("Something went wrong!");
                break;
        }

        return this;
    }
}

    class GodModeState : CharacterState
    {
        public GodModeState(CharacterData data) : base(data)
        {
            handleInteractables = true;
            handleOxygen = false;
            updateLastState = false;
        }

        public override CharacterState SpecificStateUpdate()
        {
            if (CharacterManager.customInputMaps.InGame.Switch.triggered)
                return new AIState(characterData);
            
            Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
            characterData.movement.MovePlayerFromCamera(inputVector,4);
            characterData.characterOxygenData.oxygenData.currentOxygen = 100;

            if (Input.GetKeyDown(KeyCode.Backspace))
                return characterData.lastState;

            return this;
        }

    }

