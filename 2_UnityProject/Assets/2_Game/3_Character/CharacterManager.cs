using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class OxygenData
{
    public float maxOxygen;
    public float currentOxygen;
    public float fallOfRate;

    public OxygenData(float maxOxygen,float fallOfRate)
    {
        this.maxOxygen = maxOxygen;
        currentOxygen = maxOxygen;
        this.fallOfRate = fallOfRate;
    }

    public void FallOff()
    {
        currentOxygen -=fallOfRate*Time.deltaTime;
    }

    public void InreaseOxygen(float amount)
    {
        if (currentOxygen<=maxOxygen)
            currentOxygen+=amount;
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
}

public class WomanData: CharacterData
{
    public WomanData(GameObject obj):base(obj)
    {
    }
}

public class ManData: CharacterData
{
    public ManData(GameObject obj):base(obj)
    {
    }
}

public enum Characters
{
    Man,
    Woman
}

public class CharacterManager : MonoBehaviour
{
    //Inputs
    public static CustomInputs customInputMaps;

    //State Machine
    private static CharacterData[] characterDatas;
    private static int characterIndex = 0;

    //Character Prefab
    [SerializeField] private GameObject manPrefab, womanPrefab;
    [SerializeField] private static CharacterData manData, womanData;
    [SerializeField] private GameObject cameraPrefab;

    [Header ("Debugging")]
    [SerializeField] TextMeshProUGUI debuggingCharacterStateMachines;
    [SerializeField] TextMeshProUGUI debuggingOxygenCharacters;

    //Active Character
    public static GameObject ActiveCharacterRigidbody
    {
        get
        {
            if (manData.currentState.GetType() == typeof(AIState))
            {
                return womanData.roomFadeRigidBody;
            }
            return manData.roomFadeRigidBody;
        }
    }

    private void Start()
    {
        //Set Up Inputs
        customInputMaps = new CustomInputs();
        SwitchControlScheme(customInputMaps.InGame);

        //Spawn Characters
        SpawnCharacters();

        //Set up Camera
        CamManager.SetCamPrefab(cameraPrefab);
    }

    private void Update()
    {
        manData.currentState = manData.currentState.UpdateState();
        womanData.currentState = womanData.currentState.UpdateState();

        debuggingCharacterStateMachines.text = "Woman: "+womanData.currentState.GetType() +"\n Man: "+ manData.currentState.GetType();
        debuggingOxygenCharacters.text = "WomanOxy: "+womanData.oxygenData.currentOxygen+ "\n ManOxy: "+manData.oxygenData.currentOxygen;
    }

    GameObject GetActiveCharacter()
    {
        return characterDatas[characterIndex].gameObject;
    }


    #region Setup

    void SpawnCharacters()
    {
        //Prefab Setup
        GameObject spawnedMan = Instantiate(manPrefab,Vector3.forward,Quaternion.identity);
        spawnedMan.name = "SpawnedMan";
        GameObject spawnedWoman = Instantiate(womanPrefab,Vector3.forward*2,Quaternion.identity);
        spawnedMan.name = "SpawnedWoman";
        womanData = new WomanData(spawnedWoman);
        womanData.movement.characterType = CharacterType.Woman;
        manData = new ManData(spawnedMan);
        manData.movement.characterType = CharacterType.Man;

        //Oxygen Setup
        womanData.oxygenData = new OxygenData(100,1f);
        manData.oxygenData = new OxygenData(100,1f);

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

    #region Input
    public static void SwitchControlScheme(InputActionMap actionMap)
    {
        var customInputMaps = CharacterManager.customInputMaps.asset.actionMaps;
        for (int i = 0; i < customInputMaps.Count; i++)
        {
            if (customInputMaps[i].name == actionMap.name)
            {
                DisableControlSchemes();
                actionMap.Enable();
            }
        }
    }

    public static void DisableControlSchemes()
    {
        foreach (var actionMaps in customInputMaps.asset.actionMaps)
        {
            if (actionMaps.enabled)
            {
                //actionMaps.Disable();
            }
        }
    }
    #endregion
}

public abstract class CharacterState
{
    public CharacterState(CharacterData data)
    {
        characterData = data;
    }

    public CharacterData characterData;
    public CharacterState UpdateState() //Update Method that every State checks everytime
    {   
        if (!(characterData.currentState is AIState))
            CamManager.FindOccludingObjects(characterData.gameObject.transform);

        HandleOxygen();

        HandleInteractable(out CharacterState interactableState);
        if (interactableState!=null)
            return interactableState;

        return SpecificStateUpdate();
    }

    public void HandleOxygen()
    {   
        Collider[] hitColliders = Physics.OverlapBox(characterData.gameObject.transform.position, Vector3.one/2);
        
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].TryGetComponent(out VolumetricFogHandler volumetricFogHandler))
            {
                characterData.oxygenData.FallOff();
            }
            else if (hitColliders[i].TryGetComponent(out Oxygenstation oxygenstation) 
                    &&characterData.oxygenData.currentOxygen<=characterData.oxygenData.maxOxygen )
            {   
                    characterData.oxygenData.currentOxygen+=oxygenstation.ChargePlayer();
            }
        }      
    }

    public void HandleInteractable(out CharacterState updatedState)
    {
        updatedState = null;

        if (characterData.movement.interactable !=null
        && CharacterManager.customInputMaps.InGame.Action.triggered)
        {
            switch(characterData.movement.interactable)
            {
                default:
                    characterData.movement.interactable.TriggerByPlayer();
                    characterData.movement.interactable = null;
                break;     
                case Crawl:
                    updatedState = new CrawlState(characterData);
                break;
            }
        }

        
    }

    public abstract CharacterState SpecificStateUpdate(); //Specifically for a certain state designed actions

    protected  CharacterState SwitchState(CharacterState updatedState)
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
        if (characterData.virtualCamera!=null)
            characterData.virtualCamera.gameObject.SetActive(false);

        characterData.movement.MovePlayer(Vector2.zero, 0);
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
        {
            if (characterData.virtualCamera==null)
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
        characterData.movement.MovePlayer(Vector2.zero,0);
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
            return new AIState(characterData);

        Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
        if (inputVector.magnitude > 0)
            return SwitchState(new MoveState(characterData));

        
        if (characterData.movement.interactable !=null && CharacterManager.customInputMaps.InGame.Action.triggered)
            characterData.movement.interactable.TriggerByPlayer();

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

        if (characterData.movement.interactable !=null && CharacterManager.customInputMaps.InGame.Action.triggered)
            characterData.movement.interactable.Trigger();

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
        characterData.movement.StartCrawl(characterData.movement.interactable,2);
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
            return new AIState(characterData);

        if (characterData.movement.coroutine==null)
            return new IdleState(characterData);

        return this;

    }
}

