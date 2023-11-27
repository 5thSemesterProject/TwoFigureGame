using UnityEngine;

public class MoveObject : Interactable
{
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        Debug.Log("MO");
    }
}
