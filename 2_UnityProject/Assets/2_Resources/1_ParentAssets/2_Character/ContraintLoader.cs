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

    [SerializeField] private string targetToFind;
    [SerializeField] private float radius = 5;
    [SerializeField] private AnimationCurve weightCurveIdle = new AnimationCurve();
    [SerializeField] private AnimationCurve weightCurveMove = new AnimationCurve();
    [SerializeField] private bool debug;
    [SerializeField] private Animator animator;
    private MultiAimConstraint constraint;
    private Transform target;
    private float targetWeight = 0;
    private Coroutine weightLerpRoutine;

    #region Startup
    //Load values on Editor Change
    private void OnValidate()
    {
        LoadConstraint();
        if (animator == null)
        {
            Animator[] temp = GetComponentsInParent<Animator>();
            if (temp.Length >= 2)
            {
                animator = temp[1];
            }
        }
    }

    private void Start()
    {
        LoadConstraint();
        if (animator == null)
        {
            Animator[] temp = GetComponentsInParent<Animator>();
            if (temp.Length >= 2)
            {
                animator = temp[1];
            }
        }

        SetTarget(0);
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
        GameObject targetGameobject = GameObject.Find(targetToFind);

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
            Debug.LogWarning($"Head -{targetToFind}- not found!");
        }
    }

    private void Update()
    {
        if (target == null)
            return;

        if (overrideOnlyIdle)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            SetTarget(weightCurveIdle.Evaluate(Mathf.Clamp01(distance / radius)));
            return;
        }

        switch (CheckState(animator))
        {
            case HeadState.Idle:
                float distance = Vector3.Distance(transform.position, target.position);
                SetTarget(weightCurveIdle.Evaluate(Mathf.Clamp01(distance / radius)));
                return;
            case HeadState.Move:
                distance = Vector3.Distance(transform.position, target.position);
                SetTarget(weightCurveMove.Evaluate(Mathf.Clamp01(distance / radius)));
                return;
            case HeadState.Off:
                break;
        }

        constraint.weight = 0;
    }

    private void SetTarget(float target)
    {
        targetWeight = Mathf.Clamp01(target);

        if (weightLerpRoutine == null)
            weightLerpRoutine = StartCoroutine(LerpWeight());
    }

    private IEnumerator LerpWeight()
    {
        if (constraint == null)
            yield break;

        while (true)
        {
            float velocity = 0;
            constraint.weight = Mathf.SmoothDamp(constraint.weight, targetWeight, ref velocity, 0.1f);

            if (Mathf.Abs(constraint.weight - targetWeight) <= 0.05f )
            {
                constraint.weight = targetWeight;
                weightLerpRoutine = null;
                yield break;
            }

            yield return null;
        }
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
