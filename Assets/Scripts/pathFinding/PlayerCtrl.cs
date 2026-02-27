using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCtrl : MonoBehaviour
{
    private Camera cam;
    
    public List<Unit> units = new List<Unit>();

    private IFormationPlanner planner = new HexFormationPlanner();

    private Unit mySelf;
    
    private CharacterController controller;

    
    private void Awake()
    {
        
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;   // 或 -1 不限
        QualitySettings.antiAliasing = 0;
        QualitySettings.shadows = ShadowQuality.HardOnly;
        QualitySettings.shadowCascades = 2;
        
        cam = GetComponent<Camera>();
        mySelf = units[0];
        controller = mySelf.GetComponent<CharacterController>();
        
        GameModeManager.Instance.Init(new RPGMode());
        
        
        
    }

    private void OnEnable()
    {
        // EventManager.On(EventName.GameModeChange,);
    }


    private void Update()
    {
        if (GameModeManager.Instance.CurrentGameMode() == GameModeEnum.RPG)
        {
            //这个类只负责RTS  RPGcontrl被做成组件 添加到被认为是主角的身上
        }
        else
        {
            RTSModeHandler();
        }
    }

    
    

    private void RTSModeHandler()
    {
        HandleCameraMove();

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(true);
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleClick(false);
        }
        
        if (Input.GetMouseButton(0))
        {
            HandleDragLeftClick();
        }

        HandleScroll();
    }
    
    
    
    
    [Space(100)]
    [SerializeField] float panSpeed = 20f;
    private void HandleCameraMove()
    {
        return;
        Vector3 _mouseScrrenPos = Input.mousePosition;
        Vector3 mouseScrrenPos = cam.ScreenToViewportPoint(_mouseScrrenPos);
        Vector3 moveDir = Vector3.zero;

        if (mouseScrrenPos.x <= 0.01  || mouseScrrenPos.x >= 0.99)
        {
            bool isLeft = mouseScrrenPos.x - 0.5 < 0;
            moveDir += isLeft ? -cam.transform.right : cam.transform.right;
        }

        Vector3 fwdOnPlane = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        if (mouseScrrenPos.y >= 0.99 || mouseScrrenPos.y <= 0.01)
        {
            bool isUp = mouseScrrenPos.y - 0.5 > 0;
            moveDir += isUp ? fwdOnPlane : -fwdOnPlane;
        }

        if (moveDir.sqrMagnitude > 0)
        {
            moveDir.Normalize();
            Vector3 delta = moveDir * panSpeed * Time.deltaTime;
            Vector3 pos = cam.transform.position + delta;
            
            cam.transform.position = pos;
        }
    }

    private void HandleClick(bool leftClick)
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hit, LayerMask.GetMask("Ground"));

        hitPoint = hit.point;
        isClick = true;
        
        Unit target = null;
        float pickRadius = 0.25f;
        var unitList = Physics.SphereCastAll(hit.point, pickRadius, ray.direction, Single.MaxValue,
            LayerMask.GetMask("Unit"));
        if (unitList.Length > 0)
        {
            float dis = Vector3.Distance(hit.point, unitList[0].transform.position);
            foreach (var unit in unitList)
            {
                float curentDis = Vector3.Distance(hit.point, unit.transform.position);
                if (curentDis <= dis)
                {
                    target = unit.transform.root.GetComponent<Unit>();
                    dis = curentDis;
                }
            }
        }

        if (leftClick)
        {
            HandleLeftClick(target);
        }
        else
        {
            HandleRightClick(target, hit.point);
        }
    }

    private void HandleLeftClick(Unit unit)
    {
        if (unit != null)
        {
            if (unit.unitData.Team == Team.Friend)
            {
                units.Add(unit);
            }
            
            ShowUnitInfo(unit);
        }
        else
        {
            units.Clear();
        }
    }

    private void HandleRightClick(Unit unit,Vector3 hit)
    {
        // RaycastHit hit;
        // Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        // Physics.Raycast(ray, out hit);
        //
        // var unit = hit.collider.GetComponent<Unit>();

        if (unit != null && unit.unitData.Team == Team.Enemy)
        {
            Debug.Log("Attackk Enemy");
            foreach (var _unit in units)
            {
                _unit.orderQueue.EnOrder(Order.Attack);
                _unit.attack.TryAttack(unit);
            }
            return;
        }

        if (unit != null)
        {
            foreach (var _unit in units)
            {
                _unit.orderQueue.EnOrder(Order.Move);
                _unit.move.StartChaseTo(unit);
            }
            return;
        }

        if (unit == null)
        {
            // 单位质心
            Vector3 centroid = Vector3.zero;
            foreach (var u in units) centroid += u.transform.position;
            centroid /= units.Count;

            // 朝向：由质心指向点击点
            Vector3 dir = (hit - centroid);
            dir.y = 0f; // 仅用 XZ
            if (dir.sqrMagnitude < 1e-4f) dir = Vector3.forward; // 保护
            Quaternion facing = Quaternion.LookRotation(dir.normalized, Vector3.up);
            var slots = planner.GenerateSlots(hit,units.Count,2,facing);
            foreach (var slot in slots)
            {
                DrawPoint(slot,Color.red);
            }
            var validSlots = new List<Vector3>();
            foreach (var s in slots) {
                if (planner.TryProjectToNavMesh(s, out var onNav)) validSlots.Add(onNav);
            }
            if (validSlots.Count < units.Count) {
                Debug.LogWarning($"Not enough valid slots: {validSlots.Count}/{units.Count}");
                // 可以重新生成更多槽位或分批处理
            }
            var map = planner.AssignSlots(units, validSlots);
            
            foreach (var _unit in units)
            {
                if (map.TryGetValue(_unit, out var target))
                {
                    _unit.orderQueue.EnOrder(Order.Move);
                    _unit.attack.CancleAttackOrder();
                    DrawPoint(target,Color.yellow);
                    _unit.move.StartMoveTo(target);
                }
            }
        }
    }

    private void HandleDragLeftClick()
    {
        
    }

    private void HandleScroll()
    {
        
    }

    private void ShowUnitInfo(Unit unit)
    {
        
    }

    void DrawPoint(Vector3 p, Color c, float len=0.4f) {
        Debug.DrawRay(p, Vector3.up * len, c, 10f, false);
        Debug.DrawRay(p, Vector3.right * (len*0.5f), c, 10f, false);
        Debug.DrawRay(p, Vector3.forward * (len*0.5f), c, 10f, false);
    }


    public bool isClick = false;
    public Vector3 hitPoint;
    private void OnDrawGizmos()
    {
        if (isClick)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(hitPoint,0.25f);
        }    
    }
}
