using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

[RequireComponent (typeof(NavMeshAgent))]
public class NavMeshHandler : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    CharacterController characterController;
    Animator animator;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    public void FollowPartner(Vector3 otherCharacterPos)
    {
        //Check Oxygenstation due to collider mess
        Oxygenstation oxygenstation = GetComponent<Movement>().oxygenstation;
        if (oxygenstation)
            CheckOxygenstation(oxygenstation);

        //Follow Character in case out of range
        if (GetMovementRequired(otherCharacterPos))
        {
            MovePlayerToPos(otherCharacterPos);
        }
        else
        {
            IdleAnim();
        }
    }

    public bool GetMovementRequired(Vector3 targetPos)
    {
        return CheckReachable(targetPos) && Vector3.Distance(targetPos,gameObject.transform.position)>GameStats.instance.inactiveFollowDistance;
    }

    public void MovePlayerToPos(Vector3 position,float movementSpeed=1,bool noStoppingDistance = false, bool updateRotation = true)
    {
        navMeshAgent.updateRotation = false;
        characterController.enabled = false;
        navMeshAgent.isStopped = false;

        NavMeshPath navMeshPath = new NavMeshPath();
        navMeshAgent.CalculatePath(position, navMeshPath);
        foreach (var corner in navMeshPath.corners)
        {
            Debug.DrawLine(corner,corner + Vector3.up, Color.red);
        }
        if (navMeshPath.corners.Length<=2)
        {
            Debug.DrawLine(transform.position + Vector3.up, (position) + Vector3.up, Color.yellow);
            Vector3 movementDirection = (position - transform.position).normalized;
            GetComponent<Movement>().MovePlayerGlobal(movementDirection);
        }
        else
        {
            if ((navMeshPath.corners[1] - transform.position).magnitude <= 0.5f)
            {
                Debug.DrawLine(transform.position + Vector3.up, (navMeshPath.corners[2]) + Vector3.up, Color.yellow);
                Vector3 movementDirection = (navMeshPath.corners[2] - transform.position).normalized;
                GetComponent<Movement>().MovePlayerGlobal(movementDirection);
            }
            else
            {
                Debug.DrawLine(transform.position + Vector3.up, (navMeshPath.corners[1]) + Vector3.up, Color.yellow);
                Vector3 movementDirection = (navMeshPath.corners[1] - transform.position).normalized;
                GetComponent<Movement>().MovePlayerGlobal(movementDirection);
            }
        }
    }

    public void DisableNavMesh()
    {
        //navMeshAgent.enabled = false;
    }

    public void IdleAnim()
    {
        animator.SetBool("Grounded", true);
        animator.SetFloat("MotionSpeed", 1);
        animator.SetFloat("Speed", 0);
        if (navMeshAgent.isOnNavMesh)
            navMeshAgent.isStopped = true;
    }


    public void CheckOxygenstation(Oxygenstation oxygenstation)
    {
        float distance = Vector3.Distance(transform.position,oxygenstation.transform.position);

        if (distance<=oxygenstation.GetIntersectionRadius())
        {
            Debug.Log ("In Range with "+distance+" Range was "+oxygenstation.GetIntersectionRadius());
        }
        else
        {
            Debug.Log ("Out of range with "+distance+" Range was "+oxygenstation.GetIntersectionRadius());
            GetComponent<Movement>().oxygenstation = null;
        }
    }

    public bool CheckReachable(Vector3 targetPos)
    {   
        bool currentNavmeshAgentState = navMeshAgent.enabled;
        navMeshAgent.enabled = true;

        NavMeshPath navMeshPath = new NavMeshPath();
        navMeshAgent.CalculatePath(targetPos,navMeshPath);
        navMeshAgent.enabled = currentNavmeshAgentState;
        
        return navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete;
    }

}
