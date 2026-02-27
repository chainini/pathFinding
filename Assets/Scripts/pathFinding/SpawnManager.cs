using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class SpawnManager : Singleton<SpawnManager>
{
    public UnitRegistry registry;

    protected override void Awake()
    {
        base.Awake();
        
        registry.Build();
    }

    public Unit SpawnUnit(Vector3 position,Team team, UnitType unitType , Quaternion? rotation, [CanBeNull] Transform parent)
    {
        return null;
    }
}
