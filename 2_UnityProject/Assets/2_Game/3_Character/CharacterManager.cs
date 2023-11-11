using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore;

public class CharacterData
{

    public CharacterData(GameObject obj)
    {
        gameObject = obj;
        movement = gameObject.GetComponent<Movement>();
    }

    public GameObject gameObject;
    public Movement movement;
    public CharacterState currentState;
    public CinemachineVirtualCamera virtualCamera;
    public Interactable interactable;
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

public class CharacterManager : MonoBehaviour
{
    //Inputs
    public static CustomInputs customInputMaps;

    //State Machine
    private static CharacterData[] characterDatas;
    private static int characterIndex = 0;

    //Character Prefab
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private GameObject manPrefab, womanPrefab;
    [SerializeField] private CharacterData manData, womanData;
    [SerializeField] private GameObject cameraPrefab;

    [SerializeField] TextMeshProUGUI debuggingDump;

    private void Start()
    {
        //Set Up Inputs
        customInputMaps = new CustomInputs();
        SwitchControlScheme(customInputMaps.InGame);

        //Set Up StateMachines
        //GameObject[] characters = GetOrSpawnCharacters();
        //characterDatas = SetUpCharacters(characters);

        //Spawn Characters
        SpawnCharacters();

        //Set up Camera
        CamManager.SetCamPrefab(cameraPrefab);
    }

    private void Update()
    {
        //characterDatas[characterIndex].currentState = characterDatas[characterIndex].currentState.UpdateState();
        //for (int i = 0; i < characterDatas.Length; i++)
       // {
        //    characterDatas[i].currentState = characterDatas[i].currentState.UpdateState();
        //}

        manData.currentState = manData.currentState.UpdateState();
        womanData.currentState = womanData.currentState.UpdateState();

        debuggingDump.text = "Woman: "+womanData.currentState.GetType() +"\n Man: "+ manData.currentState.GetType();
    }

    GameObject GetActiveCharacter()
    {
        return characterDatas[characterIndex].gameObject;
    }


    #region Setup

    void SpawnCharacters()
    {
        GameObject spawnedMan = Instantiate(manPrefab,Vector3.forward,Quaternion.identity);
        spawnedMan.name = "SpawnedMan";
        GameObject spawnedWoman = Instantiate(womanPrefab,Vector3.forward*2,Quaternion.identity);
        spawnedMan.name = "SpawnedWoman";
        womanData = new WomanData(spawnedWoman);
        manData = new ManData(spawnedMan);

        CamManager.SetCamPrefab(cameraPrefab);
        CamManager.SpawnCamera(womanData.gameObject.transform, out womanData.virtualCamera);
        CamManager.SpawnCamera(manData.gameObject.transform, out manData.virtualCamera);

        womanData.currentState = new SetUpState(womanData);
        manData.currentState = new SetUpState(manData);
    }
    private CharacterData[] SetUpCharacters(GameObject[] characters)
    {
        if (characters.Length < 2)
        {
            GameObject[] temp = new GameObject[2];
            for (int i = 0; i < characters.Length; i++)
            {
                temp[i] = characters[i];
            }

            for (int i = characters.Length; i < 2; i++)
            {
                temp[i] = SetUpCharacter(Instantiate(characterPrefab,Vector3.forward * i, Quaternion.identity));
                temp[i].name = "SpawnedCharacter_"+i;
            }

            characters = temp;
        }

        CharacterData[] datas = new CharacterData[characters.Length];
        for (int i = 0; i < datas.Length; i++)
        {
            GameObject obj = SetUpCharacter(characters[i]);
            datas[i] = new CharacterData(obj);
            datas[i].currentState = new SetUpState(datas[i]);
        }

        return datas;
    }

    private GameObject SetUpCharacter(GameObject obj)
    {
        CharacterController controller = obj.GetComponent<CharacterController>();
        if (!controller)
        {
            obj.AddComponent<CharacterController>();
        }

        Movement movement = obj.GetComponent<Movement>();
        if (movement)
        {
            obj.AddComponent<Movement>();
        }

        return obj;
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
        return SpecificStateUpdate();
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
        characterData.gameObject.GetComponent<Animator>().SetBool("Grounded",true);
        characterData.gameObject.GetComponent<Animator>().SetFloat("Speed",0);

        if (characterData.virtualCamera!=null)
            characterData.virtualCamera.gameObject.SetActive(false);
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
        {
            if (characterData.virtualCamera==null)
                CamManager.SpawnCamera(characterData.gameObject.transform, out characterData.virtualCamera);
            else
                characterData.virtualCamera.gameObject.SetActive(true);

            return new IdleState(characterData);
        }

        return this;
    }
}

class IdleState : CharacterState
{
    public IdleState(CharacterData characterData) : base(characterData)
    {
        characterData.gameObject.GetComponent<Animator>().SetBool("Grounded",true);
        characterData.gameObject.GetComponent<Animator>().SetFloat("Speed",0);
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
        characterData.gameObject.GetComponent<Animator>().SetFloat("MotionSpeed",2);
        characterData.gameObject.GetComponent<Animator>().SetFloat("Speed",4);
    }

    public override CharacterState SpecificStateUpdate()
    {
         if (CharacterManager.customInputMaps.InGame.Switch.triggered)
            return new AIState(characterData);

        if (characterData.movement.interactable !=null && CharacterManager.customInputMaps.InGame.Action.triggered)
            characterData.movement.interactable.Trigger();

        Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
        if (inputVector.magnitude <= 0)
            return SwitchState(new IdleState(characterData));

        characterData.movement.MovePlayer(inputVector);

        return this;

    }
}