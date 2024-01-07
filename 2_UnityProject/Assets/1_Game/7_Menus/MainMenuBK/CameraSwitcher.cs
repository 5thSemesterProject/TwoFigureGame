using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public enum MainMenuCamIdentifier
{
    PressAnyKey,MainMenu, Archive, Options, Play
}


[Serializable]
class MainMenuCamera
{
    public MainMenuCamIdentifier identifier;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    public void SetCamera(bool active)
    {
        virtualCamera.enabled = active;
    }
}

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField]private MainMenuCamera [] mainMenuCameras = new MainMenuCamera[Enum.GetValues(typeof(MainMenuCamIdentifier)).Length];
    private MainMenuCamera [] _mainMenuCameras = new MainMenuCamera[Enum.GetValues(typeof(MainMenuCamIdentifier)).Length];
    private MainMenuCamIdentifier activeCameraId;
    private MainMenuCamIdentifier previousCameraId;

    void Awake()
    {
        //Deactivate all cams except for set as first cam
        int firstCams=0;
        foreach(MainMenuCamera mainMenuCamera in mainMenuCameras)
        {
            if (mainMenuCamera.identifier == MainMenuCamIdentifier.PressAnyKey)
            {
                firstCams++;
                activeCameraId = mainMenuCamera.identifier;
                previousCameraId = activeCameraId ;
            }
            mainMenuCamera.SetCamera(mainMenuCamera.identifier == MainMenuCamIdentifier.PressAnyKey);    
        }

       if (firstCams>1)
        Debug.LogWarning("Multiple Cameras are marked as first one. Please only mark one!");

    }

    void  OnValidate()
    {
        //Update Array in case enums have updated

        //Prevent multiple cams of the same type
        int[] countMultiple = new int[Enum.GetValues(typeof(MainMenuCamIdentifier)).Length];
        for (int i = 0; i < mainMenuCameras.Length; i++)
        {
            int index = (int)mainMenuCameras[i].identifier;
            countMultiple[index]++;

            if (countMultiple[index]>1)
            {
                Debug.LogError("Multiple cams of the same type are not allowed!");
                mainMenuCameras  = _mainMenuCameras;
                return;
            }
        }

        _mainMenuCameras = mainMenuCameras;

    }

    public void SwitchToPreviousCamera()
    {
        SwitchCamera(previousCameraId);
    }

    public void SwitchCamera(MainMenuCamIdentifier menuCamIdentifier)
    {
        GetCameraByIdentifier(activeCameraId).SetCamera(false);
        
        previousCameraId = activeCameraId;
        activeCameraId = menuCamIdentifier;
        GetCameraByIdentifier(menuCamIdentifier).SetCamera(true);
    }

    public void SwitchCamera(string menuCamIdentifier)
    {
        GetCameraByIdentifier(activeCameraId).SetCamera(false);
        previousCameraId = activeCameraId;
        MainMenuCamera updatedCam = GetCameraByIdentifier(menuCamIdentifier);
        updatedCam.SetCamera(true);
        activeCameraId = updatedCam.identifier;
    }


    MainMenuCamera GetCameraByIdentifier(MainMenuCamIdentifier menuCamIdentifier)
    {
        for (int i = 0; i < mainMenuCameras.Length; i++)
        {
            if (mainMenuCameras[i].identifier == menuCamIdentifier)
                return mainMenuCameras[i];
        }

        Debug.LogWarning("Camera not found. Make sure to add the camera");
        return null;
    }

    MainMenuCamera GetCameraByIdentifier(string menuCamIdentifier)
    {
        for (int i = 0; i < mainMenuCameras.Length; i++)
        {
            if (mainMenuCameras[i].identifier.ToString() == menuCamIdentifier)
                return mainMenuCameras[i];
        }

        Debug.LogWarning("Camera not found. Make sure to add the camera");
        return null;
    }

    
}
