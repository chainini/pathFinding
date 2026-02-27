using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    [SerializeField] private float attack;
    [SerializeField] private float attackRange;
    [SerializeField] private float rePathInterval = 0.25f;
    [SerializeField] private float checkInterval = 0.1f;
    [SerializeField] private float attackCooldown = 1f;

    private Unit unit;
    [SerializeField] private Unit target;
    public AttackState state = AttackState.Idle;
    private float nextCheck, nextRepath, cdEnd;

    [Header("普通攻击")]
    [Header("是否眩晕")]
    public bool isStun;
    public float stunDuration;
    [Header("是否击退")]
    public bool isKnockback;
    public Vector2 konckbackInfo;
    public float knockbackDuration;
    [Header("是否卡肉")]
    public bool isStop;
    public float positionOffset;
    public float stopDuration;
    
    private AttackRangeIndicator attackRangeIndicator;
    

    private void Awake()
    {
        unit = GetComponent<Unit>();
        attackRangeIndicator = unit.transform.Find("Effect").transform.Find("AttackRange").GetComponent<AttackRangeIndicator>();
    }

    //RTS
    public void TryAttack(Unit target)
    {
        this.target = target;
        state = AttackState.Chase;
        nextCheck = 0;
        nextRepath = 0;
    }
    //RPG
    public void StartAttack()
    {
        Attack();
    }

    private void Update()
    {
        if (target == null) return;

        if (Time.time < nextCheck) return;
        nextCheck = Time.time + checkInterval;

        switch (state)
        {
            case AttackState.Chase:
            {
                float dist = Vector3.Distance(this.transform.position, target.transform.position);
                if (dist <= attackRange)
                {
                    unit.move.StopMove();
                    Vector3 flat = target.transform.position - this.transform.position;
                    flat.y = 0;
                    if (flat.sqrMagnitude > 0.001f)
                    {
                        transform.rotation = Quaternion.LookRotation(flat);
                    }
                    if (Time.time < cdEnd) return;
                    Attack();
                    cdEnd = Time.time + attackCooldown;
                    state = AttackState.AttackCD;
                }
                else
                {
                    unit.move.StartChaseTo(target);
                }
            }
                break;
            case AttackState.AttackCD:
            {
                if (Time.time >= cdEnd)
                {
                    state = AttackState.Chase;
                }
                else
                {
                    unit.animation.StopAllAnimation();
                    unit.animation.PlayAnimation("isIdle");
                }
            }
                break;
            
        }
    }

    public void CancleAttackOrder()
    {
        target = null;
    }

    public void Attack()
    {
        unit.stateMachine.stateDuration = unit.stateMachine.GetStateLength("attack");
        attackRangeIndicator.ShowAttackRange(unit.stateMachine.stateDuration,Sweeplight.Sector,0.5f);
        
        unit.animation.StopAnimation("isIdle");
        unit.animation.StopAnimation("isMove");
        unit.animation.PlayAnimation("isAttack");
        
        Debug.Log("how many time");
    }
    
    public void OnApplyAttack()
    {
        List<Unit> targets = new List<Unit>();
        
        if (GameModeManager.Instance.CurrentGameMode() == GameModeEnum.RPG)
        {
            targets = FindTargetsInAttackRange(unit);
        }
        else
        {
            targets.Add(target);
        }
        ReactionManager.Instance.ApplyAttack(unit, targets);
        foreach (Unit unit in targets)
        {
            unit.animation.StopAllAnimation();
        }
    }
    
    private List<Unit> FindTargetsInAttackRange(Unit attacker)
    {
        List<Unit> found = new List<Unit>();
        
        // 扇形参数
        float angle = 60f;  // 扇形角度（比如 60 度）
        float halfAngle = angle * 0.5f;
        
        // 计算扇形左右边界方向
        Vector3 forward = attacker.transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, halfAngle, 0) * forward;
        
        // 画扇形边界线（从角色位置出发）
        Debug.DrawRay(attacker.transform.position, leftBoundary * attackRange, Color.red, 0.5f);
        Debug.DrawRay(attacker.transform.position, rightBoundary * attackRange, Color.red, 0.5f);
        
        // 画扇形弧线（用多个点连成线）
        int segments = 20;  // 弧线分段数
        Vector3 lastPoint = attacker.transform.position + leftBoundary * attackRange;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float currentAngle = -halfAngle + angle * t;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 currentPoint = attacker.transform.position + direction * attackRange;
            
            Debug.DrawLine(lastPoint, currentPoint, Color.yellow, 0.5f);
            lastPoint = currentPoint;
        }
        
        // 原有的检测逻辑
        Collider[] hits = Physics.OverlapSphere(
            attacker.transform.position + attacker.transform.forward * attackRange * 0.5f,
            attackRange,
            LayerMask.GetMask("Unit")
        );
        
        foreach (var hit in hits)
        {
            Unit target = hit.GetComponent<Unit>();
            if (target != null && target != attacker && target.unitData.Team == Team.Enemy)
            {
                // 检查是否在扇形范围内
                Vector3 toTarget = (target.transform.position - attacker.transform.position).normalized;
                float angleToTarget = Vector3.Angle(forward, toTarget);
                
                float dist = Vector3.Distance(attacker.transform.position, target.transform.position);
                if (dist <= attackRange && angleToTarget <= halfAngle)
                {
                    found.Add(target);
                }
            }
        }
        
        return found;
    }
}
