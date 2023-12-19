using System;
using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;

#region Data

public enum Characters
{
    Man,
    Woman
}
#endregion

public class CharacterManager : MonoBehaviour
{
    //Inputs
    public static CustomInputs customInputMaps;

    static CharacterData manData, womanData;

    public static bool IsGameOver
    {
        get => (IsManDead || IsWomanDead);
    }
    public static bool IsManDead
    {
        get => manData.oxygenData.currentOxygen <= 0;
    }
    public static bool IsWomanDead
    {
        get => womanData.oxygenData.currentOxygen <= 0;
    }
    
    //Character Prefab

    [Header("Prefabs")]
    [SerializeField] private GameObject manPrefab;
    [SerializeField] private GameObject womanPrefab;
    [SerializeField]private GameObject cameraPrefab;

    [Header ("Other")]
    [SerializeField] Transform spawnPointMan;
    [SerializeField] Transform spawnPointWoman;


    [Header("Debugging")]
    [SerializeField] TextMeshProUGUI debuggingCharacterStateMachines;
    [SerializeField] TextMeshProUGUI debuggingOxygenCharacters;



    //Active Character
    public static GameObject ActiveCharacterRigidbody
    {
        get
        {
            if (CheckAIState(manData))
            {
                return womanData.roomFadeRigidBody;
            }
            return manData.roomFadeRigidBody;
        }
    }

    
    public static CharacterData ActiveCharacterData
    {
        get
        {
            if (CheckAIState(manData))
            {
                return womanData;
            }
            return manData;
        }
    }

    private void Start()
    {
        //Set Up Inputs
        customInputMaps = CustomEventSystem.GetInputMapping;
        CustomEventSystem.SwitchControlScheme(customInputMaps.InGame);

        //Spawn Characters
        SpawnCharacters();

        //Set up Camera
        CamManager.SetCamPrefab(cameraPrefab);
    }

    static bool CheckAIState(CharacterData characterData)
    {
        return characterData.currentState.GetType() == typeof(AIState);
    }


    private void Update()
    {
        if (Time.timeScale>=0)
        {
            manData.currentState = manData.currentState.UpdateState();
            womanData.currentState = womanData.currentState.UpdateState();

            debuggingCharacterStateMachines.text = "Woman: " + womanData.currentState.GetType() + "\n Man: " + manData.currentState.GetType();
            debuggingOxygenCharacters.text = "WomanOxy: " + womanData.oxygenData.currentOxygen + "\n ManOxy: " + manData.oxygenData.currentOxygen;
        }
    }


    #region Setup

    void SpawnCharacters()
    {
        //Prefab Setup
        GameObject spawnedMan = Instantiate(manPrefab, spawnPointMan?spawnPointMan.position:Vector3.forward, Quaternion.identity);
        spawnedMan.name = "SpawnedMan";
        GameObject spawnedWoman = Instantiate(womanPrefab, spawnPointWoman?spawnPointWoman.position:Vector3.forward*2, Quaternion.identity);
        spawnedWoman.name = "SpawnedWoman";
        womanData = new WomanData(spawnedWoman);
        womanData.movement.characterType = CharacterType.Woman;
        manData = new ManData(spawnedMan);
        manData.movement.characterType = CharacterType.Man;
        manData.other = womanData;
        womanData.other   = manData;

        //Oxygen Setup
        womanData.oxygenData = GameStats.instance.characterOxy;
        manData.oxygenData = GameStats.instance.characterOxy;

        //Cam Setup
        CamManager.SetCamPrefab(cameraPrefab);
        CamManager.SpawnCamera(womanData.gameObject.transform, out womanData.virtualCamera);
        CamManager.SpawnCamera(manData.gameObject.transform, out manData.virtualCamera);


        //Statemachine Setup
        womanData.currentState = new SetUpState(womanData);
        manData.currentState = new SetUpState(manData);
    }


    private GameObject[] GetOrSpawnCharacters()
    {
        GameObject[] characters = GameObject.FindGameObjectsWithTag("Character");
        if (characters == null)
            return new GameObject[0];
        return characters;
    }
    #endregion
}


class SetUpState : CharacterState
{
    public SetUpState(CharacterData characterData) : base(characterData) { }

    public override CharacterState SpecificStateUpdate()
    {
        if (characterData is ManData)
            return new AIState(characterData);
        return new IdleState(characterData);
    }
}

class AIState : CharacterState
{
    public AIState(CharacterData data) : base(data)
    {
        if (characterData.virtualCamera != null)
            characterData.virtualCamera.gameObject.SetActive(false);

        characterData.movement.MovePlayer(Vector2.zero, 0);

        handleInteractables = false;
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
        {
            if (characterData.virtualCamera == null)
                CamManager.SpawnCamera(characterData.gameObject.transform, out characterData.virtualCamera);
            else
                characterData.virtualCamera.gameObject.SetActive(true);

            CustomEvents.RaiseCharacterSwitch(characterData.roomFadeRigidBody);

            characterData.movement.DisableNavMesh();
            return new IdleState(characterData);
        }

        //Follow Partner
        FollowPartner();


        return this;
    }

    void FollowPartner()
    {   
        Vector3 otherCharacterPos = characterData.other.gameObject.transform.position;
        Vector3 pos = characterData.gameObject.transform.position;
        if (characterData.movement.GetPossiblePath(otherCharacterPos) && Vector3.Distance(otherCharacterPos,pos)>GameStats.instance.inactiveFollowDistance)
        {
            characterData.movement.MovePlayerToPos(otherCharacterPos);
        }
            
        else
        {
            characterData.movement.DisableNavMesh();
            characterData.movement.EnableIdleAnim();
        }
            

    }
}

class IdleState : CharacterState
{
    public IdleState(CharacterData characterData) : base(characterData)
    {
       // characterData.movement.MovePlayer(Vector2.zero, 0);
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
            return new AIState(characterData);

        Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
        if (inputVector.magnitude > 0)
            return SwitchState(new MoveState(characterData));


        if (characterData.movement.interactable != null && CharacterManager.customInputMaps.InGame.Action.triggered)
            //characterData.movement.interactable.TriggerByPlayer();
            Debug.Log ("");

        return this;
    }
}

class MoveState : CharacterState
{
    public MoveState(CharacterData data) : base(data)
    {

    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
            return new AIState(characterData);

        if (characterData.movement.interactable != null && CharacterManager.customInputMaps.InGame.Action.triggered)
            characterData.movement.interactable.Trigger(characterData.movement);

        Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
        Vector2 MovementVector = characterData.movement.MovePlayer(inputVector);
        if (MovementVector.magnitude <= 0)
            return SwitchState(new IdleState(characterData));


        return this;

    }
}

class CrawlState : CharacterState
{
    public CrawlState(CharacterData data) : base(data)
    {
        characterData.movement.StartTraversing(characterData.movement.interactable, TraversalType.Crawl, 2);
        handleInteractables = false;
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
            return new AIState(characterData);

        if (characterData.movement.coroutine == null)
            return new IdleState(characterData);

        return this;

    }
}

class JumpOverState : CharacterState
{
    public JumpOverState(CharacterData data) : base(data)
    {
        characterData.movement.StartTraversing(characterData.movement.interactable, TraversalType.JumpOver, 2);
        handleInteractables = false;
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
            return new AIState(characterData);

        if (characterData.movement.coroutine == null)
            return new IdleState(characterData);

        return this;

    }
}

class MoveObjectState : CharacterState
{
    private MoveBox movableObject;
    private Transform previousParent;

    public MoveObjectState(CharacterData data) : base(data)
    {
        //Disable ability to interact with other objects
        handleInteractables = false;

        //Get the NoveObject Script if it exists
        if (data.movement.interactable.GetComponent<MoveBox>() != null)
        {
            movableObject = data.movement.interactable.GetComponent<MoveBox>();
            Debug.Log(movableObject);
        }
        else
        {
            Debug.LogWarning("Interactable activated MoveObject State but is not MoveObject!");
        }

        //Parent Character To Box
        previousParent = characterData.gameObject.transform.parent;
        characterData.gameObject.transform.parent = movableObject.transform;

        //Lerp Player to handle
        characterData.movement.LerpPlayerTo(movableObject.playerHandlePosition, true, 0.2f);
    }

    public override CharacterState SpecificStateUpdate()
    {
        //Enter AI State if triggered
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
        {
            //Unparent Character
            characterData.gameObject.transform.parent = previousParent;

            return new AIState(characterData);
        }

        //Return to idle if player released the input
        if (CharacterManager.customInputMaps.InGame.Action.phase == InputActionPhase.Waiting)
        {
            //Unparent Character
            characterData.gameObject.transform.parent = previousParent;

            //Stop lerp to handle
            characterData.movement.StopLerp();

            return new IdleState(characterData);
        }

        Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
        Vector2 gameWorldVector = VectorHelper.Convert3To2(Camera.main.transform.forward).normalized * inputVector.y + VectorHelper.Convert3To2(Camera.main.transform.right).normalized * inputVector.x;
        float moveDir = movableObject.MoveWithObject(gameWorldVector);
        switch (moveDir)
        {
            case -1:
                //Trigger Backwards Movement
                break;
            case 0:
                //Trigger Idle
                break;
            case 1:
                //Trigger Forward Movement
                break;
            default:
                Debug.LogWarning("Something went wrong!");
                break;
        }

        return this;
    }
 
}


