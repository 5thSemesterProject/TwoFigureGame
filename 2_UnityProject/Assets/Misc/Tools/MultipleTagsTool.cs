using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleTagsTool : MonoBehaviour
{
    [SerializeField] List<string> tags;

    static List<string> existingTags = new List<string>();

    public string[] GetTags()
    {
        return tags.ToArray();
    }

    static void TryAddToExistingTags(string tagToAdd)
    {
        if (!existingTags.Contains(tagToAdd))
            existingTags.Add(tagToAdd);
    }
    public static string[] GetExistingTags()
    {
        return existingTags.ToArray();
    }

}
