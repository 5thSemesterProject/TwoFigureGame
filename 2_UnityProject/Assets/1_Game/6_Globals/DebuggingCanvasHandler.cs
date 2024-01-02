using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuggingCanvasHandler : MonoBehaviour
{
#if UNITY_EDITOR
    bool debuggingActive = false;

        void  Update()
        {
            
            if (Input.GetKey(KeyCode.LeftShift))
            {
                debuggingActive = true;
                GetComponent<Canvas>().enabled = true;
            }
            else
            {
                debuggingActive = false;
                GetComponent<Canvas>().enabled = false;
            }

        }
#endif

}
