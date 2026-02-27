using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UnitStateMachine : MonoBehaviour,IControlAffectable
{
    public RPGInput input;

    private Unit unit;

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    [Space(100)]
    [Header("RPG info")]
    [Tooltip("RPG 当前状态̬")]
    public RPGState currentState = RPGState.Idle;
    
    [Space(100)]
    [Header("RPG 状态数据")]
    [Tooltip("当前状态已持续的时间")]
    public float stateTimer = 0f;  
    [Tooltip("当前状态需要持续的时间")]
    public float stateDuration = 0f;  
    [Tooltip("状态是否被锁定（持续中）")]
    public bool isStateLocked = false; 

    public void SwitchState(RPGState newState)
    {
        unit.animation.StopAllAnimation();
        switch (newState)
        {
            case RPGState.Idle:
                unit.animation.PlayAnimation("isIdle");
                break;
            case RPGState.Move:
                //unit.animation.PlayAnimation("isMove");
                break;
            case RPGState.Attack:
                //unit.animation.PlayAnimation("isAttack");
                break;
            case RPGState.Dash:
                ;
                break;
            case RPGState.Skill:
                ;
                break;
            case RPGState.Roll:
                ;
                break;
        }
        currentState = newState;
    }

    /// <summary>
    /// 获取当前动画状态的播放时间（秒）
    /// </summary>
    /// <returns>当前动画的播放时间</returns>
    public float GetCurrentStateTime()
    {
        if (unit == null || unit.animation == null || unit.animation.anim == null)
            return 0f;
        
        return unit.animation.GetCurrentAnimationTime();
    }
    
    /// <summary>
    /// 获取动画长度
    /// </summary>
    /// <param name="AnimationName">动画名称  是动画名称</param>
    /// <returns></returns>
    public float GetStateLength(string AnimationName)
    {
        if (unit == null || unit.animation == null || unit.animation.anim == null)
            return 0f;
        var clips = unit.animation.anim.runtimeAnimatorController.animationClips;
        var targetClip = clips[0];
        foreach (var clip in clips)
        {
            if (clip.name == AnimationName)
            {
                targetClip = clip;
            }
        }
        Debug.Log(targetClip.name);
        Debug.Log(targetClip.length);
        return targetClip.length;
    }

    public RPGState CalculateInput(RPGInput input)
    {
        RPGState newState = RPGState.Idle;

        if (input.attackPressed)
        {
            newState = RPGState.Attack;
        }else if (input.moveAxis.magnitude > 0)
        {
            newState = RPGState.Move;
        }
        
        return newState;
    }
    
    public void ApplyControlEffect(ControlEffect effect)
    {
        
    }
}

// RPG模式下的输入
public struct RPGInput
{
    // 键鼠
    public Vector2 moveAxis;       
    public bool attackPressed;     
    public bool attackHeld;        
    public bool dashPressed;
    public bool jumpPressed;
    public bool skill1Pressed;
    public Vector3 mouseOnPlane;

    // 地形
    public bool isGrounded;
    public bool isOnSlope;
    public float verticalVelocity;

    // 技能/攻击
    public List<ControlEffect> pendingEffects;
}
