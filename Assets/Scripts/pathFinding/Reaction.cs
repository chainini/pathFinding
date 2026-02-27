
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Reaction
{
    public React react;
    public float duration;
    public bool canStack;
    public bool isBloacking;
    public int priority;
    public event Action<Unit> Ended;
    
    public void RaiseEnded(Unit unit) => Ended?.Invoke(unit);
    
    public abstract void OnTrigger(Unit attacker, Unit target, float damage);
    public abstract void OnUpdate(Unit target, float deltaTime);
    public abstract void OnEnd(Unit target);
}


public class StunReaction : Reaction
{
    public override void OnTrigger(Unit attacker, Unit target, float damage)
    {
        
    }

    public override void OnUpdate(Unit target, float deltaTime)
    {
        
    }

    public override void OnEnd(Unit target)
    {
        
    }
}

public class KnockbackReaction : Reaction
{
    private Vector2 knockBackForce;
    private CharacterController targetController;
    private Tween knockBackTween;

    public KnockbackReaction(Vector2 knockBackForce , float duration)
    {
        this.knockBackForce = knockBackForce;
        this.duration = duration;
        isBloacking = false;
    }

    private Vector3 startPos;
    public override void OnTrigger(Unit attacker, Unit target, float damage)
    {    
        Vector3 dir = (target.transform.position - attacker.transform.position);
        dir.y = 0f;
        dir.Normalize();
        
        targetController = target.GetComponent<CharacterController>();
        if (targetController == null)
        {
            targetController = target.gameObject.AddComponent<CharacterController>();
            targetController.height = 2f;
            targetController.radius = 0.5f;
            targetController.center = new Vector3(0, 1f, 0);
        }
        
        target.agent.StopNavigation();
        
        startPos = target.transform.position;
        Vector3 endPos = startPos + dir * knockBackForce.x;               // 水平终点
        Vector3 topPos = startPos + dir * (knockBackForce.x * 0.5f);      // 水平在中点
        topPos.y += knockBackForce.y;                                     // 抛起高度
        
        float t = 0f;
        knockBackTween = DOTween.To(() => t, x => t = x, 1f, duration)
            .SetEase(Ease.Linear)          
            .OnUpdate(() =>
            {
                if (targetController == null) return;

                Vector3 lastPos   = target.transform.position;
                Vector3 newPos    = CurveTool.Bezier2(startPos, topPos, endPos, t);
                Vector3 delta     = newPos - lastPos;

                targetController.Move(delta);
            })
            .OnComplete(() =>
            {
                OnEnd(target);
            });
        
    }

    public override void OnUpdate(Unit target, float deltaTime)
    {
        
    }

    public override void OnEnd(Unit target)
    {
        target.transform.position = new Vector3(target.transform.position.x, startPos.y, target.transform.position.z);
        target.agent.OpenNavigation();
        if (knockBackTween != null)
        {
            knockBackTween.Kill();
            knockBackTween = null;
        }
    }
}


public class HitStopReaction : Reaction
{
    private float positionOffset;
    private Vector3 originalPosition;

    public HitStopReaction(float positionOffset, float duration)
    {
        this.positionOffset = positionOffset;
        this.duration = duration;
        isBloacking = true;
    }
    
    public override void OnTrigger(Unit attacker, Unit target, float damage)
    {
        target.StopCoroutine(HitStopCoroutine(attacker, target, damage));
        
        attacker.animation.anim.enabled = false;
        target.animation.anim.enabled = false;
        
        originalPosition = target.transform.position;
        
        duration = Mathf.Clamp(damage * 0.01f, 0.1f, 0.5f);

        target.StartCoroutine(HitStopCoroutine(attacker ,target, 1.5f));
    }
    
    IEnumerator HitStopCoroutine(Unit attacker, Unit target, float duration)
    {
        if (duration <= 0)
        {
            attacker.animation.anim.enabled = true;
            target.animation.anim.enabled = true;
            target.animation.anim.transform.position = originalPosition;
            yield break;
        }
        var x =  Random.Range(target.animation.anim.transform.position.x - positionOffset, target.animation.anim.transform.position.x + positionOffset);
        var z =  Random.Range(target.animation.anim.transform.position.z - positionOffset, target.animation.anim.transform.position.z + positionOffset);
        target.animation.anim.transform.position = new Vector3(x,target.animation.anim.transform.position.y,z);
        yield return new WaitForEndOfFrame();
        yield return HitStopCoroutine(attacker ,target, duration - 0.1f);
    }

    public override void OnUpdate(Unit target, float deltaTime)
    {
        
    }

    public override void OnEnd(Unit target)
    {
        RaiseEnded(target);
    }
}
