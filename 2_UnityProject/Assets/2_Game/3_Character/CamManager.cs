using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Burst.CompilerServices;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

class OccludingObject
{
     public GameObject gameObject;
     public Coroutine coroutine;
     public Renderer renderer;
     public MonoBehaviour runCoroutineOn;

     public OccludingObject(GameObject gameObject,Coroutine coroutine, MonoBehaviour runCoroutineOn)
     {
          this.gameObject = gameObject;
          this.coroutine = coroutine;
          this.runCoroutineOn = runCoroutineOn;
          renderer = gameObject.GetComponent<Renderer>();
     }

    public override bool Equals(object obj)
    {
         if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        OccludingObject other = (OccludingObject)obj;
        return ReferenceEquals(this.gameObject, other.gameObject);
    }

     public override int GetHashCode()
    {
        return ReferenceEquals(gameObject, null) ? 0 : gameObject.GetHashCode();
    }

    public void LerpAlpha(float targetAlpha)
     {
          
        if (coroutine != null)
        {
            runCoroutineOn.StopCoroutine(coroutine);
            coroutine = null;
        }

        coroutine = runCoroutineOn.StartCoroutine(_LerpAlpha(targetAlpha));
     }

     IEnumerator _LerpAlpha(float targetAlpha)
     {
        float currentAlpha = GetAlpha();
        float elapsedTime = 0;
        float duration = 0.8f;

        while (elapsedTime < duration)
        {
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, elapsedTime / duration);

            SetAlpha(currentAlpha);   
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        coroutine = null;
     }

     public void SetAlpha(float inputAlpha)
     {
          Material material = renderer.materials[0];     
          Vector4 color = material.GetVector("_BaseColor");
          color.w = inputAlpha;
          material.SetVector("_BaseColor",color);
     }

     public float GetAlpha()
     {
          Material material = renderer.materials[0];     
          Vector4 color = material.GetVector("_BaseColor");
          return color.w;
     }

}


public class CamManager: MonoBehaviour
{    
    [SerializeField] Transform camParent;
    CinemachineBrain cineBrain;
    static CamManager instance;
    static GameObject characterCamPrefab;
    static List<CinemachineVirtualCamera> activeCameras = new List<CinemachineVirtualCamera>();
    static IEnumerable<OccludingObject> occludingObjects = new List<OccludingObject>();
    static RaycastHit [] raycastHits = new RaycastHit[12];


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

          Destroy(objectToDestroy);
   }

   public static void SetCamPrefab(GameObject camPrefab)
   {
        characterCamPrefab = camPrefab;
   }


   public static void FindOccludingObjects(Transform transformToBeVisible)
   {
          var tempOccludingObjects = GetOccludingObjects(transformToBeVisible);

          //Find Objects To Reset and Reset
          List<OccludingObject> objectsToReset = occludingObjects.Except(tempOccludingObjects).ToList();
          objectsToReset.ForEach(occludingObject =>occludingObject.LerpAlpha(1));
 
          //Update List and Adjust Alpha
          occludingObjects = tempOccludingObjects;
          
          foreach(var item in occludingObjects)
          {
               item.LerpAlpha(0.5f);
          }
   }

   static IEnumerable<OccludingObject> GetOccludingObjects(Transform transformToBeVisible)
   { 
     int smokeLayerIndex = 7;
     int allLayersMask = ~ (1 << smokeLayerIndex);
     int layerMask =allLayersMask;
     MonoBehaviour runCouroutineOn = transformToBeVisible.GetComponent<Movement>();
     CharacterController characterController = transformToBeVisible.GetComponent<CharacterController>();

     float radius = characterController.radius;
     float radiusOffset =  characterController.height/2-radius;
     Vector3 worldCenter  = characterController.transform.TransformPoint(characterController.center);
     Vector3 point1 = worldCenter+ characterController.transform.up*radiusOffset;
     Vector3 point2 = worldCenter -characterController.transform.up*radiusOffset;
     
     Vector3 startPoint = Camera.main.transform.position;;
     Vector3 direction = startPoint-worldCenter;
     direction = direction.normalized;

     int amountOfHits = Physics.CapsuleCastNonAlloc(point1,point2,radius,direction,raycastHits,Mathf.Infinity,layerMask);

     List<OccludingObject> tempOccludingObjects = new List<OccludingObject>();

     for (int i = 0; i < amountOfHits; i++)
     {
          GameObject hitGameObject = raycastHits[i].transform.gameObject;
          if (hitGameObject.tag =="Wall")
          {
               tempOccludingObjects.Add(new OccludingObject(hitGameObject,null,runCouroutineOn));
          }
               
     }

     return tempOccludingObjects;
   }


}
