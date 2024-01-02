using System.Collections;
using UnityEngine;
using TMPro;

public class WinMenuManager : MonoBehaviour
{
    private Coroutine transition;
    [SerializeField] private TextMeshProUGUI textMeshPro;

    private void OnEnable()
    {
        textMeshPro.text = FormatTime(GameManager.GetGameTime);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time/60);
        int seconds = Mathf.FloorToInt(time%60);
        int milliseconds = Mathf.FloorToInt((time*100)%100);
        string returnString = $"Time: \n{minutes}:{seconds:00}.<size=70%>{milliseconds}";
        return returnString;
    }

    public void PauseMenuLogic(string action)
    {
        IEnumerator coroutine = null;

        switch (action)
        {
            case "Restart":
                coroutine = RestartLogic();
                break;
            case "Menu":
                coroutine = MenuLogic();
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
    private IEnumerator RestartLogic()
    {
        yield return null;
        Time.timeScale = 1;
        GameManager.UnSubscribeEvents();
        SceneLoader.LoadScene("LevelScene", this, 3, true, false);
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