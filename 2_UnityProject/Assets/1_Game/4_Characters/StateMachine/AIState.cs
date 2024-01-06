using UnityEngine;
using UnityEngine.AI;

class AIState : CharacterState
{
    private BoxCollider collider;

    public AIState(CharacterData data) : base(data)
    {
        if (characterData.virtualCamera != null)
            characterData.virtualCamera.gameObject.SetActive(false);

        characterData.movement.MaxSpeed *= GameStats.instance.AISpeedMultiplier;

        characterData.audioListener.enabled = false;

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
            GameObject.Destroy(collider);
            collider = null;

            characterData.movement.GetComponent<CharacterController>().enabled = false;
            characterData.movement.GetComponent<NavMeshAgent>().enabled = true;

            characterData.movement.MaxSpeed /= GameStats.instance.AISpeedMultiplier;

            return new IdleState(characterData);
        }

        //Follow Partner
        if (movement.interactable !=null && movement.interactable.TryGetComponent(out PressurePlate pressurePlate)) //Don't follow partner if on pressure plate
        {
            movement.GetComponent<NavMeshHandler>().IdleAnim();
        }
        else
        {
            movement.interactable = null;
            Vector3 otherCharacterPos = characterData.other.gameObject.transform.position;
            characterData.movement.FollowPartner(otherCharacterPos);
        }

        //Stop Handling Oxygen if other character is in GodMode
        if (characterData.other.currentState  is GodModeState)
            handleOxygen = false;
        else
            handleOxygen = true;

        return this;
    }
}








