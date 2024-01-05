using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class Cutscene : MonoBehaviour
{
    [SerializeField] ActorData maleData;
    [SerializeField] ActorData femaleData;
    public  CutsceneHandler cutsceneHandler;

    PlayableDirector playableDirector;

    public void  Awake()
    {
        maleData.PrepareActorForScene();
        maleData.characterType = CharacterType.Man;
        femaleData.PrepareActorForScene();
        femaleData.characterType = CharacterType.Woman;

        playableDirector = GetComponent<PlayableDirector>();

        cutsceneHandler = new CutsceneHandler(new ActorData[]{maleData,femaleData},playableDirector);
    }
}
