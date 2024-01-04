using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

[RequireComponent (typeof(CharacterController),typeof(NavMeshAgent))]
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
        navMeshAgent.enabled = true;

        //Check Oxygenstation due to collider mess
        Oxygenstation oxygenstation = GetComponent<Movement>().oxygenstation;
        if (oxygenstation)
            CheckOxygenstation(oxygenstation);

        //Follow Character in case out of range
        if (GetMovementRequired(otherCharacterPos))
        {
            MovePlayerToPos(otherCharacterPos);
        }
        //Stop in case in range    
        else
        {
            DisableNavMesh();
            IdleAnim();
        }
    }

    public bool GetMovementRequired(Vector3 targetPos)
    {
        NavMeshPath navMeshPath = new NavMeshPath();
        navMeshAgent.CalculatePath(targetPos,navMeshPath);

        return navMeshPath.status == NavMeshPathStatus.PathComplete && Vector3.Distance(targetPos,gameObject.transform.position)>GameStats.instance.inactiveFollowDistance;
    }

    private Vector2 previousMove;
    public void MovePlayerToPos(Vector3 position,float movementSpeed=1,bool noStoppingDistance = false, bool updateRotation = true)
    {
        navMeshAgent.updateRotation = false;
        characterController.enabled = false;
        navMeshAgent.isStopped = false;

        if (noStoppingDistance)
            navMeshAgent.stoppingDistance = 0.1f;

        if (position!=navMeshAgent.destination)
        {
            navMeshAgent.SetDestination(position);
            animator.SetBool("Grounded", true);
            animator.SetFloat("MotionSpeed", 1);
            animator.SetFloat("Speed", navMeshAgent.velocity.magnitude+1);

            //Set Rotation
            if(navMeshAgent.velocity.magnitude>0.01f)
            {   
                if (updateRotation)
                    transform.rotation = Quaternion.LookRotation(navMeshAgent.velocity);
            
                //Caclulate Movement Angle
                Vector2 currentMove = VectorHelper.Convert3To2(transform.forward).normalized;
                float angle = Vector2.Angle(currentMove,previousMove);
                previousMove = currentMove;
                animator.SetFloat("RotationAngle", angle);
            }
        }
    }

    public void DisableNavMesh()
    {
        navMeshAgent.enabled = false;
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

}
