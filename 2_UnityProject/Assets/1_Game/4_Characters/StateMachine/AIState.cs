using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

class AIState : CharacterState
{
    private BoxCollider collider;

    public AIState(CharacterData data) : base(data)
    {
        if (characterData.virtualCamera != null)
            characterData.virtualCamera.gameObject.SetActive(false);

        characterData.movement.MovePlayer(Vector2.zero, 0);

        handleInteractables = false;

        collider = characterData.gameObject.AddComponent<BoxCollider>();
        collider.size = Vector3.one*0.1f;
    }

    public override CharacterState SpecificStateUpdate()
    {
        Movement movement = characterData.movement;

        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
        {
            if (characterData.virtualCamera == null)
                CamManager.SpawnCamera(characterData.gameObject.transform, out characterData.virtualCamera);
            else
                characterData.virtualCamera.gameObject.SetActive(true);

            CustomEvents.RaiseCharacterSwitch(characterData.roomFadeRigidBody);

            movement.DisableNavMeshHandling();
            GameObject.Destroy(collider);
            collider = null;

            return new IdleState(characterData);
        }

        //Follow Partner
        if (movement.interactable !=null && movement.interactable.TryGetComponent(out PressurePlate pressurePlate)) //Don't follow partner if on pressure plate
        {
            //movement.DisableNavMeshHandling();
            movement.MovePlayer(Vector2.zero,0);
        }
        else if (movement.interactable == null)
        {
            Vector3 otherCharacterPos = characterData.other.gameObject.transform.position;
            characterData.movement.FollowPartner(otherCharacterPos);
        }


        return this;
    }

}








