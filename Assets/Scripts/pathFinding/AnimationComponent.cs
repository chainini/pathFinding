using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnimationComponent : MonoBehaviour
{
    public Animator anim;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void PlayAnimation(string animationName)
    {
        anim.SetBool(animationName, true);
    }

    public void StopAnimation(string animationName)
    {
        anim.SetBool(animationName, false);
    }
    
    public void StopAllAnimation()
    {
        anim.SetBool("isIdle",false);
        //anim.SetBool("isHurt",false);
        anim.SetBool("isMove",false);
        anim.SetBool("isAttack",false);
    }

    public void Idle()
    {
        anim.Play("idle");
    }

    public void OnCancleAttack()
    {
        StopAllAnimation();
        anim.SetBool("isIdle",true);
    }
    
    /// <summary>
    /// 获取当前动画状态信息
    /// </summary>
    /// <param name="layerIndex">层级索引，默认为0</param>
    /// <returns>当前动画状态信息</returns>
    public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex = 0)
    {
        if (anim == null) return default(AnimatorStateInfo);
        return anim.GetCurrentAnimatorStateInfo(layerIndex);
    }
    
    /// <summary>
    /// 获取当前播放动画的长度（秒）
    /// 注意：返回的是Animator状态的长度，可能受Speed、Transition等影响
    /// </summary>
    /// <param name="layerIndex">层级索引，默认为0</param>
    /// <returns>动画长度（秒）</returns>
    public float GetCurrentAnimationLength(int layerIndex = 0)
    {
        if (anim == null) return 0f;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(layerIndex);
        return stateInfo.length;
    }
    
    /// <summary>
    /// 获取当前动画的实际播放时长（考虑Speed参数）
    /// 注意：AnimatorStateInfo.length 返回的是状态长度，可能包含过渡时间
    /// 如果动画片段本身是1秒，但length返回1.333秒，可能是因为：
    /// 1. 动画片段本身确实是1.333秒
    /// 2. 状态有Transition Duration影响了length
    /// 3. 在状态刚进入时获取，可能还在过渡中
    /// </summary>
    /// <param name="layerIndex">层级索引，默认为0</param>
    /// <returns>动画长度（秒）</returns>
    public float GetCurrentAnimationPlaybackDuration(int layerIndex = 0)
    {
        if (anim == null) return 0f;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(layerIndex);
        
        // AnimatorStateInfo.length 已经考虑了Speed参数
        // 如果Speed=1，length就是动画片段的长度
        // 如果Speed=0.75，length会相应调整
        
        return stateInfo.length;
    }
    
    /// <summary>
    /// 获取当前动画的归一化时间（0-1）
    /// </summary>
    /// <param name="layerIndex">层级索引，默认为0</param>
    /// <returns>归一化时间（0-1）</returns>
    public float GetCurrentAnimationNormalizedTime(int layerIndex = 0)
    {
        if (anim == null) return 0f;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(layerIndex);
        return stateInfo.normalizedTime;
    }
    
    /// <summary>
    /// 获取当前动画的播放时间（秒）
    /// </summary>
    /// <param name="layerIndex">层级索引，默认为0</param>
    /// <returns>当前播放时间（秒）</returns>
    public float GetCurrentAnimationTime(int layerIndex = 0)
    {
        if (anim == null) return 0f;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(layerIndex);
        return stateInfo.normalizedTime * stateInfo.length;
    }
    
    /// <summary>
    /// 检查当前是否在播放指定名称的动画
    /// </summary>
    /// <param name="stateName">动画状态名称</param>
    /// <param name="layerIndex">层级索引，默认为0</param>
    /// <returns>是否在播放该动画</returns>
    public bool IsPlayingAnimation(string stateName, int layerIndex = 0)
    {
        if (anim == null) return false;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(layerIndex);
        return stateInfo.IsName(stateName);
    }
    
}
