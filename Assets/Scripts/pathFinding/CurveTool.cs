using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveTool
{
    public static Vector3 Bezier2(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }
}
