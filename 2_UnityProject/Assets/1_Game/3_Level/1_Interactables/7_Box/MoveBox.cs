using UnityEngine;

[RequireComponent(typeof(TriggerByCharacter))]
public class MoveBox : PlayerActionType
{
    [SerializeField] public Transform playerHandlePosition;
    [SerializeField] private LayerMask blockingLayers;

    [SerializeField] private float forwardSpeed = 1.0f;
    [SerializeField] private float backwardSpeed = 0.5f;

    private BoxCollider boxCollider;
    [HideInInspector]public Movement currentMover;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    public bool CheckIfBlocked(bool bForward, out float distance)
    {
        distance = 1;
        bool isBlocked = Physics.BoxCast(transform.position, boxCollider.size / 2, transform.forward * (bForward ? 1 : -1), out RaycastHit hit, Quaternion.identity, 2, blockingLayers,QueryTriggerInteraction.Ignore);

        if (isBlocked)
        {
            distance = hit.distance;
        }

        distance = Mathf.Clamp01(Mathf.Abs(distance));
        return isBlocked;
    }

    #region ObjectMove
    public float MoveWithObject(Vector2 moveVector)
    {
        float moveDirection = DetermineMoveDirection(moveVector);

        if (moveDirection == 0)
        {
            return 0;
        }

        bool moveForward = moveDirection > 0 ? true : false;

        float speed = moveForward ? backwardSpeed : forwardSpeed;

        if (CheckIfBlocked(moveForward, out float distance))
        {
            Move(moveDirection, distance, speed);
        }
        else
        {
            Move(moveDirection, distance, speed);
        }

        return moveDirection;
    }

    private void Move(float direction, float distance, float speed)
    {
        distance = Mathf.Clamp01(distance - (0.2f * (direction < 0 ? 1 : 3)));
        float moveDistance = Mathf.Clamp(direction * Time.deltaTime * speed, -distance, distance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    private float DetermineMoveDirection(Vector2 moveVector)
    {
        Vector2 objectForward = VectorHelper.Convert3To2(transform.forward).normalized;
        float scalar = Vector2.Dot(moveVector, objectForward);

        return scalar > 0 ? 1 : scalar < 0 ? -1 : 0;
    }
    #endregion
}