using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class StartMenuManager : MonoBehaviour
{
    private Coroutine transition;
    [SerializeField] private MainMenuCameraManager cameraManager;

    private IEnumerator Start()
    {
        yield return null;
        cameraManager.ActivateCamera("MainMenuCamera");
    }

    private void DoMenuTransition(IEnumerator enumerator)
    {
        if (transition != null)
        {
            Debug.LogWarning("Previous transition did not yet end!");
        }
        StartCoroutine(enumerator);
    }

    public void MenuAction(string actionName)
    {
        IEnumerator coroutine = null;

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

        if (coroutine == null)
        {
            return;
        }

        DoMenuTransition(coroutine);
    }

    #region Button Logics
    private IEnumerator StartGame()
    {
        CinemachineVirtualCamera camera =  cameraManager.ActivateCamera("GameStartCam");
        CinemachineTrackedDolly path = camera.GetCinemachineComponent<CinemachineTrackedDolly>();

        float i = 0;
        while (i<2)
        {
            i += Time.deltaTime/1.5f;
            path.m_PathPosition = i;
            yield return null;
        }
        transition = null;
        SceneManager.LoadScene(1);
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
