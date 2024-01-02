using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioUtility
{
    List<AudioClip> audioClips = new List<AudioClip>();
    int numberOfClipsToLoad;
    private Action<AudioClip[]> onAllClipsLoaded;

    public void LoadAllAudioClips<T>(Action<AudioClip[]> onAllClipsLoadedCallback, bool removePrefix = true,string prefixToRemove="E_") where T: Enum
    {
        onAllClipsLoaded = onAllClipsLoadedCallback;

        string[] filenames = Enum.GetNames(typeof(T));
        numberOfClipsToLoad = filenames.Length;
        string folderName = typeof(T).UnderlyingSystemType.Name;
        folderName = RemovePrefix(folderName,prefixToRemove);

        for (int i = 0; i < filenames.Length; i++)
        {
            filenames[i] = RemovePrefix(filenames[i],"_");
            string filePath = "Assets/2_Resources/2_Sound/"+folderName+"/"+filenames[i]+".wav";
            AsyncOperationHandle<AudioClip> asyncOperationHandle =  Addressables.LoadAssetAsync<AudioClip>(filePath);
            asyncOperationHandle.Completed+=LoadClipToMemory;
        }
    }

    void LoadClipToMemory(AsyncOperationHandle<AudioClip> asyncOperationHandle)
    {
        AudioClip audioClip = asyncOperationHandle.Result;
        audioClips.Add(audioClip);

        if (numberOfClipsToLoad == audioClips.Count)
            onAllClipsLoaded.Invoke(audioClips.ToArray());
    }

    public static string RemovePrefix(string input, string prefix)
    {
        if (input.StartsWith(prefix, StringComparison.Ordinal))
        {
            return input.Substring(prefix.Length);
        }

        // No match, return the original string
        return input;
    }

    public static int RandomNumber(int lastRandom, int length, out int newLastRandom)
    {   
        length = length-1;
        int random = UnityEngine.Random.Range(0,length);
        if (length!=0)
        {
            if(random==lastRandom)
                random = (random + 1) % length;
        }
        newLastRandom = random;
        return random;
    }
}
