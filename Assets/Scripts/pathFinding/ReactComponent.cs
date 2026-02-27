using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactComponent : MonoBehaviour
{
    private Unit unit = null;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public void ApplyStun()
    {
        
    }

    public void ApplyKnockback()
    {
        
    }
}
