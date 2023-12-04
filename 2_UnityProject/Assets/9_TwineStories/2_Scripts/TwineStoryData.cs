using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TwineStoryData
{
    public string title;
    public string thumbnailPath;
    public string link;
    public bool unlocked;

    public TwineStoryData(string title,string thumbnailPath,string link)
    {
        this.title = title;
        this.thumbnailPath = thumbnailPath;
        this.link = link;
        unlocked = false;
    }

}
