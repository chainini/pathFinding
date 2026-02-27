using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids : MonoBehaviour
{
    public float boundaryRadius = 40f;
    public float maxBoundaryDistance = 50f;

    public float perceptionRadius = 10f;
    public float separationRadius = 5f;  // 分离用更短的距离
    public float maxSpeed = 10f;
    public float maxForce = 5f;  // 每帧最大转向力

    // 三个力的权重
    public float sepWeight = 1.5f;
    public float aliWeight = 1f;
    public float cohWeight = 1f;

    List<GameObject> boids = new List<GameObject>();
    void Start()
    {
        for(int i = 0; i < 100; i++){
            GameObject boid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boids.Add(boid);
            boid.transform.parent = transform;
            boid.transform.position = Random.insideUnitSphere * 30f;

            velocities.Add(Random.insideUnitSphere * maxSpeed);
            newVelocities.Add(Vector3.zero);
        }
    }


    List<Vector3> velocities = new List<Vector3>();  // 新增：速度数组
    List<Vector3> newVelocities = new List<Vector3>();  // 新增：下一帧速度
    void Update(){
        for (int i = 0; i < boids.Count; i++)
        {
            List<int> neighborIndices = GetNeighborIndices(boids[i], i);
            Vector3 separation = CalculateSeparation(neighborIndices, i);
            Vector3 alignment = CalculateAlignment(neighborIndices, i);
            Vector3 cohesion = CalculateCohesion(neighborIndices, i);
            Vector3 boundary = CalculateBoundaryForce(boids[i].transform.position);

            // 加权合成转向力
            Vector3 steer = separation * sepWeight 
                          + alignment * aliWeight 
                          + cohesion * cohWeight
                          + boundary;
            
            // 限制转向力大小
            steer = Vector3.ClampMagnitude(steer, maxForce);
            // Vector3 desired = (separation * sepWeight + alignment * aliWeight + cohesion * cohWeight).normalized * maxSpeed;
            // Vector3 steer = Vector3.ClampMagnitude(desired - velocities[i] + boundary, maxForce);
            // 更新速度
            Vector3 newVel = velocities[i] + steer * Time.deltaTime;
            newVel = Vector3.ClampMagnitude(newVel, maxSpeed);
            newVelocities[i] = newVel;
        }
    }

    void LateUpdate(){
        for (int i = 0; i < boids.Count; i++)
        {
            velocities[i] = newVelocities[i];
            
            // 更新位置
            boids[i].transform.position += velocities[i] * Time.deltaTime;
            
            // 朝向运动方向
            if (velocities[i].sqrMagnitude > 0.001f)
            {
                boids[i].transform.forward = velocities[i].normalized;
            }
        }
    }


   List<int> GetNeighborIndices(GameObject boid, int selfIndex)
    {
        List<int> neighbors = new List<int>();
        for (int i = 0; i < boids.Count; i++)
        {
            if (i == selfIndex) continue;
            if (Vector3.Distance(boid.transform.position, boids[i].transform.position) < perceptionRadius)
            {
                neighbors.Add(i);
            }
        }
        return neighbors;
    }

    Vector3 CalculateBoundaryForce(Vector3 position)
    {
        float distanceFromCenter = position.magnitude;
        
        if (distanceFromCenter < boundaryRadius)
            return Vector3.zero;  // 在舒适区内，无边界力
        
        // 越远离中心，力越强（0 ~ 1）
        float t = Mathf.InverseLerp(boundaryRadius, maxBoundaryDistance, distanceFromCenter);
        
        // 方向指向中心
        Vector3 toCenter = -position.normalized;
        
        // 力的大小随距离增加（可选：平方增长，边缘更强烈）
        return toCenter * t * t * maxForce * 2f;
    }

    Vector3 CalculateSeparation(List<int> neighborIndices, int selfIndex)
    {
        Vector3 steer = Vector3.zero;
        int count = 0;
        
        foreach (int i in neighborIndices)
        {
            float dist = Vector3.Distance(boids[selfIndex].transform.position, boids[i].transform.position);
            if (dist < separationRadius && dist > 0.001f)
            {
                // 远离邻居，距离越近力越大
                Vector3 diff = boids[selfIndex].transform.position - boids[i].transform.position;
                diff.Normalize();
                diff /= dist;  // 距离衰减
                steer += diff;
                count++;
            }
        }

        if (count > 0)
        {
            steer /= count;  // 平均
        }
        
        // 从"期望速度"转为"转向力"
        if (steer.sqrMagnitude > 0.001f)
        {
            steer.Normalize();
            steer *= maxSpeed;
            steer -= velocities[selfIndex];  // Reynolds: steer = desired - velocity
        }
        
        return steer;
    }

    Vector3 CalculateAlignment(List<int> neighborIndices, int selfIndex)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        
        foreach (int i in neighborIndices)
        {
            sum += velocities[i];
            count++;
        }

        if (count == 0) return Vector3.zero;

        // 平均方向作为期望速度
        Vector3 desired = (sum / count).normalized * maxSpeed;
        Vector3 steer = desired - velocities[selfIndex];
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    Vector3 CalculateCohesion(List<int> neighborIndices, int selfIndex)
    {
        if (neighborIndices.Count == 0) return Vector3.zero;

        // 计算邻居中心点
        Vector3 center = Vector3.zero;
        foreach (int i in neighborIndices)
        {
            center += boids[i].transform.position;
        }
        center /= neighborIndices.Count;

        // 朝向中心
        Vector3 desired = (center - boids[selfIndex].transform.position).normalized * maxSpeed;
        Vector3 steer = desired - velocities[selfIndex];
        return Vector3.ClampMagnitude(steer, maxForce);
    }
}
