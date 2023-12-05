using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUI : MonoBehaviour
{

   static InGameUI instance;

   void Awake()
   {
         if (instance==null)
            instance = this;
        else if (instance!=this)
            Destroy(instance);
   }


}
