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
    [SerializeField] private Animator animator;
    private MultiAimConstraint constraint;
    private Transform target;

    #region Startup
    //Load values on Editor Change
    private void OnValidate()
    {
        LoadConstraint();
        if (animator == null)
            animator = GetComponentsInParent<Animator>()[1];
    }

    private void Start()
    {
        LoadConstraint();
        if (animator == null)
            animator = GetComponentsInParent<Animator>()[1];
    }
    #endregion

    private HeadState CheckState(Animator animator)
    {
        if (animator == null)
            return HeadState.Idle;

        if (animator.GetBool("JumpOver") == true ||
            animator.GetBool("Crawl") == true ||
            animator.GetBool("MoveBox") == true ||
            animator.GetBool("Dead") == true)
            return HeadState.Off;

        if (animator.GetFloat("Speed") == 0)
            return HeadState.Idle;

        if (animator.GetFloat("Speed") != 0)
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

            RigBuilder rigBuilder = GetComponentInParent<RigBuilder>();
            if (rigBuilder != null)
                rigBuilder.Build();
            else
                Debug.LogWarning("Rigbuilder not found");
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

        switch (CheckState(animator))
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
            if (constraint.data.constrainedObject != null)
                Gizmos.DrawLine(constraint.data.constrainedObject.position, target.position);
        }
    }
}
