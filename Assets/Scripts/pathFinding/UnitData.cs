using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Unit")]
public class UnitData : ScriptableObject
{
    public Team team;
    public UnitType unitType;
    public int HP;
    public int MP;
    public int attack;
    public float attackRange;
    public float castRange;
    public int defense;
    public int moveSpeed;
    public int level;
    public int experience;
    public int heavy;
}
