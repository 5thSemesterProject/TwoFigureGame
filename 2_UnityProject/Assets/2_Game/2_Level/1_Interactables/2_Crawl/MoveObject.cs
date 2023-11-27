using UnityEngine;

public class MoveObject : Interactable
{
    [SerializeField] private Vector3 characterPushPoint; 

    private Movement movement;

    public bool CheckIfBlocked(bool bForward, out float distance)
    {
        //Logic
        distance = 1;
        return false;
    }

    #region ObjectMove
    public void MoveWithObject(Vector2 moveVector)
    {
        float moveDirection = DetermineMoveDirection(moveVector);

        if (moveDirection == 0)
        {
            return;
        }

        bool moveForward = moveDirection > 0 ? true : false;

        if (CheckIfBlocked(moveForward, out float distance))
        {
            Move(moveDirection ,distance);
        }

        Move(moveDirection ,distance);
    }

    private void Move(float direction,float distance)
    {
        float moveDistance = Mathf.Clamp(direction * Time.deltaTime, -distance, distance);
        transform.Translate(Vector3.forward * moveDistance);
    }

    private float DetermineMoveDirection(Vector2 moveVector)
    {
        Vector2 objectForward = VectorHelper.Convert3To2(transform.forward);
        float scalar = Vector2.Dot(moveVector, objectForward);

        return scalar > 0 ? -1 : scalar < 0 ? 1 : 0;
    }
    #endregion

    protected override void Highlight()
    {
        base.Highlight();
    }
}