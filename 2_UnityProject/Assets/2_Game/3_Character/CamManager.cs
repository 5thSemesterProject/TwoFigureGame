using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CamManager: MonoBehaviour
{    
     [SerializeField] Transform camParent;
     CinemachineBrain cineBrain;
     static CamManager instance;
    static GameObject characterCamPrefab;

    static List<CinemachineVirtualCamera> activeCameras = new List<CinemachineVirtualCamera>();

    void Awake()
    {
          if (instance!=null && instance!=this)
               Destroy(this);
          else
               instance = this;

          cineBrain = Camera.main.GetComponent<CinemachineBrain>();
    }

   public static void SpawnCamera(Transform followTransform,out CinemachineVirtualCamera spawnedCam)
   {
        GameObject spawnedCamHolder =GameObject.Instantiate(characterCamPrefab);
        spawnedCamHolder.name = "CharacterCam";
        spawnedCamHolder.transform.SetParent(instance.camParent);
        spawnedCam = spawnedCamHolder.GetComponent<CinemachineVirtualCamera>();
        activeCameras.Add(spawnedCam);
        spawnedCam.Follow = followTransform.transform ;
   }

   public static void DeleteCamera(CinemachineVirtualCamera camToDestroy)
   {
        camToDestroy.gameObject.SetActive(false);
        activeCameras.Remove(camToDestroy);

        instance.StartCoroutine(instance.WaitForCameraToDelete(camToDestroy.gameObject));
   }


   IEnumerator WaitForCameraToDelete(GameObject objectToDestroy)
   {
          yield return new WaitForSeconds(0.1f);
          yield return new WaitUntil(()=>!instance.cineBrain.IsBlending);

          Debug.Log ("Destroyed");
          Destroy(objectToDestroy);
   }

   public static void SetCamPrefab(GameObject camPrefab)
   {
        characterCamPrefab = camPrefab;
   }


}
