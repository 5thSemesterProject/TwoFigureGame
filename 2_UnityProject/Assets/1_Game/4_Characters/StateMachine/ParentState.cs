using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public abstract class CharacterState
{
    protected bool handleInteractables = true;
    protected bool updateLastState = true;
    protected bool activateOxygenBar = true;
    protected bool handleOxygen = true;
    protected bool enableCheats = true;

    protected int cheatIndex = 0;
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

        #if UNITY_EDITOR
        if (enableCheats && !(characterData.currentState is AIState))
        {
            CharacterState godMode = HandleCheats();

            if (godMode!=null)
                return godMode;
        }
        #endif
           
        return SpecificStateUpdate();
    }

    #region OxygenHandling
    #region  OxygenBar
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
    #endregion


    public void HandleOxygen()
    {   
        //OxygenBar
        if (activateOxygenBar)
            HandleOxygenBar();
        else
            HideOxygenBar();

        //Oxygenstation Charge And Uncharge
        Oxygenstation oxygenstation  = characterData.movement.oxygenstation;
        
        if (oxygenstation!=null && !CheckForOccludingWalls(oxygenstation.transform.position))
            ChargeCharacter(oxygenstation);
        else
        {
            UnchargeCharacter();
            oxygenstation = null;
        }

        //Check For Death
        if (characterData.characterOxygenData.oxygenData.currentOxygen <= 0)
        {
            characterData.animator.SetBool("Dead", true);
        }

        //Increase FallOff With Time
        characterData.elapsedTime +=Time.deltaTime;
        characterData.characterOxygenData.UpdateFallOff(characterData.elapsedTime);
    }

    bool CheckForOccludingWalls(Vector3 targetToCheckPos)
    {
        //Check For Occluding Walls
        Vector3 playerPos = characterData.movement.transform.position+Vector3.up;
        Vector3 rayDir = playerPos-targetToCheckPos;
        Ray ray = new Ray(targetToCheckPos,rayDir);
        LayerMask allLayersMask = int.MaxValue;
        RaycastHit hitInfo;
        
        if (Physics.Raycast(targetToCheckPos,rayDir, out hitInfo, Mathf.Infinity,allLayersMask,QueryTriggerInteraction.Ignore))
        {
            Movement  movementComp=null;
            Transform parent = hitInfo.transform.parent;
            if (parent!=null)
                movementComp = parent.GetComponent<Movement>();

            if (movementComp!=null)
                    return false;
        }

        return true;
    }

    void ChargeCharacter(Oxygenstation oxygenstation)
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

    void UnchargeCharacter()
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
    #endregion

    #region  Interactables
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
                        updatedState = HandleCutsceneTrigger(cutsceneTrigger);

                        //In case character can't reach
                        if (updatedState==null)
                        {   
                            characterData.movement.interactable = null;
                            return;
                        }
                            
                    break;
                }
            }

            //Interact with Object without switching state
            Movement movement = characterData.movement;
            if (movement.interactable.TryGetComponent(out TriggerByCharacter triggerByCharacter))
            {
                Debug.Log ("Activate");
                triggerByCharacter.Activate(movement);
                movement.interactable = null;
            }
        }
    }

    private CharacterState HandleCutsceneTrigger(CutsceneTrigger cutsceneTrigger)
    {
         CutsceneHandler cutsceneHandler = cutsceneTrigger.cutscene.cutsceneHandler;
                        
            //In case Door
            if (cutsceneTrigger.TryGetComponent(out Door door))
            {   

                //In case other character cant reach
                if (characterData.other.navMeshHandler.CheckReachable(characterData.other.gameObject.transform.position))
                    return new WaitForDoor(characterData,cutsceneHandler,door);
            }
            else
            {   
                //In case actor Positions reachable
                Vector3 actorPos = cutsceneHandler.GetActorData(characterData.CharacterType).actor.transform.position;
                Vector3 otherActor = cutsceneHandler.GetActorData(characterData.other.CharacterType).actor.transform.position;
                if (characterData.navMeshHandler.CheckReachable(actorPos) 
                && characterData.other.navMeshHandler.CheckReachable(otherActor))
                    return new WalkTowards(characterData,cutsceneHandler);
            }

            //Play not reachable sound
            CharacterType characterType = characterData.CharacterType;
            EVoicelines voicelineToPlay = characterType == CharacterType.Woman?EVoicelines.CanNotPass_E:EVoicelines.CanNotPass_V;
            SoundSystem.Play(voicelineToPlay,characterData.gameObject.transform,SoundPriority.High,false,1);

            return null;
    } 
    #endregion

    protected CharacterState HandleCheats()
    {
        string cheatCode = "cheat";

        if (Input.anyKeyDown) 
        {
            if (Input.GetKeyDown(cheatCode[cheatIndex].ToString())) 
                cheatIndex++;
            else 
                cheatIndex = 0;	
        }

        if (cheatIndex >=cheatCode.Length-1)
        {
            cheatIndex = 0;
            return new GodModeState(characterData);
        }

        return null;
    }

    public abstract CharacterState SpecificStateUpdate(); //Specifically for a certain state designed actions

    protected CharacterState SwitchState(CharacterState updatedState)
    {
        return updatedState;
    }



    

}