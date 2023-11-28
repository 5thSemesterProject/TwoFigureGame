using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public enum ActivationMode
{
    Player, Box
}

public class PressurePlate : Interactable
{
    [SerializeField] float scaleColliderFactor = 0.8f;
    [SerializeField] int maxColliders = 100;
    [SerializeField] GameObject button;
    [SerializeField] float pressAmount = 0.05f;
    [SerializeField] float pressSpeed = 0.1f;
    [SerializeField] ActivationMode activationMode;

    Vector3 initialButtonPos;

    Vector3 colliderSize;
    Collider[] hitColliders;

    Vector3 targetPos;
    Coroutine coroutine;

    bool pressed;

    void Start()
    {
        colliderSize = transform.localScale * scaleColliderFactor;
        colliderSize.y = 2;
        hitColliders = new Collider[maxColliders];
        coroutine  = StartCoroutine(PressDownAnim());
        initialButtonPos = button.transform.position;
    }

    IEnumerator PressDownAnim()
    {
        targetPos = button.transform.position;
        float t = 0;

        while (true)
        {
            Vector3 curentPos = Vector3.Lerp(button.transform.position,targetPos,t);
            button.transform.position = curentPos;
            t+=Time.deltaTime*pressSpeed;
            yield return null;
        }
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
        if (SteppedOn() && pressed==false)
        {
            targetPos = initialButtonPos+Vector3.down*pressAmount;
            pressed = true;
            Trigger();
        }
        else if (!SteppedOn()&&pressed==true)
        {
             targetPos = initialButtonPos;
             Untrigger();
             pressed = false;
        }
            
    }

    bool CheckAccess(Collider collider)
    {
        if (collider!=null)
        {
            if (collider.gameObject.TryGetComponent(out Movement movement)&&activationMode == ActivationMode.Player)
                return true;
            else if (collider.gameObject.TryGetComponent(out MoveBox moveObject)&&activationMode == ActivationMode.Box)
                return true;
        }
        return false;
    }

    public bool SteppedOn()
    {   
        System.Array.Clear(hitColliders, 0, hitColliders.Length);
        Physics.OverlapBoxNonAlloc(transform.position+Vector3.up*colliderSize.y/2,colliderSize, hitColliders, Quaternion.identity);

        for (int i = 0; i < hitColliders.Length; i++)
        {  
            //Check if player is in collider
            if (CheckAccess(hitColliders[i]))
                return true;
        }
        
        return false;
    }


    private void Update()
    {
        CheckActivation();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position+Vector3.up*colliderSize.y/2, colliderSize);
    }

}


