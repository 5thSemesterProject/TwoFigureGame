using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.Rendering.DebugUI;

public enum TraversalType
{
    Crawl, JumpOver
}

public class Movement : MonoBehaviour, IIntersectSmoke
{
    private CharacterController characterController;
    private Animator animator;

    private NavMeshAgent navMeshAgent;

    public Coroutine coroutine;
    public Coroutine lerpRoutine;
    public Coroutine moveAcross;

    [SerializeField] private float lerpValue = 0.2f;
    [SerializeField] private float movementSpeed = 25f;
    [SerializeField] private float rotationSpeed = 50f;

    [SerializeField] private float smokeIntersectionRadius = 1;
    [SerializeField] private float gravity = 9.81f;
    private float minWallDistance = 0.7f;
    private float timeFalling;

    public Interactable interactable;
    public Oxygenstation oxygenstation;

    public CharacterType characterType;

    private void Awake()
    {
        if (!TryGetComponent(out characterController))
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.slopeLimit = characterController.stepOffset = 0;
        }

        animator = GetComponentInChildren<Animator>();

        if (!TryGetComponent(out navMeshAgent))
        {
            Debug.LogError("Missing Navmesh Agent on character prefab");
        }
        else
            navMeshAgent.enabled = false;
    }

    #region FogStuff
    public Vector4 GetSphereInformation()
    {
        return VectorHelper.Convert3To4(transform.position,2f);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public float GetIntersectionRadius()
    {
        return smokeIntersectionRadius;
    }
    #endregion

    #region Movement
    private void Update()
    {
        //characterController.Move(Vector3.down*0.001f);

        if (!characterController.isGrounded)
        {
            float gravityFallDistance = gravity * timeFalling * timeFalling;
            characterController.Move(Vector3.down * gravityFallDistance);
            timeFalling += Time.deltaTime;
        }
        else
        {
            timeFalling = 0;
        }
    }

    public Vector2 MovePlayer(Vector2 axis, float speed = 1)
    {

        Vector3 movement=default;
        Vector3 movementDir=default;

        //float yValue = transform.position.y;
        axis = axis.magnitude >= 1 ? axis.normalized : axis;

        Vector3 characterForward = Camera.main.transform.forward;
        Vector3 characterRight = Camera.main.transform.right;
        movementDir = characterForward * axis.y + characterRight * axis.x;
        movement = movementDir * movementSpeed * speed * Time.deltaTime * Time.timeScale / 3;
        movement = VectorHelper.Convert2To3(OptimizeMovement(transform.position, VectorHelper.Convert3To2(movement)));
 
        characterController.Move(movement);

        if (movement.magnitude >= 0.001)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement.normalized);
            float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
            if (Mathf.Abs(angleDifference) > 4)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lerpValue * rotationSpeed * Time.deltaTime * speed);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
  
        //transform.position = new Vector3(transform.position.x, yValue, transform.position.z);
        animator.SetBool("Grounded", true);
        animator.SetFloat("MotionSpeed", 1);
        animator.SetFloat("Speed", speed>0? (movement.magnitude / Time.deltaTime * 3):0);
        return VectorHelper.Convert3To2(movement);
    }

    public void MovePlayerToPos(Vector3 position,float speed=1)
    {
        navMeshAgent.enabled = true;
        navMeshAgent.autoTraverseOffMeshLink = false;
        characterController.enabled = false;

        if (position!=navMeshAgent.destination && moveAcross==null)
        {
            navMeshAgent.SetDestination(position);
            animator.SetBool("Grounded", true);
            animator.SetFloat("MotionSpeed", 1);
            animator.SetFloat("Speed", navMeshAgent.velocity.magnitude / Time.deltaTime * 3f);
        }

        characterController.enabled = true;



        if (navMeshAgent.isOnOffMeshLink)
        {
            if (moveAcross==null)
            {
               moveAcross = StartCoroutine(MoveAcrossNavMeshLink());
            }      
        }

    }

    IEnumerator MoveAcrossNavMeshLink()
    {
        OffMeshLinkData data = navMeshAgent.currentOffMeshLinkData;
        navMeshAgent.updateRotation = false;
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        Vector3 startPos = navMeshAgent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * navMeshAgent.baseOffset;
        float duration = (endPos-startPos).magnitude/navMeshAgent.velocity.magnitude;

        duration = ((endPos-startPos).magnitude/movementSpeed) * 3f;

        float t = 0.0f;
        float tStep = 1.0f/duration;
        tStep = 1*tStep;
        characterController.enabled = false;
        navMeshAgent.enabled = false;
        
        Debug.Log ("MoveAcrossStart");

        while(t<1.0f){
            transform.position = Vector3.Lerp(startPos,endPos,t);
            if (navMeshAgent.isOnNavMesh)
                navMeshAgent.destination = transform.position;
            
            t+=tStep*Time.deltaTime;

            animator.SetFloat("Speed", (1/duration) / Time.deltaTime * 3f);

            Debug.Log ("MovingAcross");

            yield return null;
        }
        transform.position = endPos;

        Debug.Log ("MoveAcrossEnd");

        navMeshAgent.enabled = true;
        navMeshAgent.updateRotation = true;
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        navMeshAgent.updateRotation = true;

        
        navMeshAgent.CompleteOffMeshLink();
        moveAcross = null;
    }


    public bool GetPossiblePath(Vector3 targetPos)
    {
        navMeshAgent.enabled = true;
        NavMeshPath navMeshPath = new NavMeshPath();

        navMeshAgent.CalculatePath(targetPos,navMeshPath);

        return navMeshPath.status == NavMeshPathStatus.PathComplete;
    }

    public void DisableNavMesh()
    {
        navMeshAgent.enabled = false;
    }

    public void EnableIdleAnim()
    {
        animator.SetBool("Grounded", true);
        animator.SetFloat("MotionSpeed", 1);
        animator.SetFloat("Speed", 0);
    }

    private Vector2 AssureMovement(Vector3 position, Vector2 input)
    {
        return OptimizeMovement(position, input);
    }

    private Vector2 OptimizeMovement(Vector3 position, Vector2 input)
    {
        //If Wall
        return CalculateOptimizedMovement(input, position);

    }

    private Vector2 CalculateOptimizedMovement(Vector2 movement, Vector3 position)
    {
        Vector3 characterMidpoint = characterController.center + transform.position;
        Vector3 characterExtents = new Vector3( characterController.radius * 2, 0.1f, characterController.radius * 2);
        RaycastHit hit;
        Physics.SphereCast(characterMidpoint, characterExtents.x, VectorHelper.Convert2To3(movement.normalized), out hit, 4f, LayerMask.GetMask("Walls"));
        //Physics.Linecast(position + Vector3.up, position + Vector3.up + VectorHelper.Convert2To3(movement.normalized) * 4, out hit, LayerMask.GetMask("Walls"));
        if (hit.collider != null)
        {
            Vector2 ray = VectorHelper.Convert3To2(hit.point - position);
            Vector3 dir = hit.point - position;
            Vector2 dir2 = VectorHelper.Convert3To2(dir);
            Vector3 normal = hit.normal;
            Vector2 normal2 = VectorHelper.Convert3To2(normal).normalized;

            float distance = ray.magnitude * Vector2.Dot(ray.normalized, normal2.normalized);
            distance = Mathf.Abs(distance);
            distance = Mathf.Clamp01(distance - minWallDistance);
            float influence = 1 - distance;

            float moveMagnitude = movement.magnitude;
            Vector2 cardinalDir = GetCardinalDirection(dir2, normal2);
            Vector2 cardinal = cardinalDir * moveMagnitude;

            Vector2 moveToCardinal = cardinal - movement;
            Vector2 resultdir = (movement + moveToCardinal * influence).normalized;
            Vector2 result = resultdir * moveMagnitude;

            return result;
        }

        return movement;
    }

    private Vector2 GetCardinalDirection(Vector2 dir2, Vector2 normal2)
    {
        normal2 = -normal2;
        float dotProduct = Vector3.Dot(dir2, normal2);
        float magnitudeSquared = normal2.sqrMagnitude;

        Vector2 proj = (dotProduct / magnitudeSquared) * normal2;

        return (dir2 - proj).normalized;
    }
    #endregion

    #region Traversing
    public void StartTraversing(Interactable crawl,TraversalType traversalType,float traversalDuration = 1)
    {
        string animationName="";
        if (traversalType == TraversalType.Crawl)
            animationName = "Crawl";
        else if (traversalType == TraversalType.JumpOver)
            animationName = "JumpOver";

        
        coroutine = StartCoroutine(Traverse(crawl.gameObject,traversalDuration,animationName));
    }



    private IEnumerator Traverse(GameObject traverseObject,float traverseDuration, string animationType)
    {
        float time = 0;
        Vector3 traverseDir = GetTraverseDir(traverseObject);

        //Lerp Rotation and Position
        Vector3 originPos = transform.position;
        Quaternion originRot = transform.rotation;

        Vector3 targetPos = new Vector3(traverseObject.transform.position.x, transform.position.y, traverseObject.transform.position.z) + -traverseDir * 1f;
        Quaternion targetRot = Quaternion.LookRotation(traverseDir);

        while (time < 0.1f)
        {
            transform.position = Vector3.Lerp(originPos, targetPos, time * 10);
            transform.rotation = Quaternion.Slerp(originRot, targetRot, time * 10);

            time += Time.deltaTime * Time.timeScale;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        
        //Start Traversing
        time = 0;
        animator.SetBool(animationType,true);
        animator.SetFloat("Speed",0);
        while (time < traverseDuration)
        {   
            transform.Translate(traverseDir * Time.timeScale * Time.deltaTime*movementSpeed / 10, Space.World);
            time += Time.deltaTime * Time.timeScale;

            yield return null;
        }

        animator.SetBool(animationType,false);
        coroutine = null;
    }

    private IEnumerator Traverse(Vector3 traverseDir,float traverseDuration)
    {
        float time = 0;

        //Lerp Rotation and Position
        Vector3 originPos = transform.position;
        Quaternion originRot = transform.rotation;

        Vector3 targetPos = transform.position + -traverseDir * 1f;
        Quaternion targetRot = Quaternion.LookRotation(traverseDir);

        while (time < 0.1f)
        {
            transform.position = Vector3.Lerp(originPos, targetPos, time * 10);
            transform.rotation = Quaternion.Slerp(originRot, targetRot, time * 10);

            time += Time.deltaTime * Time.timeScale;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        
        //Start Traversing
        time = 0;
        animator.SetFloat("Speed",1/traverseDuration);
        while (time < traverseDuration)
        {   
            transform.Translate(traverseDir * Time.timeScale * Time.deltaTime*movementSpeed / 10, Space.World);
            time += Time.deltaTime * Time.timeScale;

            yield return null;
        }
        coroutine = null;
    }

    private Vector3 GetTraverseDir(GameObject crawlObject)
    {
        Vector3 crawlPos = crawlObject.transform.position;
        Vector3 crawlDir = crawlObject.transform.forward;
        Vector3 relativePos = Vector3.Normalize(transform.position - crawlPos);

        float scalar = Vector3.Dot(relativePos, crawlDir) > 0 ? -1 : 1;
        crawlDir = new Vector3(crawlDir.x, 0, crawlDir.z).normalized;

        return crawlDir * scalar;
    }
    #endregion

    public void LerpPlayerTo(Transform target, bool keepYPos, float speed = 1)
    {
        if (lerpRoutine != null)
        {
            StopCoroutine(lerpRoutine);
        }
        lerpRoutine = StartCoroutine(LerpPlayer(target, keepYPos, speed));
    }

    public void StopLerp()
    {
        if (lerpRoutine != null)
        {
            StopCoroutine(lerpRoutine);
        }
    }

    private IEnumerator LerpPlayer(Transform targetTransform, bool keepYPos, float speed)
    {
        Vector3 postion = targetTransform.position;
        Quaternion rotation = targetTransform.rotation;
        Vector3 origin = transform.position;
        Quaternion originalRotation = transform.rotation;
        float timeElapsed = 0;

        while (timeElapsed <= 1)
        {
            postion = targetTransform.position;
            postion.y = keepYPos ? transform.position.y : postion.y;
            rotation = targetTransform.rotation;
            Vector3 targetPosition = Vector3.Lerp(origin, postion, timeElapsed);
            Quaternion targetRotation = Quaternion.Lerp(originalRotation, rotation, timeElapsed);

            transform.position = targetPosition;
            transform.rotation = targetRotation;

            timeElapsed += Time.deltaTime / speed;

            yield return null;
        }

        transform.position = postion;
        transform.rotation = rotation;

        lerpRoutine = null;
    }
}


