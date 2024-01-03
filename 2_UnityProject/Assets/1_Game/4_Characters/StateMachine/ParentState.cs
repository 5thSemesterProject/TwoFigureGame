using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CharacterState
{
    protected bool handleInteractables = true;
    protected bool updateLastState = true;
    protected bool activateOxygenBar = true;
    protected bool handleOxygen = true;
    protected Oxygenstation lastOxyggenStation;

    public CharacterState(CharacterData data)
    {
        characterData = data;
    }

    public CharacterData characterData;
    public CharacterState UpdateState() //Update Method that every State checks everytime
    {
        if (updateLastState)
            characterData.lastState = characterData.currentState;

        if (!(characterData.currentState is AIState))
            CamManager.FindOccludingObjects(characterData.gameObject.transform);

        if (handleOxygen)
            HandleOxygen();
        else
            HideOxygenBar();

        if (handleInteractables)
        {
            HandleInteractable(out CharacterState interactableState);
            if (interactableState != null)
                return interactableState;
        }

        //Handle Cutscene
        if (characterData.other.currentState is WalkTowards && !(characterData.currentState is CutsceneState))
        {
            WalkTowards walkTowards = characterData.other.currentState as WalkTowards;
            return new WalkTowards(characterData,walkTowards.GetCutSceneHandler());
        }

           
        return SpecificStateUpdate();
    }

    public void HandleOxygenBar()
    {            
        float currentOxygen = characterData.characterOxygenData.oxygenData.currentOxygen;

        if (currentOxygen<characterData.characterOxygenData.oxygenData.maxOxygen)
        {     
            if (characterData.oxygenBar == null)
                WSUI.FadeInElement(characterData.gameObject.GetComponentInChildren<CharacterUI>().GetOxygenBar().gameObject,characterData.gameObject.transform,out characterData.oxygenBar);
            
            WSUI_Element oxygenBar = characterData.oxygenBar;
            oxygenBar.GetComponent<OxygenBar>().SetValue(currentOxygen);    
        }
        else
        {
           HideOxygenBar();
        }
    }

    public void HideOxygenBar(bool fadeout = false, float smoothTime = 0.33f)
    {
        if (characterData.oxygenBar !=null)
        {
            WSUI.RemoveAndFadeOutPrompt(characterData.oxygenBar,0.05f);
            characterData.oxygenBar = null;
        }
    }


    public void HandleOxygen()
    {   
        //OxygenBar
        if (activateOxygenBar)
            HandleOxygenBar();
        else
            HideOxygenBar();

        //Oxygenstation
        Oxygenstation oxygenstation  = characterData.movement.oxygenstation;
        if (oxygenstation!=null)
        {
            if (characterData.characterOxygenData.oxygenData.currentOxygen<=characterData.characterOxygenData.oxygenData.maxOxygen)
            {
                characterData.characterOxygenData.oxygenData.currentOxygen+=oxygenstation.ChargePlayer();
                characterData.raisedLowOxygenEvent = false;

                //On Charging
                if (!characterData.raisedChargingEvent)
                {
                    characterData.raisedChargingEvent = true;
                    CustomEvents.RaiseChargingOxygen(characterData);
                }
            }
                
        }
        else
        {
            characterData.characterOxygenData.oxygenData.FallOff();
            characterData.raisedChargingEvent = false;

            //Raise Low Health Event
            if (characterData.characterOxygenData.oxygenData.currentOxygen<=characterData.characterOxygenData.lowOxygenThreshhold && !characterData.raisedLowOxygenEvent)
            {
                characterData.raisedLowOxygenEvent = true;
                CustomEvents.RaiseLowOxygen(characterData);
            }
        }

        if (characterData.characterOxygenData.oxygenData.currentOxygen <= 0)
        {
            characterData.animator.SetBool("Dead", true);
        }

        //Increase FallOff With Time
        characterData.elapsedTime +=Time.deltaTime;
        characterData.characterOxygenData.UpdateFallOff(characterData.elapsedTime);
        Debug.Log (characterData.characterOxygenData.oxygenData.fallOfRate);
    }

    public void HandleInteractable(out CharacterState updatedState)
    {
        updatedState = null;

        if (characterData.movement.interactable != null
        && CharacterManager.customInputMaps.InGame.Action.triggered)
        {   
            //Check if there a Player Action Type
            if (characterData.movement.interactable.TryGetComponent(out PlayerActionType playerActionType))
            {
            
                switch (playerActionType)
                {
                    default:
                        break;
                    case Crawl:
                        updatedState = new CrawlState(characterData);
                        break;
                    case JumpOver:
                        updatedState = new JumpOverState(characterData);
                        break;
                    case MoveBox:
                        updatedState = new MoveObjectState(characterData);
                      break;
                    case CutsceneTrigger:
                        var cutsceneTrigger = playerActionType as CutsceneTrigger;
                        updatedState = new WalkTowards(characterData,cutsceneTrigger.GetCutsceneHandler());
                    break;
                }
            }

            //Interact with Object without switching state
            else
            {
                Movement movement = characterData.movement;
                if (movement.interactable.TryGetComponent(out TriggerByCharacter triggerByCharacter))
                {
                    triggerByCharacter.Activate(movement);
                    movement.interactable = null;
                }
            }
        }
    }

    public abstract CharacterState SpecificStateUpdate(); //Specifically for a certain state designed actions

    protected CharacterState SwitchState(CharacterState updatedState)
    {
        return updatedState;
    }

}