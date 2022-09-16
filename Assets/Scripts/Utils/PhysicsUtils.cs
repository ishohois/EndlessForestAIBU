using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicsUtils
{
    public static Vector3 CalculateNormalForce(Vector3 velocity, Vector3 normal)
    {
        float dotProduct;

        if ((dotProduct = Vector3.Dot(velocity, normal)) < 0.0f)
        {
            return -(dotProduct * normal);
        }
        return Vector3.zero;
    }
}
