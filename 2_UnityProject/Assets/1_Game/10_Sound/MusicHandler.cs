using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicHandler : MonoBehaviour
{
    [SerializeField] E_2_Music[] songsToPlay;
    private List <AudioClip> musicClips = new List<AudioClip>();
    void Awake()
    {

        var audioUtility = new AudioUtility();
        audioUtility.LoadAllAudioClipsAsync<E_2_Music>(SaveAllMusicClips);
    }

   void SaveAllMusicClips(AudioClip[] audioClips)
   {
        var songNames  = new List<string>();
        for (int i = 0; i < songsToPlay.Length; i++)
        {
            string songName = songsToPlay[i].ToString();
            songName = AudioUtility.RemovePrefix(songName,"_");
            songNames.Add(songName);
        }

        for (int i = 0; i < audioClips.Length; i++)
        {
            if (songNames.Contains(audioClips[i].name))
            {
                musicClips.Add(audioClips[i]);
            }

        }

        SoundSystem.PlayBackgroundMusic(musicClips.ToArray(),0);
   }

}
