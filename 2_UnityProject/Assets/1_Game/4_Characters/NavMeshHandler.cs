using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent (typeof(CharacterController),typeof(NavMeshAgent))]
public class NavMeshHandler : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    CharacterController characterController;

    Coroutine movingAcrossOffMeshLink;

    Animator animator;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    public void FollowPartner(Vector3 otherCharacterPos)
    {
        //Follow Character in case out of range
        if (GetMovementRequired(otherCharacterPos))
        {
            MovePlayerToPos(otherCharacterPos);
        }
        //Stop in case in range    
        else if (movingAcrossOffMeshLink==null)
        {
            DisableNavMesh();
            IdleAnim();
        }
    }

    public void MovePlayerToPos(Vector3 position,float movementSpeed=1,bool noStoppingDistance = false)
    {
        navMeshAgent.enabled = true;
        navMeshAgent.autoTraverseOffMeshLink = false;
        characterController.enabled = false;

        if (noStoppingDistance)
            navMeshAgent.stoppingDistance = 0.1f;

        if (position!=navMeshAgent.destination && movingAcrossOffMeshLink==null)
        {
            navMeshAgent.SetDestination(position);
            animator.SetBool("Grounded", true);
            animator.SetFloat("MotionSpeed", 1);
            animator.SetFloat("Speed", navMeshAgent.velocity.magnitude / Time.deltaTime * 3f);
        }

        characterController.enabled = true;


        if (navMeshAgent.isOnOffMeshLink)
        {
            if (movingAcrossOffMeshLink==null)
            {
               movingAcrossOffMeshLink = StartCoroutine(MoveAcrossNavMeshLink(movementSpeed));
            }      
        }

    }

    public bool GetMovementRequired(Vector3 targetPos)
    {
        navMeshAgent.enabled = true;
        NavMeshPath navMeshPath = new NavMeshPath();

        navMeshAgent.CalculatePath(targetPos,navMeshPath);

        return navMeshPath.status == NavMeshPathStatus.PathComplete && Vector3.Distance(targetPos,gameObject.transform.position)>GameStats.instance.inactiveFollowDistance;
    }

    IEnumerator MoveAcrossNavMeshLink(float movementSpeed)
    {
        OffMeshLinkData data = navMeshAgent.currentOffMeshLinkData;

        Vector3 startPos = navMeshAgent.transform.position;
        Vector3 endPos = data.endPos + Vector3.up * navMeshAgent.baseOffset;

        //ACalulate veloctiy
        float navMeshAgentVelocity = navMeshAgent.velocity.magnitude;
        float duration = (endPos-startPos).magnitude/(navMeshAgentVelocity>2.5f?navMeshAgentVelocity:movementSpeed* 2.5f);

        //Caccluate other characters rotation
        //Vector3 otherCharacterPosition = Vector3.zero;
       // transform.rotation  = Quaternion.Euler(startPos-endPo);

        float t = 0.0f;
        float tStep = 1.0f/duration;
        tStep = 1*tStep;
    

        while(t<1.0f){
            transform.position = Vector3.Lerp(startPos,endPos,t);
            
            if (navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                navMeshAgent.destination = transform.position;
            
            t+=tStep*Time.deltaTime;

            //Adjustanimation
            animator.SetFloat("Speed", tStep / Time.deltaTime * 2f);


            yield return null;
        }
        transform.position = endPos;

        navMeshAgent.CompleteOffMeshLink();
        movingAcrossOffMeshLink = null;
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
    }


}
