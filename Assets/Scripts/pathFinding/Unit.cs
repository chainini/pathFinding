using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public HealthComponent health;
    public MoveComponent move;
    public AttackComponent attack;
    public UnitDataComponent unitData;
    public NavigationAgentComponent agent;
    public SensorTargetComponent sensorTarget;
    public OrderQueueComponent orderQueue;
    public AnimationComponent animation;
    public UnitStateMachine stateMachine;

    private void Awake()
    {
        health = GetComponent<HealthComponent>();
        move = GetComponent<MoveComponent>();
        attack = GetComponent<AttackComponent>();
        unitData = GetComponent<UnitDataComponent>();
        agent = GetComponent<NavigationAgentComponent>();
        sensorTarget = GetComponent<SensorTargetComponent>();
        orderQueue = GetComponent<OrderQueueComponent>();
        animation = GetComponent<AnimationComponent>();
        stateMachine = GetComponent<UnitStateMachine>();
    }


    private void Update()
    {
        //如果主动命令

        return;
        //无主动命令
        //侦察
        return;
        //移动
        return;
        //攻击
        return;
    }

    
    
    
    
}


