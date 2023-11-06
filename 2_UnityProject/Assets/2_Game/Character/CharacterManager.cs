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
}

public class CharacterManager : MonoBehaviour
{
    //Inputs
    public static CustomInputs customInputMaps;

    //State Maschine
    private static CharacterData[] characterDatas;
    private static int count = 0;

    //Character Prefab
    [SerializeField] private GameObject characterPrefab;

    void Start()
    {
        //Set Up Inputs
        customInputMaps = new CustomInputs();
        SwitchControlScheme(customInputMaps.InGame);

        //Set Up StateMachines
        GameObject[] characters = GetOrSpawnCharacters();
        characterDatas = SetUpCharacters(characters);
    }

    void Update()
    {
        characterDatas[count].currentState = characterDatas[count].currentState.UpdateState();
    }

    public static void SwitchCharacters()
    {
        characterDatas[count].currentState = new AIState(characterDatas[count].currentState.characterData);
        characterDatas[count].virtualCamera.gameObject.SetActive(false);
        count = (count+1) % characterDatas.Length;
        characterDatas[count].virtualCamera.gameObject.SetActive(true);
        Debug.Log(count);
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
            datas[i].virtualCamera = datas[i].gameObject.GetComponentInChildren<CinemachineVirtualCamera>();
            datas[i].virtualCamera.gameObject.SetActive(false);
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
                Debug.Log($"Input: {actionMap.name} enabled");
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
    }

    public CharacterData characterData;
    public abstract CharacterState UpdateState();
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
    public AIState(CharacterData data) : base(data) { }

    public override CharacterState UpdateState()
    {
        return new IdleState(characterData);
    }
}

class IdleState : CharacterState
{
    public IdleState(CharacterData characterData) : base(characterData)
    {
        CharacterManager.customInputMaps.InGame.Action.performed += Action;
    }

    public override CharacterState UpdateState()
    {
        Vector2 inputVector = CharacterManager.customInputMaps.InGame.Movement.ReadValue<Vector2>();
        characterData.virtualCamera.gameObject.SetActive(true);
        characterData.movement.MovePlayer(inputVector);

        return this;
    }

    private void Action(InputAction.CallbackContext context)
    {
        ExitState();
        CharacterManager.SwitchCharacters();
    }

    private void ExitState()
    {
        CharacterManager.customInputMaps.InGame.Action.performed -= Action;
    }
}
