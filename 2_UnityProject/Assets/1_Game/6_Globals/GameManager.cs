using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
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

    //LowTriggers
    public event ButtonEvent notifyLow;
    public event ButtonEvent unNotifyLow;
    private bool hasBeenNotified;

    //Game Time
    public static float GetGameTime { get => elapsedGameTime; }
    private static float elapsedGameTime;
    private static float lastTimeMarker = 0;

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (Input.GetKey(KeyCode.LeftShift))
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
    }
#endif

    #region Game Time
    private static void ResetTimer()
    {
        elapsedGameTime = 0;
    }
    private static void ResumeTimer()
    {
        lastTimeMarker = Time.realtimeSinceStartup;
    }
    private static float StopTimer()
    {
        elapsedGameTime += Time.realtimeSinceStartup - lastTimeMarker;
        return elapsedGameTime;
    }
    #endregion

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

        ResetTimer();
        ResumeTimer();
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

        UnNotifyLow();
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
            StopTimer();
            pauseMenu = WSUI.AddOverlay(pauseMenuPrefab);
            Time.timeScale = 0;
            CustomEventSystem.SwitchControlScheme(CustomEventSystem.GetInputMapping.InUI);
        }
        else
        {
            ResumeTimer();
            WSUI.RemovePrompt(pauseMenu);
            Time.timeScale = 1;
            CustomEventSystem.SwitchControlScheme(CustomEventSystem.GetInputMapping.InGame);
        }
    }
    #endregion

    #region NotifyLow
    private static void NotifyLow()
    {
        instance.notifyLow?.Invoke();
        Volume postProsess = GameObject.Find("PostProcessing").GetComponent<Volume>();
        Vignette vignette;
        if (postProsess.profile.TryGet(out vignette))
                postProsess.StartCoroutine(LerpVignette(vignette, 0.5f, 1));
    }
    private static void UnNotifyLow()
    {
        instance.unNotifyLow?.Invoke();
        Volume postProsess = GameObject.Find("PostProcessing").GetComponent<Volume>();
        Vignette vignette;
        if (postProsess.profile.TryGet(out vignette))
            postProsess.StartCoroutine(LerpVignette(vignette, 0.4f, 0.5f));
    }

    private static IEnumerator LerpVignette(Vignette vignette, float targetIntesity, float targetSmoothness, float duration = 1)
    {
        float currentIntesity = (float)vignette.intensity;
        float currentSmoothness = (float)vignette.smoothness;
        float time = 0;

        while (time < 1)
        {
            vignette.intensity.value = Mathf.Lerp(currentIntesity, targetIntesity, time);
            vignette.smoothness.value = Mathf.Lerp(currentSmoothness, targetSmoothness, time);
            time += Time.deltaTime / duration;
            yield return null;
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
        else if (CharacterManager.IsManLow || CharacterManager.IsWomanLow)
        {
            if (!hasBeenNotified)
            {
                hasBeenNotified = true;
                NotifyLow();
            }
        }
        else if (hasBeenNotified)
        {
            hasBeenNotified = false;
            UnNotifyLow();
        }
    }

    public static void EndGameLogic(EndCondition endCondition)
    {
        UnSubscribeEvents();
        instance.hasGameEnded = true;
        StopTimer();

        GameObject endPrefab = Instantiate(instance.endScreenPrefab);

        EndGameManager endGameManager = endPrefab.GetComponent<EndGameManager>();
        endGameManager.EndGame(endCondition);
    }
    #endregion
}
