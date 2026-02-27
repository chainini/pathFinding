using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    Friend,
    Enemy,
    Mid,
}

public enum UnitType
{
    Cube,
    Sphere,
    
}

public enum Order
{
    Move,
    Attack,
    Stop,
    Cast,
    Null,
}

public enum AttackState
{
    Idle,
    Chase,
    AttackCD,
}


public enum React
{
    HitStop,
    Stun,
    Knockback,
    Fire,
    Water,
    Lightning,
    Ice,
}

public enum GameModeEnum
{
    RTS,
    RPG,
}

public enum RPGState
{
    Idle,
    Move,
    Attack,
    Skill,
    Dash,
    Jump,
    Roll,
}

public enum Sweeplight
{
    Sector,
    Circle,
    Rectangle,
}

public enum UIAnimType
{
    Pop,
    Zoom,
    Fade,
    Custom,
}

public enum UIType
{
    HUD,
    Window,
    Overlay,
    Popup,
}

public enum UIID
{
    Loading,
    RPGHUD,
    Setting,
    MainMenu,
    Backpack,
    Mission,
    Tip,
    Toast,
}




