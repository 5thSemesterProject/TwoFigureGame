using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorHelper

{
    public static Vector4  Convert3To4(Vector3 vector3, float w)
    {
        return new Vector4(vector3.x,vector3.y,vector3.z,w);
    }

}
