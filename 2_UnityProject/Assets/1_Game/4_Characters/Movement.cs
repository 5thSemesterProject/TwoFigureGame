using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public enum TraversalType
{
    Crawl, JumpOver
}

[RequireComponent(typeof (NavMeshHandler))]
public class Movement : MonoBehaviour, IIntersectGas
{
    private CharacterController characterController;
    private CharacterData characterData;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private NavMeshHandler navMeshHandler;

    public Coroutine traversalRoutine;
    public Coroutine lerpRoutine;

    //Movement
    private Coroutine moveRoutine;
    private Vector3 desiredMove = Vector3.zero;
    private Vector3 currentMove = Vector3.zero;
    private Vector3 previousMove;
    [SerializeField] private float acceleration = 10;
    [SerializeField] private float deceleration = 20;
    [SerializeField] private float maxSpeed = 4;

    [SerializeField] private float traverseDistance = 2;
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

        navMeshHandler = GetComponent<NavMeshHandler>();
    }

    private void Start()
    {
        characterData = characterType == CharacterType.Man ? CharacterManager.manData : CharacterManager.womanData;
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
            timeFalling += Time.deltaTime;
        else
            timeFalling = 0;

        //Change Hurt Value In Idle 
        float smoothHurtValue = Mathf.Lerp(animator.GetFloat("Hurt"), characterData.characterOxygenData.oxygenData.IsLow ? 1f : 0f, 0.005f);
        animator.SetFloat("Hurt", smoothHurtValue);
    }
    
    private IEnumerator _Move()
    {
        float tolerance = 0.001f;
        while (true)
        {
            while (Time.timeScale == 0)
            {
                yield return null;
            }

            //Apply Gravity
            if (timeFalling > 0)
            {
                float gravityFallDistance = gravity * timeFalling * timeFalling;
                characterController.Move(Vector3.down * gravityFallDistance);
            }


            if (currentMove.magnitude <= tolerance && desiredMove.magnitude <= tolerance &&  characterController.isGrounded)
            {
                //Move Zero
                currentMove = Vector3.zero;
                desiredMove = Vector3.zero;

                characterController.Move(Vector3.zero);
                animator.SetFloat("Speed", 0);

                moveRoutine = null;
                yield break;
            }

            //Adjust for Camera Rotation
            Transform cameraTransform = Camera.main.transform;
            Vector3 cameraForward = new(cameraTransform.forward.x, 0, cameraTransform.forward.z);
            Vector3 cameraRight = new(cameraTransform.right.x, 0, cameraTransform.right.z);
            desiredMove = (cameraForward * desiredMove.z + cameraRight * desiredMove.x).normalized * desiredMove.magnitude;

            //Calculate Deceleration
            if (desiredMove.magnitude <= 0)
            {
                currentMove -= Mathf.Min(currentMove.magnitude, deceleration * currentMove.magnitude / maxSpeed * Time.unscaledDeltaTime * Time.timeScale) * currentMove.normalized;
            }
            else if (currentMove.magnitude > 0)
                currentMove -= acceleration * Mathf.Pow((currentMove.magnitude / maxSpeed), 3) * Time.unscaledDeltaTime * Time.timeScale * currentMove.normalized;

            //Calculate Added Move
            float moveMagnitude = desiredMove.magnitude * Time.unscaledDeltaTime * Time.timeScale * acceleration;
            currentMove += desiredMove.normalized * moveMagnitude;

            //Move
            Vector3 previousPosition = transform.position;
            if (currentMove != null)
                characterController.Move(currentMove * Time.unscaledDeltaTime * Time.timeScale);
            Vector3 nextPosition = transform.position;
            currentMove = (nextPosition - previousPosition) / Time.unscaledDeltaTime * Time.timeScale;
            currentMove.y = 0;

            //Rotation Animation
            float angle = Vector3.Angle(previousMove, currentMove);

            //Rotate
            if (currentMove != null && currentMove != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(currentMove);

            previousMove = currentMove;

            desiredMove = Vector3.zero;

            //Animators
            animator.SetBool("Grounded", true);
            animator.SetFloat("MotionSpeed", 1);
            animator.SetFloat("Speed", currentMove.magnitude / Time.timeScale);
            animator.SetFloat("RotationAngle", angle);

            yield return null;
        }
    }

    public void ActivateMove()
    {
        if (moveRoutine == null)
            moveRoutine = StartCoroutine(_Move());
    }

    public void TerminateMove(bool resetMovementData = true)
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
        if (resetMovementData)
        {
            desiredMove = Vector3.zero;
            currentMove = Vector3.zero;
        }
    }

    public Vector2 MovePlayer(Vector2 axis, float speed = 1)
    {
        desiredMove += VectorHelper.Convert2To3(axis * speed);

        ActivateMove();
        return Vector2.Max(VectorHelper.Convert3To2(desiredMove), VectorHelper.Convert3To2(currentMove));

        /*
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
        */
    }

    public void FollowPartner(Vector3 otherCharacterPos)
    {
       navMeshHandler.FollowPartner(otherCharacterPos);
    }

    public void DisableNavMeshHandling()
    {
        navMeshHandler.DisableNavMesh();
    }

    /*
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
    */
    #endregion

    #region Traversing
    public void StartTraversing(Interactable traversable,TraversalType traversalType,float traversalDuration = 1)
    {
        string animationName="";
        if (traversalType == TraversalType.Crawl)
            animationName = "Crawl";
        else if (traversalType == TraversalType.JumpOver)
            animationName = "JumpOver";

        TerminateMove();

        traversalRoutine = StartCoroutine(Traverse(traversable.transform,traversalDuration,animationName));
    }



    private IEnumerator Traverse(Transform traverseObject,float traverseDuration, string animationType)
    {
        Vector3 traverseDir = GetTraverseDir(traverseObject);


        //Lerp To Start
        Vector3 targetPos = new Vector3(traverseObject.transform.position.x, transform.position.y, traverseObject.transform.position.z) + -traverseDir * 1f;
        Quaternion targetRot = Quaternion.LookRotation(traverseDir);
        yield return LerpPlayer(targetPos, targetRot, true, 0.1f);

        //Set Animations
        animator.SetBool(animationType, true);
        animator.SetFloat("Speed", 0);
        
        //Start Traversing
        yield return LerpPlayerAddative(traverseDir * traverseDistance, traverseDuration);

        //Reset Animations
        animator.SetBool(animationType,false);
        traversalRoutine = null;
    }

    private Vector3 GetTraverseDir(Transform traversable)
    {
        Vector3 crawlPos = traversable.position;
        Vector3 traverseDir = traversable.forward;
        Vector3 relativePos = Vector3.Normalize(transform.position - crawlPos);

        float scalar = Vector3.Dot(relativePos, traverseDir) > 0 ? -1 : 1;
        traverseDir = new Vector3(traverseDir.x, 0, traverseDir.z).normalized;

        return traverseDir * scalar;
    }
    #endregion

    #region Lerp Player
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
    private IEnumerator LerpPlayer(Vector3 targetPosition, Quaternion targetRotation, bool keepYPos, float speed)
    {
        Vector3 origin = transform.position;
        Quaternion originalRotation = transform.rotation;
        targetPosition.y = keepYPos ? transform.position.y : targetPosition.y;
        float timeElapsed = 0;

        while (timeElapsed <= 1)
        {
            Vector3 position = Vector3.Lerp(origin, targetPosition, timeElapsed);
            Quaternion rotation = Quaternion.Slerp(originalRotation, targetRotation, timeElapsed);

            transform.position = position;
            transform.rotation = rotation;

            timeElapsed += Time.deltaTime / speed;

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        lerpRoutine = null;
    }
    private IEnumerator LerpPlayerAddative(Vector3 addativePosition, Quaternion addativeRotation, float speed)
    {
        if (addativePosition.magnitude < 0.001f)
            yield break;

        Vector3 targetPosition = transform.position + addativePosition;
        Quaternion targetRotation = transform.rotation * addativeRotation;

        yield return LerpPlayer(targetPosition, targetRotation, false, speed);
    }
    private IEnumerator LerpPlayerAddative(Vector3 addativePosition, float speed, bool rotateToMoveForward = true)
    {
        if (addativePosition.magnitude < 0.001f)
            yield break;

        Vector3 targetPosition = transform.position + addativePosition;
        Quaternion targetRotation = rotateToMoveForward ? Quaternion.LookRotation(addativePosition) : transform.rotation;

        yield return LerpPlayer(targetPosition, targetRotation, false, speed);
    }
    #endregion
}


