using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class StartMenuManager : MonoBehaviour
{
    private Coroutine transition;

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
        yield return null;
        transition = null;
        SceneManager.LoadScene("Scene");
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
