using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AIState : CharacterState
{
    public AIState(CharacterData data) : base(data)
    {
        if (characterData.virtualCamera != null)
            characterData.virtualCamera.gameObject.SetActive(false);

        characterData.movement.MovePlayer(Vector2.zero, 0);

        handleInteractables = false;
    }

    public override CharacterState SpecificStateUpdate()
    {
        if (CharacterManager.customInputMaps.InGame.Switch.triggered)
        {
            if (characterData.virtualCamera == null)
                CamManager.SpawnCamera(characterData.gameObject.transform, out characterData.virtualCamera);
            else
                characterData.virtualCamera.gameObject.SetActive(true);

            CustomEvents.RaiseCharacterSwitch(characterData.roomFadeRigidBody);

            characterData.movement.DisableNavMeshHandling();
            return new IdleState(characterData);
        }

        //Follow Partner
        Vector3 otherCharacterPos = characterData.other.gameObject.transform.position;
        characterData.movement.FollowPartner(otherCharacterPos);

        return this;
    }

}








