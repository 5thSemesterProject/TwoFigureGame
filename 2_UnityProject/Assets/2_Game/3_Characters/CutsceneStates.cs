using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

abstract class CutsceneState : CharacterState
{
    protected CutsceneTrigger cutsceneTrigger;

    protected  CutsceneState(CharacterData data, CutsceneTrigger cutsceneTrigger) : base(data)
    {
        this.cutsceneTrigger = cutsceneTrigger;
    }

    public override abstract CharacterState SpecificStateUpdate();

}


class WalkTowards : CutsceneState
{   
    Vector2 targetDir;
    Vector2 targetPos;
    float intitialTargetDistance;
    float tolerance = 0.05f;
    public WalkTowards(CharacterData data, CutsceneTrigger cutsceneTrigger) : base(data, cutsceneTrigger)
    {
        updateLastState = false;
        handleInteractables = false;
        characterData.gameObject.GetComponent<CharacterController>().detectCollisions = false;

        targetPos = VectorHelper.Convert3To2(cutsceneTrigger.GetTargetPos(characterData.movement.characterType));
        targetDir = cutsceneTrigger.GetCharacter(characterData.movement.characterType).transform.forward;
        intitialTargetDistance = Vector2.Distance(VectorHelper.Convert3To2(characterData.gameObject.transform.position),targetPos)-tolerance;

    }

    public override CharacterState SpecificStateUpdate()
    {    
        Vector2 playerPos = VectorHelper.Convert3To2(characterData.gameObject.transform.position);
        float distanceToTarget = Vector2.Distance(playerPos,targetPos);
        
        if (distanceToTarget>tolerance)
        {
            Vector2 moveDirection = GetMoveDirection(targetPos,playerPos);

            //Slowly Lerp Player into Direction
            moveDirection = Vector2.Lerp(moveDirection,targetDir,1-distanceToTarget/intitialTargetDistance);
            moveDirection = moveDirection.normalized;

            characterData.movement.MovePlayer(moveDirection);

            return this;
        }
            
        else
        {
            characterData.gameObject.transform.position = targetPos;
            characterData.gameObject.transform.rotation = cutsceneTrigger.GetCharacter(characterData.movement.characterType).transform.rotation;
            return new WaitForOtherState(characterData,cutsceneTrigger);
        }
            
        
    }

    Vector2 GetMoveDirection(Vector2 targetPos, Vector2 playerPos)
    {
        Vector2 direction =  playerPos-targetPos;
        direction = direction.normalized;
        return  direction;
    }

    public CutsceneTrigger GetCutsceneTrigger()
    {
        return cutsceneTrigger;
    }
}

class WaitForOtherState : CutsceneState
{
    public WaitForOtherState(CharacterData data,CutsceneTrigger cutsceneTrigger) : base(data,cutsceneTrigger)
    {
        updateLastState = false;
        handleInteractables = false;

        Vector2 moveDir = VectorHelper.Convert3To2(cutsceneTrigger.GetCharacter(characterData.movement.characterType).transform.forward);

        characterData.movement.MovePlayer(moveDir,0);
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (characterData.other.currentState is WaitForOtherState ||characterData.other.currentState is PlayCutsceneState )
            return new PlayCutsceneState(characterData,cutsceneTrigger);

        return this;

    }
}

class PlayCutsceneState : CutsceneState
{
    public PlayCutsceneState(CharacterData data, CutsceneTrigger cutsceneTrigger) : base(data,cutsceneTrigger)
    {
        handleInteractables = false;
        updateLastState = false;
        
        cutsceneTrigger.ToCutsceneModel(characterData.gameObject,characterData.movement.characterType);

        if (cutsceneTrigger.playableDirector.state != PlayState.Playing)
            cutsceneTrigger.StartCutscene();

    }

    public override CharacterState SpecificStateUpdate()
    {
        if (cutsceneTrigger.playableDirector.state == PlayState.Paused || cutsceneTrigger.playableDirector.time>=cutsceneTrigger.playableDirector.duration)
        {
            cutsceneTrigger.ToPlayModel(characterData.gameObject,characterData.movement.characterType);
            characterData.gameObject.GetComponent<CharacterController>().detectCollisions = true;
                Debug.Log(characterData.gameObject.transform.position);
            if (characterData.lastState is AIState)
                return new AIState(characterData);
            else
                return new IdleState(characterData);
        }

        return this;
    }
}
