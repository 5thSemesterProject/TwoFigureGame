using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class PressurePlate : Interactable
{
    [SerializeField] float scaleColliderFactor = 0.8f;
    [SerializeField] int maxColliders = 4;
    Vector3 colliderSize;
    Collider[] hitColliders;

    void Start()
    {
        colliderSize = transform.localScale * scaleColliderFactor;
        colliderSize.y = 2;
        hitColliders = new Collider[maxColliders];
    }
    protected override void Highlight()
    {
        Debug.Log("Highlighted");
    }

    private void OnValidate()
    {
        colliderSize = transform.localScale * scaleColliderFactor;
        colliderSize.y = 2;
    }

    void CheckActivation()
    {
        if (SteppedOn())
        {
            GetComponent<Renderer>().materials[0].SetColor("_BaseColor", Color.red);
            Trigger();
        }
        else
            GetComponent<Renderer>().materials[0].SetColor("_BaseColor", Color.white);
    }

    public bool SteppedOn()
    {   
        System.Array.Clear(hitColliders, 0, hitColliders.Length);
        Physics.OverlapBoxNonAlloc(transform.position,colliderSize, hitColliders, Quaternion.identity);

        for (int i = 0; i < hitColliders.Length; i++)
        {   
            //Check if player is in collider
            if (hitColliders[i] != null && hitColliders[i].gameObject.TryGetComponent(out Movement movement))
            {
                return true;
            }
        }
        
        return false;
    }


    private void Update()
    {
        CheckActivation();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, colliderSize);
    }

}


