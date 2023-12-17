using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using System;

public class StartMenuManager : MonoBehaviour
{
    private Coroutine transition;

    [SerializeField] private CanvasGroup mainMenuGroup;
    [SerializeField] private CanvasGroup archiveGroup;

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
            mainMenuGroup.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }

        CustomEventSystem.EnableUIInputs();
        transition = null;
        SceneManager.LoadScene("LevelScene");
    }
    private IEnumerator Archive()
    {
        CustomEventSystem.DisableUIInputs();

        float alpha = 1;

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * 2;
            mainMenuGroup.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }

        alpha = 0;

        while (alpha < 1)
        {
            alpha += Time.deltaTime * 2;
            archiveGroup.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }

        CustomEventSystem.EnableUIInputs();
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
