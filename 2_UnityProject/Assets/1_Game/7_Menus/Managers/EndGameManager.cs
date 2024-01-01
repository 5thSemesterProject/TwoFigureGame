using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

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
                CustomEventSystem.SwitchControlScheme(CustomEventSystem.GetInputMapping.InUI);
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
        ColorAdjustments adjustments;
        if (postProsess.profile.TryGet(out adjustments))
            yield return LerpMomochrome(adjustments, -100f, 1);
        else
            yield return new WaitForSecondsRealtime(1);
        

        yield return new WaitForSecondsRealtime(1);

        WSUI.AddOverlay(LoseScreenTransition);

        yield return new WaitForSecondsRealtime(0.5f);

        endRoutine = null;
        SceneLoader.LoadScene("LevelScene", this);
    }

    private IEnumerator LerpMomochrome(ColorAdjustments adjustments, float targetSaturation, float duration = 1)
    {
        float currentSaturation = (float)adjustments.saturation;
        float time = 0;

        while (time < 1)
        {
            adjustments.saturation.value = Mathf.Lerp(currentSaturation, targetSaturation, time);
            time += Time.deltaTime / duration;
            yield return null;
        }

    }
    #endregion


}
