using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainPlane : MonoBehaviour
{
    // 引用
    public Material groundMaterial;           // 地面材质
    public ParticleSystem rainParticleSystem; // 雨粒子系统（可选，用于监听）

    // 涟漪管理
    private Vector4[] rippleDataArray;        // 涟漪数组（Vector4 = xyz位置 + w时间）
    public float[] rippleStrengthArray ;
    private const int MAX_RIPPLES = 128;      // 最大涟漪数量
    private int currentIndex = 0;             // 当前写入位置（循环）
    private int activeRippleCount = 0;        // 活跃涟漪数量

    // 参数
    public float rippleLifetime = 2.0f;       // 涟漪生命周期（与Shader一致）
    
    void Start()
    {
        // 初始化数组
        rippleDataArray = new Vector4[MAX_RIPPLES];
        rippleStrengthArray = new float[MAX_RIPPLES];
    
        // 清空所有涟漪（设置时间为负数，Shader会跳过）
        for (int i = 0; i < MAX_RIPPLES; i++)
        {
            rippleDataArray[i] = new Vector4(0, 0, 0, -999);
            rippleStrengthArray[i] = 1.0f;
        }
    
        // 可选：自动找到材质
        if (groundMaterial == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
                groundMaterial = renderer.material;
        }
    }
    
    void OnParticleCollision(GameObject other)
    {
        // 获取粒子系统
        ParticleSystem ps = other.GetComponent<ParticleSystem>();
        if (ps == null) return;
    
        // 获取碰撞事件
        List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
        int numCollisionEvents = ps.GetCollisionEvents(gameObject, collisionEvents);
    
        // 遍历每个碰撞点
        for (int i = 0; i < numCollisionEvents; i++)
        {
            Vector3 pos = collisionEvents[i].intersection; // 碰撞点世界坐标
            
            Vector3 velocity = collisionEvents[i].velocity;
            float speed = velocity.magnitude;

            float strength = Mathf.Clamp(speed / 20f, 0.5f, 1.5f);
            
            AddRipple(pos, strength);
        }
    }
    
    void AddRipple(Vector3 worldPosition, float strength)
    {
        // 循环写入数组
        rippleDataArray[currentIndex] = new Vector4(
            worldPosition.x,
            worldPosition.y,
            worldPosition.z,
            Time.time  // 当前时间作为生成时间
        );
        
        rippleStrengthArray[currentIndex] = strength;
    
        // 移动到下一个位置（循环）
        currentIndex = (currentIndex + 1) % MAX_RIPPLES;
    
        // 更新活跃数量（最多MAX_RIPPLES）
        activeRippleCount = Mathf.Min(activeRippleCount + 1, MAX_RIPPLES);
    }
    
    void Update()
    {
        if (groundMaterial == null) return;
    
        // 清理过期涟漪（可选优化）
        CleanupExpiredRipples();
    
        // 传递数据到Shader
        groundMaterial.SetVectorArray("_RippleDataArray", rippleDataArray);
        groundMaterial.SetFloatArray("_RippleStrengthArray", rippleStrengthArray);
        groundMaterial.SetInt("_RippleCount", activeRippleCount);
    }
    
    void CleanupExpiredRipples()
    {
        float currentTime = Time.time;
        int validCount = 0;
    
        // 遍历数组，统计仍然有效的涟漪
        for (int i = 0; i < MAX_RIPPLES; i++)
        {
            float birthTime = rippleDataArray[i].w;
            float age = currentTime - birthTime;
        
            if (age < rippleLifetime && age >= 0)
            {
                validCount++;
            }
        }
    
        activeRippleCount = validCount;
    }
}
