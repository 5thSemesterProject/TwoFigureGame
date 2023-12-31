using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[ExecuteInEditMode]
[RequireComponent(typeof(MultiAimConstraint))]
public class ContraintLoader : MonoBehaviour
{
    [SerializeField] private string headToFind;
    [SerializeField] private float radius = 5;
    [SerializeField] private AnimationCurve weightCurve;
    [SerializeField] private bool debug;
    private MultiAimConstraint constraint;
    private Transform target;

    private void OnValidate()
    {
        LoadConstraint();
    }

    private void Awake()
    {
        LoadConstraint();
    }

    private void LoadConstraint()
    {
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
        float distance = Vector3.Distance(transform.position, target.position);
        constraint.weight = Mathf.Clamp01(weightCurve.Evaluate(Mathf.Clamp01(distance / radius)));
    }

    private void OnDrawGizmos()
    {
        if (debug && constraint != null)
        {
            Gizmos.DrawWireSphere(transform.position, radius);
            Gizmos.DrawLine(constraint.data.constrainedObject.position, target.position);
        }
    }
}
