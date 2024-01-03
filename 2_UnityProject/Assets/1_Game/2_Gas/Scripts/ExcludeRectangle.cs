using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ExcludeRectangle : MonoBehaviour
{
    Vector3 scale;
    public Vector4 GetRectangleData()
    {
        Vector2 rectangleCenter = VectorHelper.Convert3To2(transform.position);
        scale = GetComponent<BoxCollider>().size;
        float width = transform.localScale.x;
        float height = transform.localScale.z;
        return new Vector4(rectangleCenter.x,rectangleCenter.y,width,height);
    }
}
