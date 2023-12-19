using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    [SerializeField] private GameObject WinScreen;
    [SerializeField] private GameObject LoseScreenVignette;
    [SerializeField] private GameObject LoseScreenTransition;

    private Coroutine endRoutine;

    #region Start
    public void EndGame(EndCondition endCondition)
    {
        if (endRoutine == null)
        {
            StartCoroutine(_EndGame(endCondition));
        }
    }

    private IEnumerator _EndGame(EndCondition endCondition)
    {
        switch (endCondition)
        {
            case EndCondition.Win:

                WSUI.AddOverlay(WinScreen);
                Debug.Log("YOU WIN!");
                yield return new WaitForSecondsRealtime(2);
                SceneManager.LoadScene("MainMenu");

                yield break;
            case EndCondition.OxygenMan:

                Debug.Log("Man Died!");
                if (CharacterManager.ActiveCharacter != Characters.Man)
                {
                    //Switch Character
                }

                //Force Switch Death State

                break;
            case EndCondition.OxygenWoman:

                Debug.Log("Woman Died!");
                if (CharacterManager.ActiveCharacter != Characters.Woman)
                {
                    //Switch Character
                }

                //Force Switch Death State

                break;
            case EndCondition.Misc:
            default:
                Debug.Log("The game designers have no clue why you died... sorry.");
                break;
        }

        WSUI.AddOverlay(LoseScreenVignette);

        yield return new WaitForSecondsRealtime(1);

        WSUI.AddOverlay(LoseScreenTransition);

        yield return new WaitForSecondsRealtime(0.5f);

        endRoutine = null;
        SceneManager.LoadSceneAsync("LevelScene");
    }
    #endregion


}
