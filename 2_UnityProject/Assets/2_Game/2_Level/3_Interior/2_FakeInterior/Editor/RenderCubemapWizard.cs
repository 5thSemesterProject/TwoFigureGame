using UnityEngine;
using UnityEditor;
using System.Collections;
using Unity.VisualScripting;

public class RenderCubemapWizard : ScriptableWizard
{
    public Transform renderFromPosition;
    public Cubemap cubemap;
    public Camera camera;



    void OnWizardUpdate()
    {
        string helpString = "Select transform to render from and cubemap to render into";
        bool isValid = (renderFromPosition != null) && (cubemap != null);
    }

    void OnWizardCreate()
    {
        GameObject go;
        if (camera==null)
        {
                    // create temporary camera for rendering
                    go = new GameObject("CubemapCamera");
                    go.AddComponent<Camera>();
        }
        else
            go = GameObject.Instantiate(camera.gameObject);

        // place it on the object
        go.transform.position = renderFromPosition.position;
        go.transform.rotation = renderFromPosition.rotation;
        // render into cubemap
        go.GetComponent<Camera>().RenderToCubemap(cubemap);

        // destroy temporary camera
        DestroyImmediate(go);
    }

    [MenuItem("GameObject/Render into Cubemap")]
    static void RenderCubemap()
    {
        ScriptableWizard.DisplayWizard<RenderCubemapWizard>(
            "Render cubemap", "Render");
    }
}