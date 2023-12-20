using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    private Coroutine transition;

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
        yield return null;
        GameManager.TogglePauseManual();
    }
    private IEnumerator OptionsLogic()
    {
        yield return null;
        Debug.Log("Not Implemented");
        transition = null;
    }
    private IEnumerator MenuLogic()
    {
        yield return null;
        Time.timeScale = 1;
        GameManager.UnSubscribeEvents();
        SceneManager.LoadSceneAsync("MainMenu");
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
