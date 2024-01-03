using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class PlayerActionType : MonoBehaviour
{
   public Transform animationStartpoint;

   public bool TryGetAnimationStartPoint(out Transform animationStartPoint)
   {
        animationStartPoint = this.animationStartpoint;
        return animationStartPoint!=null;
   }
}
