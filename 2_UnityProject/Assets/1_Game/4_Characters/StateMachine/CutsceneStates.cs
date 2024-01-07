using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.PlayerLoop;

abstract class CutsceneState : CharacterState
{
    protected CutsceneHandler cutsceneHandler;

    protected  CutsceneState(CharacterData data, CutsceneHandler cutsceneHandler) : base(data)
    {
        handleOxygen = false;
        handleInteractables = false;
        this.cutsceneHandler = cutsceneHandler;
    }

    public override abstract CharacterState SpecificStateUpdate();

}

class WaitForDoor: CutsceneState
{
    Door door;

    public WaitForDoor(CharacterData data, CutsceneHandler cutsceneHandler, Door door) : base(data, cutsceneHandler)
    {
        updateLastState = false;
        handleInteractables = false;
        handleOxygen = false;
        
        this.cutsceneHandler = cutsceneHandler;
        this.door = door;

        characterData.movement.TerminateMove();
        characterData.movement.GetComponent<CharacterController>().enabled = false;
        characterData.movement.GetComponent<NavMeshAgent>().enabled = false;
        characterData.navMeshHandler.IdleAnim();

    }

    public override CharacterState SpecificStateUpdate()
    {    
       if (door.GetOpen())
            return new WalkTowards(characterData,cutsceneHandler);
        return this;
    }
}


class WalkTowards : CutsceneState
{   
    Vector2 targetDir;
    Vector3 targetPos;
    float intitialTargetDistance;
    float tolerance = 0.1f;

    GameObject actor;
    public WalkTowards(CharacterData data, CutsceneHandler cutsceneHandler) : base(data, cutsceneHandler)
    {
        updateLastState = false;
        handleInteractables = false;
        handleOxygen = false;
        
        characterData.gameObject.GetComponent<CharacterController>().detectCollisions = false;

        //Turn off UI
        characterData.movement.GetComponent<CharacterController>().enabled = false;
        characterData.movement.GetComponent<NavMeshAgent>().enabled = true;

        //Set up positions
        actor = cutsceneHandler.GetActorData(characterData.movement.characterType).actor;
        targetPos = actor.transform.position;
        targetDir = actor.transform.forward;
        intitialTargetDistance = Vector3.Distance(characterData.gameObject.transform.position,targetPos)-tolerance;

        characterData.movement.TerminateMove();

    }

    public override CharacterState SpecificStateUpdate()
    {    
        if (characterData.other.currentState is WaitForOtherState)
            characterData.virtualCamera.gameObject.SetActive(true);

        Vector3 playerPos = characterData.gameObject.transform.position;
        float distanceToTarget = Vector3.Distance(playerPos,targetPos);
        
        if (distanceToTarget>tolerance)
        {

            characterData.navMeshHandler.MovePlayerToPos(targetPos,1,true,true);
            return this;
        }
            
        else
        {   
            characterData.gameObject.GetComponentInChildren<NavMeshAgent>().speed = 0;
            
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
        handleOxygen = false;

        Vector2 moveDir = VectorHelper.Convert3To2(cutsceneHandler.GetActorData(characterData.movement.characterType).actor.transform.forward);
        characterData.movement.MovePlayerFromCamera(moveDir,0);
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
        handleOxygen = false;

        characterData.navMeshHandler.DisableNavMesh();

        Transform playableRigRoot = characterData.gameObject.transform;
        Transform actorRigRoot = cutsceneHandler.GetActorData(characterData.CharacterType).actor.transform;

        cutsceneHandler.LerpBones(playableRigRoot,actorRigRoot,1);
        cutsceneHandler.LerpPosition(playableRigRoot,actorRigRoot,1);

        //Start Cutscene if not already playing
        playableDirector = cutsceneHandler.GetPlayableDirector();
        if (playableDirector.state != PlayState.Playing)
            playableDirector.Play();

    }

    public override CharacterState SpecificStateUpdate()
    {
        //Return to previous States
        if (playableDirector.state == PlayState.Paused || playableDirector.time>=playableDirector.duration) 
        {   
            //cutsceneHandler.StopBlendingRotationAndPosition();
            return new RecoverLastState(characterData,cutsceneHandler);
        }
            

        //Turn off player camera
        if (characterData.virtualCamera.gameObject.activeInHierarchy == true &&!Camera.main.GetComponent<CinemachineBrain>().IsBlending &&
            Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera != characterData.other.virtualCamera as ICinemachineCamera)
                characterData.other.virtualCamera.gameObject.SetActive(false);

        #if UNITY_EDITOR
        //Skip Cutscene Function
        var playable =  playableDirector.playableGraph.GetRootPlayable(0);
        if (Input.GetKeyDown(KeyCode.Space))
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(10);    
        else if (Input.GetKeyUp(KeyCode.Space))
              playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);             
        #endif  

        return this;
    }

    
}


class RecoverLastState : CutsceneState
{
    public RecoverLastState(CharacterData data, CutsceneHandler cutsceneHandler) : base(data,cutsceneHandler)
    {
        handleInteractables = false;
        updateLastState = false;
        handleOxygen = false;
    }

    public override CharacterState SpecificStateUpdate()
    {
        characterData.movement.DisableNavMeshHandling();

        //Return to previous States
        if (characterData.lastState is AIState)
        {
            characterData.movement.GetComponent<CharacterController>().enabled = false;
            characterData.movement.GetComponent<NavMeshAgent>().enabled = true;
            characterData.movement.GetComponent<NavMeshAgent>().isStopped = false;
            return new AIState(characterData);
        }
            
        else
        {
            characterData.movement.GetComponent<CharacterController>().enabled = true;
            characterData.movement.GetComponent<NavMeshAgent>().enabled = false;
            characterData.virtualCamera.gameObject.SetActive(true);
            return new IdleState(characterData);
        }
            
    }

    
}
