using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


[RequireComponent(typeof (TriggerByCharacter))]
public class CutsceneTrigger : PlayerActionType
{
    public Cutscene cutscene;

}
