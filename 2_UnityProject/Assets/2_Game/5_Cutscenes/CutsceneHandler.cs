using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class ActorData
{
    public GameObject actor;
    public Transform transformRef;
    [HideInInspector]public CharacterType characterType;

    public ActorData(GameObject actor,Transform transformRef,CharacterType characterType)
    {
        this.actor = actor;
        this.transformRef = transformRef;
        this.characterType = characterType;
    }

    public void PrepareActorForScene()
    {   
        //Toogle Visibility
        var skinnedMeshRenderer = actor.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer)
            skinnedMeshRenderer.enabled = false;
        
        //Disable Movement Script
        var movement = actor.GetComponentInChildren<Movement>();
        if (movement)
            movement.enabled = false;

        TurnOffAllCollliders (actor);
    }

    
    void TurnOffAllCollliders(GameObject input)
    {
        input.GetComponentInChildren<CharacterController>().detectCollisions = false;
        var colliders = input.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
            collider.enabled = false;
    }
}

public class CutsceneHandler
{
    ActorData[] actorDatas;

    PlayableDirector playableDirector;

    public CutsceneHandler(ActorData[] actorDatas, PlayableDirector playableDirector)
    {
        this.actorDatas = actorDatas;
        this.playableDirector = playableDirector;
    }

    public PlayableDirector GetPlayableDirector()
    {
        return playableDirector;
    }

    public ActorData GetActorData(CharacterType characterType)
    {
        for (int i = 0; i < actorDatas.Length; i++)
        {
            if (actorDatas[i].characterType == characterType)
                return actorDatas[i];
        }
        
        return null;
    }

    public void SwapToPlayModel(GameObject playModel, CharacterType characterType)
    {
        ActorData cutsceneData = GetActorData(characterType);
        SwapCharacterModels(GetActorData(characterType).actor,playModel,true,cutsceneData.transformRef.position);
    }

    public void SwapToActorModel(GameObject playModel, CharacterType characterType)
    {
        ActorData cutsceneData = GetActorData(characterType);

        if (characterType == CharacterType.Man)
            SwapCharacterModels(playModel,cutsceneData.actor);
        if (characterType == CharacterType.Woman)
            SwapCharacterModels(playModel,cutsceneData.actor);
    }

    void SwapCharacterModels(GameObject source, GameObject target, bool overwritePosition=false,Vector3 overwitePos = default)
    {
        //Set up variables
        Animator sourceAnimator = source.GetComponentInChildren<Animator>();
        AnimatorStateInfo  animatorStateInfo = sourceAnimator.GetCurrentAnimatorStateInfo(0);
        Animator targetAnimator = target.GetComponentInChildren<Animator>();  

        //Set up Cutscene Animator
        TransferAnimatorComponents(sourceAnimator,targetAnimator);
        targetAnimator.Play (animatorStateInfo.fullPathHash,0,animatorStateInfo.normalizedTime);

        //Update Position And Rotation
        if (overwritePosition)
            target.transform.position = overwitePos;
        
        target.transform.rotation = source.transform.rotation;
        
        //Swap Model visibility
        source.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        target.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
    }

    void TransferAnimatorComponents(Animator source, Animator target)
    {
        // Get all parameters from the source animator
        int parameterCount = source.parameterCount;
        for (int i = 0; i < parameterCount; i++)
        {
            AnimatorControllerParameter parameter = source.GetParameter(i);

            // Copy the parameter value to the target animator
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    target.SetFloat(parameter.name, source.GetFloat(parameter.name));
                    break;

                case AnimatorControllerParameterType.Int:
                    target.SetInteger(parameter.name, source.GetInteger(parameter.name));
                    break;

                case AnimatorControllerParameterType.Bool:
                    target.SetBool(parameter.name, source.GetBool(parameter.name));
                    break;

                case AnimatorControllerParameterType.Trigger:
                    // Optionally handle triggers if needed
                    break;

                default:
                    break;
            }
        }
    }

}
