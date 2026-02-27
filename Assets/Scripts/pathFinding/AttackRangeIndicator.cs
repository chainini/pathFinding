using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻击扫光
/// </summary>
public class AttackRangeIndicator : MonoBehaviour
{
    public Sweeplight sweeplight;
    
    //扇形
    public float sectorRadius;
    public float sectorAngle;
    
    //圆形
    public float circleRadius;
    public Vector3 circleCenter;
    
    //矩形
    public float rectangleWidth;
    public float rectangleHeight;
    
    public float duration;

    public MeshRenderer meshRenderer;
    public MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        
        // 初始状态：隐藏
        propertyBlock.SetFloat("_FillProgress", 0);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    public void ShowAttackRange(float duration, Sweeplight type, float range, float angle = 60f)
    {
        // 设置参数到 PropertyBlock
        propertyBlock.SetInt("_AttackRangeType", (int)type);
        propertyBlock.SetFloat("_SectorRadius", range);
        propertyBlock.SetFloat("_SectorAngle", angle);
        propertyBlock.SetFloat("_FillProgress", 0);
        
        // 应用 PropertyBlock
        meshRenderer.SetPropertyBlock(propertyBlock);
        
        // 启动扫光动画
        StartCoroutine(AnimateSweep(duration));
    }

    public void HideAttackRange()
    {
        propertyBlock.SetFloat("_FillProgress", 0);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
    
    IEnumerator AnimateSweep(float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            propertyBlock.SetFloat("_FillProgress", progress);
            meshRenderer.SetPropertyBlock(propertyBlock);
            
            yield return null;
        }
        
        propertyBlock.SetFloat("_FillProgress", 0);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}


