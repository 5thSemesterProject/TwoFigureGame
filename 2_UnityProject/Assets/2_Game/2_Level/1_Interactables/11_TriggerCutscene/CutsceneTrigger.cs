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
       femaleModel.GetComponentInChildren<CharacterController>().detectCollisions = false;
       maleModel.GetComponentInChildren<CharacterController>().detectCollisions = false;
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
}
