using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    private Coroutine transition;
    [SerializeField] private ButtonGroupFade rootGroup;

    [SerializeField] private ButtonEnabler archiveGroup;
    [SerializeField] private ButtonEnabler controlsGroup;
    [SerializeField] private ButtonEnabler pauseGroup;

    private void OnEnable()
    {
        CharacterManager.manData.oxygenBar.gameObject.GetComponent<CanvasGroup>().alpha = 0;
        CharacterManager.womanData.oxygenBar.gameObject.GetComponent<CanvasGroup>().alpha = 0;
    }

    private void OnDisable()
    {
        CharacterManager.manData.oxygenBar.gameObject.GetComponent<CanvasGroup>().alpha = 1;
        CharacterManager.womanData.oxygenBar.gameObject.GetComponent<CanvasGroup>().alpha = 1;
    }

    public void PauseMenuLogic(string action)
    {
        IEnumerator coroutine = null;

        switch (action)
        {
            case "Resume":
                coroutine = ResumeLogic();
                break;
            case "Menu":
                coroutine = MenuLogic();
                break;
            case "Options":
                coroutine = OptionsLogic();
                break;
            case "FromOptions":
                coroutine = FromOptionsLogic();
                break;
            case "FromArchive":
                coroutine = FromArchiveLogic();
                break;
            case "Archive":
                coroutine = ArchiveLogic();
                break;
            case "Quit":
                coroutine = QuitLogic();
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
    private IEnumerator ResumeLogic()
    {
        if (rootGroup != null)
        {
            CustomEventSystem.DisableUIInputs();
            yield return new WaitForSecondsRealtime(rootGroup.FadeOut());
            CustomEventSystem.EnableUIInputs();
        }
        GameManager.TogglePauseManual();
    }
    private IEnumerator FromOptionsLogic()
    {
        yield return StartMenuManager.TransitionBetweenCanvasGroups(controlsGroup, pauseGroup);
        transition = null;
    }
    private IEnumerator FromArchiveLogic()
    {
        yield return StartMenuManager.TransitionBetweenCanvasGroups(archiveGroup, pauseGroup);
        transition = null;
    }
    private IEnumerator OptionsLogic()
    {
        yield return StartMenuManager.TransitionBetweenCanvasGroups(pauseGroup, controlsGroup);
        transition = null;
    }
    private IEnumerator ArchiveLogic()
    {
        yield return StartMenuManager.TransitionBetweenCanvasGroups(pauseGroup, archiveGroup);
        transition = null;
    }
    private IEnumerator MenuLogic()
    {
        yield return null;
        Time.timeScale = 1;
        GameManager.UnSubscribeEvents();
        SceneLoader.LoadScene("MainMenu", this, 3, true);
    }
    private IEnumerator QuitLogic()
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
