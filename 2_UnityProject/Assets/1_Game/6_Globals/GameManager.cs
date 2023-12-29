using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum EndCondition
{
    Win,
    OxygenMan,
    OxygenWoman,
    Misc,
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private CustomInputs inputMapping;

    //Pause
    public GameObject pauseMenuPrefab;
    private WSUI_Element pauseMenu;
    private bool pause;

    //Game Events
    public event System.Action<InputAction.CallbackContext> gamePause;

    //EndGame
    public delegate void EndGame(EndCondition endCondition);
    public event EndGame gameEnd;
    public GameObject endScreenPrefab;
    public bool hasGameEnded = false;

    private void OnGUI()
    {
        if (GUILayout.Button("End Game"))
        {
            gameEnd?.Invoke(EndCondition.OxygenWoman);
        }
        if (GUILayout.Button("Win Game"))
        {
            gameEnd?.Invoke(EndCondition.Win);
        }
    }

    #region Startup
    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    #endregion

    #region Event Subscbscription
    private void Start()
    {
        UnSubscribeEvents();
        SubscribeEvents();
    }

    public static void SubscribeEvents()
    {
        //Subscribe End Game Events
        instance.gameEnd += EndGameLogic;

        //Subscribe Pause Menu Events
        if (instance.inputMapping == null)
        {
            instance.inputMapping = CustomEventSystem.GetInputMapping;
        }
        instance.gamePause += instance.TogglePause;
        instance.inputMapping.InGame.Pause.performed += instance.gamePause;
    }

    public static void UnSubscribeEvents()
    {
        //UnSubscribe End Game Events
        instance.gameEnd -= EndGameLogic;

        //UnSubscribe Pause Menu Events
        if (instance.inputMapping == null)
        {
            instance.inputMapping = CustomEventSystem.GetInputMapping;
        }
        instance.gamePause -= instance.TogglePause;
        instance.inputMapping.InGame.Pause.performed -= instance.gamePause;
    }
    #endregion

    #region PauseMenu
    public static void TogglePauseManual()
    {
        InputAction.CallbackContext t = new InputAction.CallbackContext();
        instance.TogglePause(t);
    }

    public void TogglePause(InputAction.CallbackContext context)
    {
        pause = !pause;
        if (pause)
        {
            pauseMenu = WSUI.AddOverlay(pauseMenuPrefab);
            Time.timeScale = 0;
            CustomEventSystem.SwitchControlScheme(CustomEventSystem.GetInputMapping.InUI);
        }
        else
        {
            WSUI.RemovePrompt(pauseMenu);
            //pauseMenu.GetComponent<ButtonGroupFade>().ExitAndDestroy();
            Time.timeScale = 1;
            CustomEventSystem.SwitchControlScheme(CustomEventSystem.GetInputMapping.InGame);
        }
    }
    #endregion

    #region EndGame
    private void Update()
    {
        if (CharacterManager.IsGameOver && !hasGameEnded)
        {
            if (CharacterManager.IsWomanDead)
                gameEnd?.Invoke(EndCondition.OxygenWoman);
            else
                gameEnd?.Invoke(EndCondition.OxygenMan);
            hasGameEnded = true;
        }
    }

    public static void EndGameLogic(EndCondition endCondition)
    {
        UnSubscribeEvents();
        instance.hasGameEnded = true;

        GameObject endPrefab = Instantiate(instance.endScreenPrefab);

        EndGameManager endGameManager = endPrefab.GetComponent<EndGameManager>();
        endGameManager.EndGame(endCondition);
    }
    #endregion
}
