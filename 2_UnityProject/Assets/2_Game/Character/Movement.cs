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
    private Vector2 previousMovement;

    [SerializeField] private float lerpValue = 0.2f;
    [SerializeField] private float movementSpeed = 0.5f;
    [SerializeField] private float rotationSpeed = 50f;
    private float minWallDistance = 0.7f;

    private void Awake()
    {
        if (!TryGetComponent(out characterController))
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.slopeLimit = characterController.stepOffset = 0;
        }
    }

    #region Movement
    public Vector2 MovePlayer(Vector2 axis, float speed = 1)
    {
        float yValue = transform.position.y;
        axis = axis.magnitude >= 1 ? axis.normalized : axis;
        axis = OptimizeMovement(transform.position, axis);
        axis = AssureMovement(transform.position, axis);

        previousMovement = Vector2.Lerp(previousMovement, axis, 1);
        Vector3 movementDir = (new Vector3(previousMovement.x, 0, previousMovement.y));
        Vector3 movement = movementDir * movementSpeed * speed / 10 * Time.deltaTime * Time.timeScale * 5;
        characterController.Move(movement);

        if (movement.magnitude >= 0.001)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementDir.normalized);
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

        return axis;
    }

    private Vector2 AssureMovement(Vector3 position, Vector2 input)
    {
        if (input.x == 1 || input.y == 1 || input.x == -1 || input.y == -1)
        {
            return OptimizeMovement(position, input);
        }

        return input;
    }

    private Vector2 OptimizeMovement(Vector3 position, Vector2 input)
    {
        float speedMag = input.magnitude;
        Vector3 moveDir = new Vector3(input.x, 0, input.y).normalized;

        //If Wall
        return CalculateOptimizedMovement(input, position);

    }

    private Vector2 CalculateOptimizedMovement(Vector2 movement, Vector3 position)
    {
        Vector3 RaycastEnd = position + Vector3.up + Vector2ToVector3(movement.normalized) * 4;

        RaycastHit hit;
        Physics.Linecast(position + Vector3.up, position + Vector3.up + Vector2ToVector3(movement.normalized) * 4, out hit, LayerMask.GetMask("Walls"));
        if (hit.collider != null)
        {
            Vector2 ray = Vector3ToVector2(hit.point - position);
            Vector3 dir = hit.point - position;
            Vector2 dir2 = Vector3ToVector2(dir);
            Vector3 normal = hit.normal;
            Vector2 normal2 = Vector3ToVector2(normal).normalized;

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

    #region Conversion
    private Vector2 Vector3ToVector2(Vector3 value)
    {
        return new(value.x, value.z);
    }
    private Vector3 Vector2ToVector3(Vector2 value)
    {
        return new(value.x, 0, value.y);
    }
    #endregion
}
