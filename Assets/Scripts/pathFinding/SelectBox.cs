using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectBox : MonoBehaviour
{
    public Vector2 startMousePos;
    public Vector2 endMousePos;
    public Vector3 startMoseWorldPos;
    public Vector3 endMoseWorldPos;
    public bool isSelecting = false;
    private Material lineMat;
    
    private Camera cam;
    private PlayerCtrl playerCtrl;

    private void Start()
    {
        playerCtrl = GetComponent<PlayerCtrl>();
        cam = Camera.main;
        lineMat = new Material(Shader.Find("Hidden/Internal-Colored"));
    }

    void Update()
    {
        if (GameModeManager.Instance.CurrentGameMode() == GameModeEnum.RTS)
        {
            if (Input.GetMouseButtonDown(0))
            {
                startMousePos = Input.mousePosition;
                TryGetGroundPoint(startMousePos, out startMoseWorldPos);
                isSelecting = true;
            }
            if (Input.GetMouseButton(0))
            {
                endMousePos = Input.mousePosition;
                TryGetGroundPoint(endMousePos, out endMoseWorldPos);
            }
            if (Input.GetMouseButtonUp(0))
            {
                isSelecting = false;
                SelectUnits();
            }
        }
    }

    public void SelectUnits()
    {
        // 计算 Box 的中心和半尺寸
        Vector3 center = new Vector3(
            (startMoseWorldPos.x + endMoseWorldPos.x) * 0.5f,
            0f,
            (startMoseWorldPos.z + endMoseWorldPos.z) * 0.5f
        );
    
        Vector3 halfExtents = new Vector3(
            Mathf.Abs(endMoseWorldPos.x - startMoseWorldPos.x) * 0.5f,
            10f,   // 高度给大一点，覆盖所有单位
            Mathf.Abs(endMoseWorldPos.z - startMoseWorldPos.z) * 0.5f
        );
    
        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, LayerMask.GetMask("Unit"));
        foreach (var col in hits)
        {
            Unit unit = col.GetComponentInParent<Unit>();
            if (unit != null && unit.unitData.Team == Team.Friend)
            {
                playerCtrl.AddUnit(unit);
            }
        }
    }
    
    bool TryGetGroundPoint(Vector2 screenPos, out Vector3 worldPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, LayerMask.GetMask("Ground")))
        {
            worldPos = hit.point;
            return true;
        }
        worldPos = Vector3.zero;
        return false;
    }
    
    
    void OnPostRender()
    {
        if (!isSelecting) return;
        
        lineMat.SetPass(0);
        
        GL.PushMatrix();
        GL.LoadOrtho();
        //GL.Begin(GL.LINES);
        GL.Color(new Color(1f, 1f, 1f, 1f));
        
        // 转换到 0-1 范围
        Vector2 min = Vector2.Min(startMousePos, endMousePos) / new Vector2(Screen.width, Screen.height);
        Vector2 max = Vector2.Max(startMousePos, endMousePos) / new Vector2(Screen.width, Screen.height);
        
        float thickness = 2f / Screen.height; // 2 像素宽
        
        // 绘制矩形边框
        DrawThickLine(new Vector2(min.x, min.y), new Vector2(max.x, min.y), thickness); // 下
        DrawThickLine(new Vector2(max.x, min.y), new Vector2(max.x, max.y), thickness); // 右
        DrawThickLine(new Vector2(max.x, max.y), new Vector2(min.x, max.y), thickness); // 上
        DrawThickLine(new Vector2(min.x, max.y), new Vector2(min.x, min.y), thickness); // 左
        
        GL.End();
        GL.PopMatrix();
    }
    
    void DrawThickLine(Vector2 a, Vector2 b, float thickness)
    {
        Vector2 dir = (b - a).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * thickness * 0.5f;

        GL.Begin(GL.QUADS);
        GL.Vertex3(a.x - perp.x, a.y - perp.y, 0);
        GL.Vertex3(a.x + perp.x, a.y + perp.y, 0);
        GL.Vertex3(b.x + perp.x, b.y + perp.y, 0);
        GL.Vertex3(b.x - perp.x, b.y - perp.y, 0);
        GL.End();
    }
    
    void DrawLine(Vector2 a, Vector2 b)
    {
        GL.Vertex(a);
        GL.Vertex(b);
    }
}
