using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(BoxCollider))]
public class ExcludeRectangle : MonoBehaviour
{
    Vector3 scale;
    public Vector4 GetRectangleData()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        Vector2 rectangleCenter = VectorHelper.Convert3To2(transform.position+boxCollider.center);
        Vector3 boxColliderSize = boxCollider.size;
        scale = new Vector3(
        boxColliderSize.x * transform.localScale.x,
        boxColliderSize.y * transform.localScale.y,
        boxColliderSize.z * transform.localScale.z
        );
        float width = scale.x;
        float height = scale.z;
        return new Vector4(rectangleCenter.x,rectangleCenter.y,width,height);
    }
}
