using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorHelper

{
    public static Vector4 Convert3To4(Vector3 vector3, float w)
    {
        return new Vector4(vector3.x,vector3.y,vector3.z,w);
    }

    public static Vector2 Convert3To2(Vector3 value)
    {
        return new(value.x, value.z);
    }
    public static Vector3 Convert2To3(Vector2 value)
    {
        return new(value.x, 0, value.y);
    }

    public static Vector3 FromTo(Vector3 start, Vector3 end)
    {
        return (end - start).normalized;
    }

}
