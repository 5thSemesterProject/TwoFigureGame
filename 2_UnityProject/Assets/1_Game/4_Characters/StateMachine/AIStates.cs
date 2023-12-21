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

            characterData.movement.DisableNavMesh();
            return new IdleState(characterData);
        }

        //Follow Partner
        FollowPartner();

        return this;
    }

     void FollowPartner()
    {   
       Vector3 otherCharacterPos = characterData.other.gameObject.transform.position;
        Vector3 pos = characterData.gameObject.transform.position;

        //Follow Character in case out of range
        if (characterData.movement.GetPossiblePath(otherCharacterPos) && Vector3.Distance(otherCharacterPos,pos)>GameStats.instance.inactiveFollowDistance)
        {
            characterData.movement.MovePlayerToPos(otherCharacterPos);
        }
        //Stop in case in range    
        else if (characterData.movement.moveAcross==null)
        {
            characterData.movement.DisableNavMesh();
            characterData.movement.EnableIdleAnim();
        }
    }
}








