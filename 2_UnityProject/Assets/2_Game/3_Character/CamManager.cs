using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CamManager: MonoBehaviour
{
    static GameObject characterCamPrefab;
    static List<CinemachineVirtualCamera> activeCameras = new List<CinemachineVirtualCamera>();

   public static void SpawnCamera(Transform followTransform,out CinemachineVirtualCamera spawnedCam)
   {
        GameObject.Instantiate(characterCamPrefab);
        spawnedCam = characterCamPrefab.GetComponent<CinemachineVirtualCamera>();
        activeCameras.Add(spawnedCam);
        spawnedCam.Follow = followTransform.transform ;
   }

   public static void DeleteCamera(CinemachineVirtualCamera camToDestroy)
   {
        camToDestroy.gameObject.SetActive(false);
        Destroy(camToDestroy.gameObject);
        activeCameras.Remove(camToDestroy);
   }

   public static void SetCamPrefab(GameObject camPrefab)
   {
        characterCamPrefab = camPrefab;
   }


}
