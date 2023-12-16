using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneTrigger : PlayerActionType
{
    [SerializeField] GameObject maleModel,femaleModel;
    public PlayableDirector playableDirector;

    public void  Awake()
    {
       femaleModel.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
       maleModel.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;

        
        femaleModel.GetComponent<Movement>().enabled = false;
        maleModel.GetComponent<Movement>().enabled = false;

       //Turn off all colliders
        TurnOffAllCollliders (femaleModel);
        TurnOffAllCollliders(maleModel);

    }

    void TurnOffAllCollliders(GameObject input)
    {
        input.GetComponentInChildren<CharacterController>().detectCollisions = false;
        var colliders = input.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
            collider.enabled = false;
    }

    public Vector3 GetTargetPos(CharacterType characterType)
    {
        return GetCharacter(characterType).transform.position;
    }

    public GameObject GetCharacter(CharacterType characterType)
    {
        if (characterType == CharacterType.Woman)
            return femaleModel;
        else if (characterType == CharacterType.Man)
            return maleModel;
        else
            return null;
    }

    public void StartCutscene()
    {
        playableDirector.Play();
    }

    public void SyncToPlayModel(GameObject playModel, CharacterType characterType)
    {
        SyncCharacterModels(GetCharacter(characterType),playModel);
    }

    public void SyncToCutsceneModel(GameObject playModel, CharacterType characterType)
    {
        SyncCharacterModels(playModel,GetCharacter(characterType),true,GetCharacter(characterType).transform.Find("Geometry").position);
    }

    void SyncCharacterModels(GameObject source, GameObject target, bool overwritePosition=false,Vector3 overwitePos = default)
    {
        //Set up variables
        Animator playerAnimator = source.GetComponent<Animator>();
        AnimatorStateInfo  animatorStateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        Animator cutsceneAnimator = target.GetComponent<Animator>();  

        //Set up Cutscene Animator
        CopyAnimatorParameters(playerAnimator,cutsceneAnimator);
        cutsceneAnimator.Play (animatorStateInfo.fullPathHash,0,animatorStateInfo.normalizedTime);

        //Set exact positions
        Vector3 position = overwritePosition?overwitePos:target.transform.position;

        source.transform.position = position;
        source.transform.rotation = target.transform.rotation;

        //Switch Model visibility
        source.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        target.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
    }

    void CopyAnimatorParameters(Animator source, Animator target)
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
