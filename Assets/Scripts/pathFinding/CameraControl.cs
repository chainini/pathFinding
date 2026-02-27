using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Tooltip("Åö×²°ë¾¶")]
    public float coliRadius = 0.1f;


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        Gizmos.DrawWireSphere(transform.position, coliRadius);
    }
}
