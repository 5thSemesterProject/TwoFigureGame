using TMPro;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    //Inputs
    public static CustomInputs customInputMaps;

    public static CharacterData manData, womanData;

    public static bool IsGameOver
    {
        get => (IsManDead || IsWomanDead);
    }
    public static bool IsManDead
    {
        get => manData.characterOxygenData.oxygenData.currentOxygen <= 0;
    }
    public static bool IsWomanDead
    {
        get => womanData.characterOxygenData.oxygenData.currentOxygen <= 0;
    }
    public static bool IsManLow
    {
        get
        {
            return manData.characterOxygenData.oxygenData.IsLow;
        }
    }
    public static bool IsWomanLow
    {
        get
        {
            return womanData.characterOxygenData.oxygenData.IsLow;
        }
    }

    public static float ElapsedTime
    {
        get
        {
            return womanData.elapsedTime;
        }
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

    private void Awake()
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

            debuggingCharacterStateMachines.text = $"<b>Woman</b> " + womanData.currentState.GetType() + "\n <b>Man</b> " + manData.currentState.GetType();
            
            //Oxygen Debugging View
            debuggingOxygenCharacters.text = 
            $"<b>Woman</b> \n Current: {womanData.characterOxygenData.oxygenData.currentOxygen} \n Falloff Rate: {womanData.characterOxygenData.oxygenData.fallOfRate}\n" + 
            $"<b>Man</b> \n Current: {manData.characterOxygenData.oxygenData.currentOxygen} \n Falloff Rate: {manData.characterOxygenData.oxygenData.fallOfRate}";
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
        manData.elapsedTime = 0;
        womanData.elapsedTime = 0;

        //Oxygen Setup
        womanData.characterOxygenData = GameStats.instance.characterOxy;
        manData.characterOxygenData = GameStats.instance.characterOxy;

        //Cam Setup
        CamManager.SetCamPrefab(cameraPrefab);
        CamManager.SpawnCamera(womanData.gameObject.transform, out womanData.virtualCamera);
        CamManager.SpawnCamera(manData.gameObject.transform, out manData.virtualCamera);


        //Statemachine Setup
        womanData.currentState = new SetUpState(womanData);
        manData.currentState = new SetUpState(manData);
    }
    #endregion
}