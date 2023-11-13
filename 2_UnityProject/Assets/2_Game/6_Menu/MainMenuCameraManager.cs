using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MainMenuCameraManager : MonoBehaviour
{
    private CinemachineVirtualCamera[] cameras;
    private GameObject currentActiveCamera;

    private void Awake()
    {
        //Load Cameras
        cameras = GetComponentsInChildren<CinemachineVirtualCamera>();

        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }

        //Activate First Camera
        ActivateCamera("ApplicationStartCam");
    }

    public CinemachineVirtualCamera ActivateCamera(string cameraName)
    {
        if (currentActiveCamera != null)
        {
            currentActiveCamera.SetActive(false);

            if (currentActiveCamera.name == cameraName)
            {
                return null;
            }
        }

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i].gameObject.name == cameraName)
            {
                cameras[i].gameObject.SetActive(true);
                currentActiveCamera = cameras[i].gameObject;
                return cameras[i];
            }
        }

        return null;
    }
}
