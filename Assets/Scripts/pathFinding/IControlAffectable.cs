
using UnityEngine;

public enum ControlEffectType
{
    Stun,
    Knockback,
    Airborne,
    Root,      // 不能移动
    Silence,   // 不能放技能
    Custom
}

public struct ControlEffect
{
    public ControlEffectType type;
    public float duration;
    public Vector3 direction;
    public float power;
}

public interface IControlAffectable
{
    void ApplyControlEffect(ControlEffect effect);
}
