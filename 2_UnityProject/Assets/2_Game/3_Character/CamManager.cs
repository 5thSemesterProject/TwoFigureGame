using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Burst.CompilerServices;
using UnityEngine;
using System.Linq;

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
          if (coroutine!=null)
          {
               runCoroutineOn.StopCoroutine(coroutine);
               coroutine = null;
          }
               
          coroutine = runCoroutineOn.StartCoroutine(_LerpAlpha(targetAlpha));
     }

     IEnumerator _LerpAlpha(float alpha)
     {
          float currentAlpha = GetAlpha();
          float targetAlpha = alpha;
          float speed = 0.1f;
          float t = 0;

          while (Mathf.Abs(currentAlpha-targetAlpha)>0.01f)
          {
               currentAlpha = Mathf.Lerp(currentAlpha,targetAlpha,t);
               SetAlpha(currentAlpha);
               t +=speed*Time.deltaTime;
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
          Material[] materials = new Material[1]{material};
          renderer.materials = materials;
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
    static List<OccludingObject> occludingObjects = new List<OccludingObject>();


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
          List<OccludingObject> tempOccludingObjects = GetOccludingObjects(transformToBeVisible);

          //Find Objects To Reset and Reset
          List<OccludingObject> objectsToReset = occludingObjects.Except(tempOccludingObjects).ToList();;
          objectsToReset.ForEach(occludingObject =>occludingObject.LerpAlpha(1));


          //Update List and Adjust Alpha
          occludingObjects = tempOccludingObjects;
          occludingObjects.ForEach(occludingObject=>occludingObject.LerpAlpha(0.5f));
   }

   static List<OccludingObject> GetOccludingObjects(Transform transformToBeVisible)
   { 
     Vector3 offset = new Vector3(0,1,0);
     int smokeLayerIndex = 7;
     int allLayersMask = ~ (1 << smokeLayerIndex);
     int layerMask =allLayersMask;

     List<OccludingObject> tempOccludingObjects = new List<OccludingObject>();
     MonoBehaviour runCouroutineOn = transformToBeVisible.GetComponent<Movement>();

     Vector3 startPoint = Camera.main.transform.position;
     Vector3 endPoint = transformToBeVisible.position+offset;
     
     Vector3 endPointHead = transformToBeVisible.position+Vector3.up*2;

     Vector3 direction = endPoint-startPoint;
     direction = direction.normalized;

     GameObject hitObject;


     //Import all occluding objects
     while(true)
     {
          Debug.DrawRay(startPoint,direction*400,Color.red,0.1f,true);

          if (Physics.Raycast(startPoint, direction, out RaycastHit hitInfo,Mathf.Infinity,layerMask))
          {    
               hitObject = hitInfo.transform.gameObject;
          }
               
          else
               break;

               if (hitObject.tag =="Wall" && !tempOccludingObjects.Any(obj => obj.gameObject == hitObject))
               {
                    tempOccludingObjects.Add(new OccludingObject(hitObject,null,runCouroutineOn));
               }
               else
               break;

          startPoint = hitInfo.point;
     }

     return tempOccludingObjects;
   }



}
