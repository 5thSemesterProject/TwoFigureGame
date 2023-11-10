using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
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

public class CharacterManager : MonoBehaviour
{
    //Inputs
    public static CustomInputs customInputMaps;

    //State Maschine
    private static CharacterData[] characterDatas;
    private static int characterIndex = 0;

    //Character Prefab
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private GameObject cameraPrefab;

    private void Start()
    {
        //Set Up Inputs
        customInputMaps = new CustomInputs();
        SwitchControlScheme(customInputMaps.InGame);

        //Set Up StateMachines
        GameObject[] characters = GetOrSpawnCharacters();
        characterDatas = SetUpCharacters(characters);

        //Setu up Camera
        CamManager.SetCamPrefab(cameraPrefab);
        SwitchCharacters();
    }

    private void Update()
    {
        characterDatas[characterIndex].currentState = characterDatas[characterIndex].currentState.UpdateState();
    }

    GameObject GetActiveCharacter()
    {
        return characterDatas[characterIndex].gameObject;
    }

    public static void SwitchCharacters()
    {   
        //Delete old Camera
        if (characterDatas[characterIndex].virtualCamera!=null)
        {
            characterDatas[characterIndex].virtualCamera.gameObject.SetActive(false);
            characterDatas[characterIndex].virtualCamera = null;
        }

        characterDatas[characterIndex].currentState = new AIState(characterDatas[characterIndex].currentState.characterData);
        characterIndex = (characterIndex+1) % characterDatas.Length;
        
        //Set up new Camera
        if (characterDatas[characterIndex].virtualCamera==null)
            CamManager.SpawnCamera(characterDatas[characterIndex].gameObject.transform, out characterDatas[characterIndex].virtualCamera);
    }


    #region Setup
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
                temp[i] = EnsureCharacter(Instantiate(characterPrefab,Vector3.forward * i, Quaternion.identity));
            }

            characters = temp;
        }

        CharacterData[] datas = new CharacterData[characters.Length];
        for (int i = 0; i < datas.Length; i++)
        {
            GameObject obj = EnsureCharacter(characters[i]);
            datas[i] = new CharacterData(obj);
            datas[i].currentState = new SetUpState(datas[i]);
        }

        return datas;
    }

    private GameObject EnsureCharacter(GameObject obj)
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
        {
            return new GameObject[0];
        }
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
                actionMaps.Disable();
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
        CharacterManager.customInputMaps.InGame.Action.performed += SwitchCharacters;
    }

    public CharacterData characterData;
    public abstract CharacterState UpdateState();

    protected void RemoveCharacterSwitch()
    {
        CharacterManager.customInputMaps.InGame.Action.performed -= SwitchCharacters;
    }

    protected virtual void SwitchCharacters(InputAction.CallbackContext context)
    {
        ExitState();
        CharacterManager.SwitchCharacters();
    }

    protected virtual void ExitState()
    {
        RemoveCharacterSwitch();
    }
    
}

class SetUpState : CharacterState
{
    public SetUpState(CharacterData characterData) : base(characterData) { }

    public override CharacterState UpdateState()
    {
        return new IdleState(characterData);
    }
}

class AIState : CharacterState
{
    public AIState(CharacterData data) : base(data)
    {
        RemoveCharacterSwitch();
    }

    public override CharacterState UpdateState()
    {
        return new IdleState(characterData);
    }
}

class IdleState : CharacterState
{
    public IdleState(CharacterData characterData) : base(characterData)
    {
    
    }

    public override CharacterState UpdateState()
    {
        Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
        if (inputVector.magnitude > 0)
        {
            ExitState();
            return new MoveState(characterData);
        }

        characterData.gameObject.GetComponent<Animator>().SetFloat("Speed",0);

        return this;
    }
}

class MoveState : CharacterState
{
    public MoveState(CharacterData data) : base(data)
    {

    }

    public override CharacterState UpdateState()
    {
        Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
        if (inputVector.magnitude <= 0)
        {
            ExitState();
            return new IdleState(characterData);
        }

        characterData.movement.MovePlayer(inputVector);
        characterData.gameObject.GetComponent<Animator>().SetBool("Grounded",true);
        characterData.gameObject.GetComponent<Animator>().SetFloat("MotionSpeed",2);
        characterData.gameObject.GetComponent<Animator>().SetFloat("Speed",4);

        return this;

    }
}