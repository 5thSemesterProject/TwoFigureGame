using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.Rendering.DebugUI;

public class Movement : MonoBehaviour
{
    private CharacterController characterController;
    private Animator animator;

    [SerializeField] private float lerpValue = 0.2f;
    [SerializeField] private float movementSpeed = 25f;
    [SerializeField] private float rotationSpeed = 50f;
    private float minWallDistance = 0.7f;

    public Interactable interactable;

    private void Awake()
    {
        if (!TryGetComponent(out characterController))
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.slopeLimit = characterController.stepOffset = 0;
        }

        if (!TryGetComponent(out animator))
        {
            Debug.LogWarning("No animator found on the character!");
        }
    }

    public Vector4 GetSphereInformation()
    {
        return VectorHelper.Convert3To4(transform.position,2);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    #region Movement

    public Vector2 MovePlayer(Vector2 axis, float speed = 1)
    {
        float yValue = transform.position.y;
        axis = axis.magnitude >= 1 ? axis.normalized : axis;

        Vector3 movementDir = VectorHelper.Convert2To3(axis);
        Vector3 movement = movementDir * movementSpeed * speed * Time.deltaTime * Time.timeScale / 3;
        movement = VectorHelper.Convert2To3(OptimizeMovement(transform.position, VectorHelper.Convert3To2(movement)));
        movement = VectorHelper.Convert2To3(AssureMovement(transform.position, VectorHelper.Convert3To2(movement)));
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
        transform.position = new Vector3(transform.position.x, yValue, transform.position.z);

        float animationSpeed = movementDir.magnitude * 3;
        animator.SetBool("Grounded", true);
        animator.SetFloat("MotionSpeed", 1);
        animator.SetFloat("Speed", movement.magnitude / Time.deltaTime * 3);
        return VectorHelper.Convert3To2(movement);
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
}
