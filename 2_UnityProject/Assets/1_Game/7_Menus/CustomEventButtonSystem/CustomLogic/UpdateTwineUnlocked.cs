using UnityEngine;

public class UpdateTwineUnlocked : MonoBehaviour
{
    [SerializeField] private TextAsset twineStory;

    private void OnEnable()
    {
        if (!CheckIfUnlocked())
        {
            gameObject.SetActive(false);
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
