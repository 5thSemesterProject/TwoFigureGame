using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using System;

public class StartMenuManager : MonoBehaviour
{
    private Coroutine transition;
    [SerializeField] private ButtonEnabler mainMenuGroup;
    [SerializeField] private ButtonEnabler archiveGroup;
    [SerializeField] private ButtonEnabler startGroup;
    [SerializeField] private VideoManager introVideo;

    private void Start()
    {
        CustomEventSystem.SwitchControlScheme(CustomEventSystem.GetInputMapping.InUI);
    }

    public void MenuAction(string actionName)
    {
        IEnumerator coroutine = null;

        //Switch Logics
        switch (actionName)
        {
            case "Start":
                coroutine = StartGame();
                break;
            case "Options":
                coroutine = Options();
                break;
            case "Quit":
                coroutine = QuitGame();
                break;
            case "Archive":
                coroutine = Archive();
                break;
            case "Menu":
                coroutine = Menu();
                break;
            case "Enter":
                coroutine = Enter();
                break;
            default:
                break;
        }

        //Catch Typos
        if (coroutine == null)
        {
            return;
        }

        //Do Menu Transition
        if (transition != null)
        {
            Debug.LogWarning("Previous transition did not yet end!");
        }
        transition = StartCoroutine(coroutine);
    }

    #region Button Logics
    private IEnumerator StartGame()
    {
        CustomEventSystem.DisableUIInputs();

        float alpha = 1;

        while (alpha > 0)
        {
            alpha -= Time.deltaTime;
            mainMenuGroup.canvasGroup.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }

        CustomEventSystem.EnableUIInputs();
        transition = null;

        introVideo.StartVideo();
        introVideo.videoFinished.AddListener(() => SceneManager.LoadSceneAsync("LevelScene"));

    }
    private IEnumerator Archive()
    {
        yield return TransitionBetweenCanvasGroups(mainMenuGroup, archiveGroup);
    }
    private IEnumerator Menu()
    {
        yield return TransitionBetweenCanvasGroups(archiveGroup,mainMenuGroup);
    }
    private IEnumerator Enter()
    {
        yield return TransitionBetweenCanvasGroups(startGroup, mainMenuGroup);
    }
    private IEnumerator TransitionBetweenCanvasGroups(ButtonEnabler startGroup, ButtonEnabler endGroup)
    {
        CustomEventSystem.DisableUIInputs();

        float alpha = startGroup.canvasGroup.alpha;

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 2;
            startGroup.canvasGroup.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }

        startGroup.canvasGroup.blocksRaycasts = false;
        startGroup.interactable = false;

        alpha = endGroup.canvasGroup.alpha;

        while (alpha < 1)
        {
            alpha += Time.deltaTime * 2;
            endGroup.canvasGroup.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }

        endGroup.canvasGroup.blocksRaycasts = true;
        endGroup.interactable = true;

        CustomEventSystem.EnableUIInputs();
        CustomEventSystem.ResetSelectedButtons();

        transition = null;
    }
    private IEnumerator Options()
    {
        yield return null;
        Debug.Log("Not Implemented");
        transition = null;
    }
    private IEnumerator QuitGame()
    {
        yield return null;
        transition = null;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

}
