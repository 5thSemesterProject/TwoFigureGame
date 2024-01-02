using UnityEngine;

public class UpdateTwineUnlocked : MonoBehaviour
{
    [SerializeField] private TextAsset twineStory;
    [SerializeField] private CustomButton correspondingButton;

    private void OnEnable()
    {
        if (!CheckIfUnlocked())
        {
            gameObject.SetActive(false);
            if (correspondingButton != null)
            {
                correspondingButton.IsInteractable = false;
            }
        }
    }

    private bool CheckIfUnlocked()
    {
        if (twineStory != null)
        {
            TwineStoryData stroy = JsonUtility.FromJson<TwineStoryData>(twineStory.ToString());
            return stroy.unlocked;
        }
        return false;
    }
}
