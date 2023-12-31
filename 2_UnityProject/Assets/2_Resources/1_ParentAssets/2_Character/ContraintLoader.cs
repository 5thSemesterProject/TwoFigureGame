using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

enum HeadState
{
    Idle,
    Move,
    Off,
}

[RequireComponent(typeof(MultiAimConstraint))]
public class ContraintLoader : MonoBehaviour
{
    public bool overrideOnlyIdle = true;

    [SerializeField] private string headToFind;
    [SerializeField] private float radius = 5;
    [SerializeField] private AnimationCurve weightCurveIdle = new AnimationCurve();
    [SerializeField] private AnimationCurve weightCurveMove = new AnimationCurve();
    [SerializeField] private bool debug;
    private MultiAimConstraint constraint;
    private Transform target;
    private Movement movement;

    #region Startup
    //Load values on Editor Change
    private void OnValidate()
    {
        LoadConstraint();
        movement = GetComponentInParent<Movement>();
    }

    private void Start()
    {
        LoadConstraint();
        movement = GetComponentInParent<Movement>();
    }
    #endregion

    private HeadState CheckState(Movement movement)
    {
        if (movement == null)
            return HeadState.Idle;

        CharacterData data;
        if (movement.characterType == CharacterType.Man)
            data = CharacterManager.manData;
        else
            data = CharacterManager.womanData;

        if (data == null)
            return HeadState.Idle;

        if (data.currentState.GetType() == typeof(IdleState))
            return HeadState.Idle;

        if (data.currentState.GetType() == typeof(MoveState))
            return HeadState.Move;

        return HeadState.Off;
    }

    private void LoadConstraint()
    {
        //Get constraint and target
        constraint = GetComponent<MultiAimConstraint>();
        GameObject targetGameobject = GameObject.Find(headToFind);

        if (targetGameobject != null)
        {
            target = targetGameobject.transform;
            WeightedTransformArray array = new WeightedTransformArray { new WeightedTransform(target, 1) };

            constraint.data.sourceObjects = array;
        }
        else
        {
            Debug.LogWarning($"Head -{headToFind}- not found!");
        }
    }

    private void Update()
    {
        if (target == null)
            return;

        if (overrideOnlyIdle)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            constraint.weight = Mathf.Clamp01(weightCurveIdle.Evaluate(Mathf.Clamp01(distance / radius)));
            return;
        }

        switch (CheckState(movement))
        {
            case HeadState.Idle:
                float distance = Vector3.Distance(transform.position, target.position);
                constraint.weight = Mathf.Clamp01(weightCurveIdle.Evaluate(Mathf.Clamp01(distance / radius)));
                return;
            case HeadState.Move:
                distance = Vector3.Distance(transform.position, target.position);
                constraint.weight = Mathf.Clamp01(weightCurveMove.Evaluate(Mathf.Clamp01(distance / radius)));
                return;
            case HeadState.Off:
                break;
        }

        constraint.weight = 0;
    }

    private void OnDrawGizmos()
    {
        if (debug && constraint != null && target != null)
        {
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.DrawLine(constraint.data.constrainedObject.position, target.position);
        }
    }
}
