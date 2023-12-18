using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


[RequireComponent(typeof (TriggerOnEnter))]
public class CutsceneTrigger : PlayerActionType
{
    [SerializeField] PlayableDirector playableDirector;

    [SerializeField] ActorData maleData;
    [SerializeField] ActorData femaleData;

    CutsceneHandler cutsceneHandler;

    public void  Awake()
    {
        maleData.PrepareActorForScene();
        maleData.characterType = CharacterType.Man;
        femaleData.PrepareActorForScene();
        femaleData.characterType = CharacterType.Woman;

        cutsceneHandler = new CutsceneHandler(new ActorData[]{maleData,femaleData},playableDirector);
    }

    public CutsceneHandler GetCutsceneHandler()
    {
        return cutsceneHandler;
    }
}
