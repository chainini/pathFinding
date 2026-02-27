using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using RVO;

public class NavigationAgentComponent : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] float stoppingDistance = 0.1f;
    [SerializeField] float pathUpdateInterval = 0.2f;

    private Unit unit;
    private NavMeshAgent agent;
    private NavMeshObstacle obstacle;
    

    public NavMeshAgent Agent
    {
        get { return agent; }
    }
    private Vector3 currentDestination;
    [SerializeField]private bool isNavigating = false;
    private float lastPathUpdate;
    
    public bool IsNavigating
    {
        get { return isNavigating; }
        set { isNavigating = value; }
    }

    public Vector3 CurrentDestination => currentDestination;

    private void Awake()
    {
        unit = GetComponent<Unit>();
        agent = GetComponent<NavMeshAgent>();
        obstacle = GetComponent<NavMeshObstacle>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        if (obstacle == null)
        {
            obstacle = gameObject.AddComponent<NavMeshObstacle>();
        }
        
        obstacle.size = Vector3.one * 0.5f;
        
        
    }
    
    private void Start()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
        
        agent.acceleration = float.MaxValue;        
        agent.angularSpeed = float.MaxValue; 
        
        agent.speed = unit.move.MoveSpeed; 
        radius = GetColliderRadius();
        stoppingDistance = radius * 0.8f;
        agent.stoppingDistance = stoppingDistance;
        
        
        neighborDist = radius * 1.5f;
        
        
        
        
        var p = transform.position;
        rvoId = Simulator.Instance.addAgent(new RVO.Vector2(p.x, p.z));
        Simulator.Instance.setAgentRadius(rvoId, radius);
        Simulator.Instance.setAgentNeighborDist(rvoId, neighborDist);
        Simulator.Instance.setAgentMaxSpeed(rvoId, unit.move.MoveSpeed);
    }

    private float GetColliderRadius()
    {
        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere)
        {
            return sphere.radius * 1.5f;
        }
        
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule)
        {
            return capsule.radius* 1.5f;
        }
        
        BoxCollider box = GetComponent<BoxCollider>();
        if (box)
        {
            Vector3 size = box.size;
            return Mathf.Max(size.x,size.z) * 0.75f;
        }

        return 0.5f;
    }

    public void SetDestinationWithChase(Vector3 destination, float attackRange)
    {
        
        obstacle.carving = false;
        isNavigating = true;
        
        
        stoppingDistance = attackRange;
        agent.stoppingDistance = stoppingDistance;
        
        Vector3 dir = destination - transform.position;
        dir.y = 0;
        Vector3 want = destination - dir.normalized * (stoppingDistance + 0.2f);

        if (UnityEngine.AI.NavMesh.SamplePosition(want, out var hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
        {
            want = hit.position;
        }

        currentDestination = want;
        
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }
    
    public void SetDestination(Vector3 destination)
    {
        obstacle.carving = false;
        currentDestination = destination;
        isNavigating = true;
        
        stoppingDistance = radius * 0.8f;
        agent.stoppingDistance = stoppingDistance;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
        }
    }

    public void StopNavigation()
    {
        isNavigating = false;
        if (agent != null)
        {
            //agent.velocity = Vector3.zero;
            //agent.ResetPath();
        }
    }

    public void OpenNavigation()
    {
        isNavigating = true;
    }

    private void Update()
    {
        if (!isNavigating) return;
        
        agent.nextPosition = transform.position;
        
        if (agent.remainingDistance <= stoppingDistance)
        {
            OnReachDestination();
            return;
        }

        if (Time.time - lastPathUpdate > pathUpdateInterval)
        {
            UpdatePath();
            lastPathUpdate = Time.time;
        }

        
        var pos = transform.position;
        Simulator.Instance.setAgentPosition(rvoId, new RVO.Vector2(pos.x, pos.z));
        Simulator.Instance.setAgentMaxSpeed(rvoId, agent.speed);

        Vector3 desireVel = CalculateDesireVelocityFormPath();
        Simulator.Instance.setAgentPrefVelocity(rvoId, new RVO.Vector2(desireVel.x, desireVel.z));
        
        var rvoVel2 = Simulator.Instance.getAgentVelocity(rvoId);
        Vector3 rvoVelocity = new Vector3(rvoVel2.x_, 0f, rvoVel2.y_);
        
        //Debug.Log($"pref:{desireVel.magnitude:F2} rvo:{rvoVelocity.magnitude:F2} angle:{Vector3.Angle(desireVel, rvoVelocity):F1} max:{agent.speed:F2}");
        
        float scale = 0.6f;
        Debug.DrawRay(transform.position, rvoVelocity * scale, Color.green, 0f, false);
        
        if (unit.move != null && unit.move.IsMoving)
        {
            unit.move.UpdateMovement(rvoVelocity, Time.deltaTime);
        }
    }
    
    Vector3 lastRvoVelocity; 
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + lastRvoVelocity * 0.6f);
    }

    private void OnDisable()
    {
        if (rvoId != -1)
        {
            Simulator.Instance.delAgent(rvoId);
            rvoId = -1;
        }
    }


    private void UpdatePath()
    {
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(currentDestination);
        }
    }

    private void OnReachDestination()
    {
        
        isNavigating = false;

        if (unit.move != null)
        {
            unit.move.StopMove();
        }
        

        Simulator.Instance.setAgentPrefVelocity(rvoId,new RVO.Vector2(0,0));
        obstacle.carving = true;
    }
    
    public bool CanReach(Vector3 destination)
    {
        if(agent == null) return false;
        
        NavMeshPath path = new NavMeshPath();

        return agent.CalculatePath(destination, path);
    }

    public float GetPathDistance(Vector3 destination)
    {
        if (agent == null) return float.MaxValue;
        
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(destination, path))
        {
            return GetPathLength(path);
        }
        return float.MaxValue;
    }

    private float GetPathLength(NavMeshPath path)
    {
        float lenth = 0;
        for (int i = 0; i < path.corners.Length - 1; ++i)
        {
            lenth += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }
        return lenth;
    }


    [Header("RVO Settings")] 
    [SerializeField] private float timeHorizon = 1f;
    [SerializeField] private float neighborDist = 3f;
    [SerializeField] private float radius = 0.5f;
    private int rvoId = -1;
    

    private Vector3 CalculateDesireVelocityFormPath()
    {
        if (!agent.hasPath || agent.path.corners.Length < 2)
        {
            return Vector3.zero;
        }
        
        Vector3 nextConrner = agent.path.corners[1];
        Vector3 direction = (nextConrner - transform.position).normalized;
        
        Vector3 desireVelocity = direction * unit.move.MoveSpeed;
        
        return desireVelocity;
    }
}
