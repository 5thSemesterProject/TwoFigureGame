using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBlockLogic : MonoBehaviour
{
    [SerializeField] private Transform renderCamera;

    private void Start()
    {
        renderCamera = GetComponentInChildren<Camera>().transform;
    }
    // Update is called once per frame
    void LateUpdate()
    {
        renderCamera.position = Camera.main.transform.position;
        renderCamera.rotation = Camera.main.transform.rotation;
    }
}
