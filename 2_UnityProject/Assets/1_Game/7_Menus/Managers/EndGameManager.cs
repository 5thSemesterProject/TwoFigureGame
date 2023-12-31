using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    [SerializeField] private GameObject WinScreen;
    [SerializeField] private GameObject LoseScreenTransition;

    private Coroutine endRoutine;

    #region Start
    public void EndGame(EndCondition endCondition)
    {
        CustomEventSystem.SwitchControlScheme(CustomEventSystem.GetInputMapping.InUI);

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
                yield break;
            case EndCondition.OxygenMan:

                Debug.Log("Man Died!");
                if (CharacterManager.ActiveCharacterData.movement.characterType != CharacterType.Man)
                {
                    CharacterManager.ActiveCharacterData.other.virtualCamera.gameObject.SetActive(true);
                    CharacterManager.ActiveCharacterData.virtualCamera.gameObject.SetActive(false);
                }
                break;
            case EndCondition.OxygenWoman:

                Debug.Log("Woman Died!");
                if (CharacterManager.ActiveCharacterData.movement.characterType != CharacterType.Woman)
                {
                    CharacterManager.ActiveCharacterData.other.virtualCamera.gameObject.SetActive(true);
                    CharacterManager.ActiveCharacterData.virtualCamera.gameObject.SetActive(false);
                }
                break;
            case EndCondition.Misc:
            default:
                Debug.Log("The game designers have no clue why you died... sorry.");
                break;
        }

        Volume postProsess = GameObject.Find("PostProcessing").GetComponent<Volume>();
        Vignette vignette;
        if (postProsess.profile.TryGet(out vignette))
            yield return LerpVignette(vignette, 0.6f, 1);
        else
            yield return new WaitForSecondsRealtime(1);
        

        yield return new WaitForSecondsRealtime(1);

        WSUI.AddOverlay(LoseScreenTransition);

        yield return new WaitForSecondsRealtime(0.5f);

        endRoutine = null;
        SceneLoader.LoadScene("LevelScene", this);
    }

    private IEnumerator LerpVignette(Vignette vignette, float targetIntesity, float targetSmoothness, float duration = 1)
    {
        float currentIntesity = (float)vignette.intensity;
        float currentSmoothness = (float)vignette.smoothness;
        float time = 0;

        while (time < 1)
        {
            vignette.intensity.value = Mathf.Lerp(currentIntesity,targetIntesity,time);
            vignette.smoothness.value = Mathf.Lerp(currentSmoothness,targetSmoothness,time);
            time += Time.deltaTime / duration;
            yield return null;
        }

    }
    #endregion


}
