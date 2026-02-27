using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveComponent : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 180f;
    
    private Unit unit;

    private Vector3 targetPos;
    [SerializeField] private bool isMoving;
    
    public float MoveSpeed
    {
        get { return moveSpeed; }
    }

    public float RotationSpeed
    {
        get { return rotationSpeed; }
    }
    public bool IsMoving
    {
        get { return isMoving; }
    }

    public Vector3 TargetPos
    {
        get { return targetPos; }
    }

    private void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public void StartChaseTo(Unit target)
    {
        StopCoroutine(ChaseTo(target.transform.position, unit.unitData.AttackRange));
        StartCoroutine(ChaseTo(target.transform.position, unit.unitData.AttackRange));
        unit.animation.StopAnimation("isIdle");
        unit.animation.StopAnimation("isAttack");
        unit.animation.PlayAnimation("isMove");
    }


    public void StartMoveTo(Vector3 pos)
    {
        StopCoroutine(MoveTo(pos));
        StartCoroutine(MoveTo(pos));
        unit.animation.StopAnimation("isIdle");
        unit.animation.StopAnimation("isAttack");
        unit.animation.PlayAnimation("isMove");
    }
    
    public IEnumerator ChaseTo(Vector3 pos, float attackRange)
    {
        
        
        unit.agent.SetDestinationWithChase(pos,attackRange);

        yield return new WaitUntil(() => !unit.agent.Agent.pathPending);
        
        // 检查路径是否有效
        if (!unit.agent.Agent.hasPath)
        {
            Debug.LogWarning($"Unit {name} cannot reach destination {pos}");
            yield break;
        }
        
        targetPos = pos;
        isMoving = true;
        unit.agent.IsNavigating = true;
    }


    public IEnumerator MoveTo(Vector3 pos)
    {
        
        
        unit.agent.SetDestination(pos);

        yield return new WaitUntil(() => !unit.agent.Agent.pathPending);
        
        // 检查路径是否有效
        if (!unit.agent.Agent.hasPath)
        {
            Debug.LogWarning($"Unit {name} cannot reach destination {pos}");
            yield break;
        }
        
        targetPos = pos;
        isMoving = true;
        unit.agent.IsNavigating = true;
    }

    public void StopMove()
    {
        unit.animation.StopAnimation("isMove");
        unit.animation.PlayAnimation("isIdle");
        isMoving = false;
        targetPos = transform.position;
        
        unit.agent.StopNavigation();
    }

    public void UpdateMovement(Vector3 direction, float deltaTime)
    {
        if (!isMoving || direction.magnitude < 0.1f) return;
        
        Vector3 movement = direction.normalized * moveSpeed * deltaTime;
        // transform.position += movement;
        
        var newPos = transform.position + movement;
        // UnityEngine.AI.NavMeshHit hit;
        // if (UnityEngine.AI.NavMesh.SamplePosition(newPos, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
        // {
        //     float footOffset = 0f;
        //     var cd = GetComponent<Collider>();
        //     if (cd)
        //     {
        //         footOffset = transform.position.y - cd.bounds.center.y;
        //     }
        //     newPos.y = hit.position.y + footOffset + 0.02f;
        // }
        
        transform.position = newPos;

        if (direction != Vector3.zero)
        {
            Quaternion tagretRoatation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                tagretRoatation,
                rotationSpeed*deltaTime);
        }
    }
}
