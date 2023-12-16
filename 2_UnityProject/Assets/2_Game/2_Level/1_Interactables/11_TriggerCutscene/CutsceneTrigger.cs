using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneTrigger : PlayerActionType
{
    [SerializeField] GameObject maleModel,femaleModel;

    [SerializeField] Transform malePos, femalePos;
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

    void Update()
    {
       // Debug.Log(CharacterManager.ActiveCharacterData.gameObject.transform.position);
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

    public void ToPlayModel(GameObject playModel, CharacterType characterType)
    {
        if (characterType == CharacterType.Man)
            SyncCharacterModels(GetCharacter(characterType),playModel,true,malePos.position);
        if (characterType == CharacterType.Woman)
            SyncCharacterModels(GetCharacter(characterType),playModel,true,femalePos.position);
    }

    public void ToCutsceneModel(GameObject playModel, CharacterType characterType)
    {
        if (characterType == CharacterType.Man)
            SyncCharacterModels(playModel,GetCharacter(characterType));
        if (characterType == CharacterType.Woman)
            SyncCharacterModels(playModel,GetCharacter(characterType));
    }

    void SyncCharacterModels(GameObject source, GameObject target, bool overwritePosition=false,Vector3 overwitePos = default)
    {
        //Set up variables
        Animator sourceAnimator = source.GetComponentInChildren<Animator>();
        AnimatorStateInfo  animatorStateInfo = sourceAnimator.GetCurrentAnimatorStateInfo(0);
        Animator targetAnimator = target.GetComponentInChildren<Animator>();  

        //Set up Cutscene Animator
        CopyAnimatorParameters(sourceAnimator,targetAnimator);
        targetAnimator.Play (animatorStateInfo.fullPathHash,0,animatorStateInfo.normalizedTime);

        Debug.Log("Original pos = "+target.transform.position);
        //Set exact positions
        Vector3 position = overwritePosition ? overwitePos : target.transform.position;
        if (overwritePosition)
        {
        Debug.Log(overwitePos);
        Debug.Log(CharacterManager.ActiveCharacterData.gameObject.name + "---" + target.gameObject.name);
        target.transform.position = position;
        Debug.Log(target.transform.position);
        }

        target.transform.rotation = source.transform.rotation;
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
