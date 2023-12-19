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

    public delegate void EndGame(EndCondition endCondition);
    public event EndGame gameEnd;
    public bool hasGameEnded = false;

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
        Debug.Log(pause);
        if (pause)
        {
            pauseMenu = WSUI.AddOverlay(pauseMenuPrefab);
            Time.timeScale = 0;
            CustomEventSystem.SwitchControlScheme(CustomEventSystem.GetInputMapping.InUI);
        }
        else
        {
            WSUI.RemovePrompt(pauseMenu);
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
                gameEnd.Invoke(EndCondition.OxygenWoman);
            else
                gameEnd.Invoke(EndCondition.OxygenMan);
            hasGameEnded = true;
        }
    }

    public static void EndGameLogic(EndCondition endCondition)
    {
        instance.StartCoroutine(_EndGame(endCondition));
    }

    private static IEnumerator _EndGame(EndCondition endCondition)
    {
        UnSubscribeEvents();

        switch (endCondition)
        {
            case EndCondition.Win:
                Debug.Log("YOU WIN!");
                yield return new WaitForSecondsRealtime(2);
                SceneManager.LoadScene("MainMenu");
                yield break;
            case EndCondition.OxygenMan:
                Debug.Log("Man Died!");
                break;
            case EndCondition.OxygenWoman:
                Debug.Log("Woman Died!");
                break;
            case EndCondition.Misc:
            default:
                Debug.Log("The game designers have no clue why you died... sorry.");
                break;
        }

        yield return new WaitForSecondsRealtime(2);
        SceneManager.LoadScene("LevelScene");

        yield return null;
    }
    #endregion
}
