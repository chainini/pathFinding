using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDataComponent : MonoBehaviour
{
    [SerializeField]private Team team;

    public Team Team
    {
        get{return team;}
        set{team = value;}
    }
    
    
    [SerializeField] private int hp;
    public int HP
    {
        get
        {
            return hp;
        }

        set
        {
            hp = value;
        }
    }
    [SerializeField] private int mp;

    public int MP
    {
        get
        {
            return mp;
        }
        set
        {
            mp = value;
        }
    }
    [SerializeField] private int attack;

    public int Attack
    {
        get
        {
            return attack;
        }
        set
        {
            attack = value;
        }
    }
    [SerializeField] private int defence;

    public int Defence
    {
        get
        {
            return defence;
        }
        set
        {
            defence = value;
        }
    }
    [SerializeField] private int moveSpeed;

    public int MoveSpeed
    {
        get
        {
            return moveSpeed;
        }
        set
        {
            moveSpeed = value;
        }
    }
    [SerializeField] private int level;

    public int Level
    {
        get
        {
            return level;
        }
        set
        {
            level = value;
        }
    }
    [SerializeField] private int experience;

    public int Experience
    {
        get
        {
            return experience;
        }
        set
        {
            experience = value;
        }
    }
    
    [SerializeField] private int heavy;

    public int Heavy
    {
        get
        {
            return heavy;
        }
        set
        {
            heavy = value;
        }
    }
    
    [SerializeField] private float attackRange;
    public float AttackRange
    {
        get
        {
            return attackRange;
        }
        set
        {
            attackRange = value;
        }
    }
    
    [SerializeField] private float castRange;
    public float CastRange
    {
        get
        {
            return castRange;
        }
        set
        {
            castRange = value;
        }
    }
    
    public UnitType unitType;





    public int totalHP;
    public int totalMP;

}
