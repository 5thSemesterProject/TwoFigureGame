using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

#region Data
public class OxygenData
{
    public float maxOxygen;
    public float currentOxygen;
    public float fallOfRate;

    public OxygenData(float maxOxygen, float fallOfRate)
    {
        this.maxOxygen = maxOxygen;
        currentOxygen = maxOxygen;
        this.fallOfRate = fallOfRate;
    }

    public void FallOff()
    {
        currentOxygen -= fallOfRate * Time.deltaTime;
    }

    public void InreaseOxygen(float amount)
    {
        if (currentOxygen <= maxOxygen)
            currentOxygen += amount;
    }
}

public class CharacterData
{
    public CharacterData(GameObject obj)
    {
        gameObject = obj;
        movement = gameObject.GetComponent<Movement>();
        animator = gameObject.GetComponent<Animator>();

        var rigidbodysOnCharacter = gameObject.GetComponentsInChildren<Rigidbody>();
        foreach (var rigidbody in rigidbodysOnCharacter)
        {
            if (rigidbody.gameObject.layer == 9)
            {
                roomFadeRigidBody = rigidbody.gameObject;
            }
        }
    }

    public GameObject gameObject;
    public Movement movement;
    public Animator animator;
    public CharacterState currentState;
    public CinemachineVirtualCamera virtualCamera;
    public OxygenData oxygenData;
    public GameObject roomFadeRigidBody;
    public CharacterData other;
    public CharacterState lastState;

}

public class WomanData : CharacterData
{
    public WomanData(GameObject obj) : base(obj)
    {
    }
}

public class ManData : CharacterData
{
    public ManData(GameObject obj) : base(obj)
    {
    }
}

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
        manData.currentState = manData.currentState.UpdateState();
        womanData.currentState = womanData.currentState.UpdateState();

        debuggingCharacterStateMachines.text = "Woman: " + womanData.currentState.GetType() + "\n Man: " + manData.currentState.GetType();
        debuggingOxygenCharacters.text = "WomanOxy: " + womanData.oxygenData.currentOxygen + "\n ManOxy: " + manData.oxygenData.currentOxygen;
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
        womanData.oxygenData = new OxygenData(100, 1f);
        manData.oxygenData = new OxygenData(100, 1f);

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

public abstract class CharacterState
{
    protected bool handleInteractables = true;
    protected bool updateLastState = true;
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

        HandleOxygen();

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
            return new WalkTowards(characterData,walkTowards.GetCutsceneTrigger());
        }
           
        return SpecificStateUpdate();
    }

    public void HandleOxygen()
    {   
        Oxygenstation oxygenstation  = characterData.movement.oxygenstation;
        if (oxygenstation!=null)
        {
            if (characterData.oxygenData.currentOxygen<=characterData.oxygenData.maxOxygen)
                characterData.oxygenData.currentOxygen+=oxygenstation.ChargePlayer();
        }
        else
        {
            characterData.oxygenData.FallOff();
        }    
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
                        updatedState = new WalkTowards(characterData,cutsceneTrigger);
                    break;
                }
            }

            ////Interact with Object without switching state
            else
            {
                Movement movement = characterData.movement;
                if (movement.interactable.TryGetComponent(out TriggerByCharacter triggerByCharacter))
                {
                    triggerByCharacter.Activate(movement);
                    characterData.movement.interactable = null;
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

            return new IdleState(characterData);
        }

        return this;
    }
}

class IdleState : CharacterState
{
    public IdleState(CharacterData characterData) : base(characterData)
    {
        characterData.movement.MovePlayer(Vector2.zero, 0);
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


