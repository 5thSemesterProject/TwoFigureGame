using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

abstract class CutsceneState : CharacterState
{
    protected CutsceneHandler cutsceneHandler;

    protected  CutsceneState(CharacterData data, CutsceneHandler cutsceneHandler) : base(data)
    {
        this.cutsceneHandler = cutsceneHandler;
    }

    public override abstract CharacterState SpecificStateUpdate();

}


class WalkTowards : CutsceneState
{   
    Vector2 targetDir;
    Vector2 targetPos;
    float intitialTargetDistance;
    float tolerance = 0.05f;
    public WalkTowards(CharacterData data, CutsceneHandler cutsceneHandler) : base(data, cutsceneHandler)
    {
        updateLastState = false;
        handleInteractables = false;
        characterData.gameObject.GetComponent<CharacterController>().detectCollisions = false;

        GameObject actor = cutsceneHandler.GetActorData(characterData.movement.characterType).actor;

        targetPos = VectorHelper.Convert3To2(actor.transform.position);
        targetDir = actor.transform.forward;

        intitialTargetDistance = Vector2.Distance(VectorHelper.Convert3To2(characterData.gameObject.transform.position),targetPos)-tolerance;

        characterData.movement.GetComponent<NavMeshHandler>().MovePlayerToPos(actor.transform.position);
    }

    public override CharacterState SpecificStateUpdate()
    {    
        if (characterData.other.currentState is WaitForOtherState)
            characterData.virtualCamera.gameObject.SetActive(true);

        Vector2 playerPos = VectorHelper.Convert3To2(characterData.gameObject.transform.position);
        float distanceToTarget = Vector2.Distance(playerPos,targetPos);
        
        if (distanceToTarget>tolerance)
        {
            Vector2 moveDirection = GetMoveDirection(targetPos,playerPos);

            //Slowly Lerp Player into Direction
            moveDirection = Vector2.Lerp(moveDirection,targetDir,1-distanceToTarget/intitialTargetDistance);
            moveDirection = moveDirection.normalized;

            //characterData.movement.MovePlayer(moveDirection);

            return this;
        }
            
        else
        {   
            characterData.gameObject.GetComponentInChildren<NavMeshAgent>().speed = 0;

            //Set Positions and Rotation exactly
            characterData.gameObject.transform.position = targetPos;
            characterData.gameObject.transform.rotation = cutsceneHandler.GetActorData(characterData.movement.characterType).actor.transform.rotation;
            
            //Wait For Other Character to reach the cutscene Pos
            return new WaitForOtherState(characterData,cutsceneHandler);
        }
            
    }

    Vector2 GetMoveDirection(Vector2 targetPos, Vector2 playerPos)
    {
        Vector2 direction =  playerPos-targetPos;
        direction = direction.normalized;
        return  direction;
    }

    public CutsceneHandler GetCutSceneHandler()
    {
        return cutsceneHandler;
    }
}

class WaitForOtherState : CutsceneState
{
    public WaitForOtherState(CharacterData data,CutsceneHandler cutsceneHandler) : base(data,cutsceneHandler)
    {
        updateLastState = false;
        handleInteractables = false;

        Vector2 moveDir = VectorHelper.Convert3To2(cutsceneHandler.GetActorData(characterData.movement.characterType).actor.transform.forward);
        characterData.movement.MovePlayer(moveDir,0);
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (characterData.other.currentState is WalkTowards)
        {
            characterData.virtualCamera.gameObject.SetActive (false);
        }
            

        if (characterData.other.currentState is WaitForOtherState ||characterData.other.currentState is PlayCutsceneState )
        {   
            return new PlayCutsceneState(characterData,cutsceneHandler);
        }
            

        return this;
    }
}

class PlayCutsceneState : CutsceneState
{
    PlayableDirector playableDirector;

    public PlayCutsceneState(CharacterData data, CutsceneHandler cutsceneHandler) : base(data,cutsceneHandler)
    {
        handleInteractables = false;
        updateLastState = false;

        playableDirector = cutsceneHandler.GetPlayableDirector();
        
        //Swap To Actor Model
        cutsceneHandler.SwapToActorModel(characterData.gameObject,characterData.movement.characterType);

        //Start Cutscene if not already playing
        if (playableDirector.state != PlayState.Playing)
            playableDirector.Play();

    }

    public override CharacterState SpecificStateUpdate()
    {
        if (playableDirector.state == PlayState.Paused || playableDirector.time>=playableDirector.duration)
        {   
            //Activate Play Model Again
            cutsceneHandler.SwapToPlayModel(characterData.gameObject,characterData.movement.characterType);
            
            //Activate Collisions again
            characterData.gameObject.GetComponent<CharacterController>().detectCollisions = true;

            //Return to previous States
            return new RecoverLastState(characterData,cutsceneHandler);
        }

        //Turn off player camera
        if (characterData.virtualCamera.gameObject.activeInHierarchy == true &&!Camera.main.GetComponent<CinemachineBrain>().IsBlending &&
            Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera != characterData.other.virtualCamera as ICinemachineCamera)
            characterData.other.virtualCamera.gameObject.SetActive(false);

        return this;
    }

    
}


class RecoverLastState : CutsceneState
{
    public RecoverLastState(CharacterData data, CutsceneHandler cutsceneHandler) : base(data,cutsceneHandler)
    {
        handleInteractables = false;
        updateLastState = false;
    }

    public override CharacterState SpecificStateUpdate()
    {
        characterData.movement.DisableNavMeshHandling();

        //Return to previous States
        if (characterData.lastState is AIState)
        {
            return new AIState(characterData);
        }
            
        else
        {
            characterData.virtualCamera.gameObject.SetActive(true);
            return new IdleState(characterData);
        }
            
    }

    
}
