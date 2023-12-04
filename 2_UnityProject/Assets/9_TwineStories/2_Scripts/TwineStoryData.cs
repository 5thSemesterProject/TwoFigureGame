using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwineStoryData : SaveData
{
    public string title;
    public string thumbnailPath;
    public string qrCodePath;

    public bool unlocked;

    public TwineStoryData(string title,string thumbnailPath,string qrCodePath)
    {
        this.title = title;
        this.thumbnailPath = thumbnailPath;
        this.qrCodePath = qrCodePath;
    }

    public override void LoadDefaultValues()
    {
        throw new System.NotImplementedException();
    }
}
