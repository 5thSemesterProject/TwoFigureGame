using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum EndCondition
{
    Win,
    Oxygen,
    Misc,
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private Canvas canvas;
    private CustomInputs inputMapping;

    public GameObject pauseMenuPrefab;
    private WSUI_Element pauseMenu;
    private bool pause;

    //Game Events
    public event System.Action<InputAction.CallbackContext> gamePause;

    public delegate void EndGame(EndCondition endCondition);
    public event EndGame gameEnd;

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

    //private void Awake()
    //{
    //    DontDestroyOnLoad(this.gameObject);
    //}
    #endregion

    private void Start()
    {
        //Subscribe End Game Events
        gameEnd += EndGameLogic;

        //Subscribe Pause Menu Events
        inputMapping = CustomEventSystem.GetInputMapping;
        gamePause += TogglePause;
        inputMapping.InGame.Pause.performed += gamePause;
        inputMapping.InUI.Escape.performed += gamePause;
    }

    #region PauseMenu
    public void TogglePause(InputAction.CallbackContext context)
    {

        Debug.Log("HY");
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
            Time.timeScale = 1;
            CustomEventSystem.SwitchControlScheme(CustomEventSystem.GetInputMapping.InGame);
        }
    }
    #endregion

    #region EndGame
    public static void EndGameLogic(EndCondition endCondition)
    {
        instance.StartCoroutine(_EndGame(endCondition));
    }

    private static IEnumerator _EndGame(EndCondition endCondition)
    {
        yield return null;
    }
    #endregion

    public static void SoftPauseGame()
    {

    }
}
