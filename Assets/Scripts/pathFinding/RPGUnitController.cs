using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGUnitController : MonoBehaviour
{
    private Camera cam;
    private Unit mySelf;
    private CharacterController controller;
    private CameraControl cameraControl;
    
    
    private void Awake()
    {
        cam = Camera.main;
        mySelf = GetComponent<Unit>();
        controller = GetComponent<CharacterController>();
        cameraControl = cam.GetComponent<CameraControl>();
        
        
        radius = Mathf.Sqrt(offset.y * offset.y + offset.z * offset.z);
    }


    [Header("RPG 速度参数")]
    [Tooltip("RPG 最大速度")]
    public float maxSpeedRPG = 3;
    [Tooltip("RPG 重力")]
    public float gravity = -3;
    private Vector3 velocity;
    [Tooltip("RPG 当前的速度变化")]
    public Vector3 currentSpeed = Vector3.zero;

    private void Update()
    {
        if (GameModeManager.Instance.CurrentGameMode() == GameModeEnum.RPG)
        {
            RPGModeHandler();
        }
    }

    [Space(100)]
    [Header("RPG 相机参数")]
    [Tooltip("RPG 相机位置")]
    public Vector3 offset = new Vector3(0, 5, -3);
    [Tooltip("RPG 相机跟随速度")] public float followSpeed = 5;
    [Tooltip("RPG 相机灵敏度")]
    public float camRotateSensitivity = 0.1f;
    [Tooltip("RPG 相机半径")] public float radius; 
    [Tooltip("RPG 相机半径最小")] public float radiusMin = 3;
    [Tooltip("RPG 相机半径最大")] public float radiusMax = 10;
    [Tooltip("RPG 相机水平角度累积")] public float horiAngle;
    [Tooltip("RPG 相机垂直角度累积")] public float verAngle;
    
    
    private void LateUpdate()
    {
        if (GameModeManager.Instance.CurrentGameMode() == GameModeEnum.RPG)
        {
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                float mouseZ = Input.GetAxis("Mouse ScrollWheel");
                

                horiAngle += mouseX * camRotateSensitivity;
                verAngle += mouseY * camRotateSensitivity;
                
                // 不让它靠近 0 和 π，比如离两极各留 15°
                float minV = Mathf.Deg2Rad * 15f;   // 15°
                float maxV = Mathf.Deg2Rad * 165f;  // 180-15°
                verAngle = Mathf.Clamp(verAngle, minV, maxV);
                
                radius += mouseZ * camRotateSensitivity;
                radius = Mathf.Clamp(radius,radiusMin,radiusMax);
                
                offset = CalculateSphericalOffset(-verAngle, -horiAngle, radius);
                
                RaycastHit hit;
                if (Physics.SphereCast(mySelf.transform.position, cameraControl.coliRadius, offset.normalized, out hit, radius))
                {
                    if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
                    {
                        radius = hit.distance - cameraControl.coliRadius;
                        offset = CalculateSphericalOffset(-verAngle, -horiAngle, radius);
                    }
                }
            }
            
            
            var targetPos = mySelf.transform.position + offset;
            
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, followSpeed * Time.deltaTime);
            cam.transform.position = targetPos;
            cam.transform.LookAt(mySelf.transform.position);
        }
    }

    private Vector3 CalculateSphericalOffset(float vertical, float horizontal, float radius)
    {
        return new Vector3(
            radius * Mathf.Sin(vertical) * Mathf.Cos(horizontal),
            radius * Mathf.Cos(vertical),
            radius * Mathf.Sin(vertical) * Mathf.Sin(horizontal)
        );
    }
    
    
    private void RPGModeHandler()
    {
        
        
        //输入层
        //保存输入  采集键鼠+地形信息+来自怪物/反应系统的“状态请求”
        mySelf.stateMachine.input = default(RPGInput);
        // 键鼠输入
        mySelf.stateMachine.input.moveAxis = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        
        
        mySelf.stateMachine.input.attackPressed = Input.GetMouseButtonDown(0);
        if (mySelf.stateMachine.input.attackPressed)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
    
            // 检测地面层，最大距离1000
            if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Ground")))
            {
                mySelf.stateMachine.input.mouseOnPlane = hit.point;
            }
        }
        
        
        // 地形/物理信息
        mySelf.stateMachine.input.isGrounded = controller.isGrounded;
        mySelf.stateMachine.input.verticalVelocity = currentSpeed.y;

        if (mySelf.stateMachine.isStateLocked)
        {
            mySelf.stateMachine.stateTimer += Time.deltaTime;
            if (mySelf.stateMachine.stateTimer < mySelf.stateMachine.stateDuration)
            {
                return;
            }
            else
            {
                mySelf.stateMachine.isStateLocked = false;
                mySelf.stateMachine.stateTimer = 0;
            }
        }
        
        
        
        //状态机
        //根据输入  按照优先级切换状态(这里会发出动画指令)  ——外部不能调，只在input里面加   只有这里做判断
        var state = mySelf.stateMachine.CalculateInput(mySelf.stateMachine.input);
        mySelf.stateMachine.SwitchState(state);
        
        
        //执行层
        //根据状态 执行逻辑
        switch (state)
        {
            case RPGState.Idle:
                break;
            case RPGState.Move:
                RPGMoveAndRotate();
                break;
            case RPGState.Attack:
                Vector3 mouseOnPlane = mySelf.stateMachine.input.mouseOnPlane;
                Vector3 myPos = mySelf.transform.position;
                Vector3 direction = mouseOnPlane - myPos;
                direction.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                mySelf.transform.rotation = targetRotation;
                GameModeManager.Instance.Attack(mySelf);
                mySelf.stateMachine.isStateLocked = true;
                break;
            case RPGState.Dash:
                ;
                break;
            case RPGState.Skill:
                ;
                break;
            case RPGState.Roll:
                ;
                break;
        }
        
    }
    
    private void RPGMoveAndRotate()
    {
        var hor = Input.GetAxis("Horizontal");
        var ver = Input.GetAxis("Vertical");
        
        var camForwardOnPlane = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        var camRightOnPlane = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
        
        var moveDir = camForwardOnPlane * ver + camRightOnPlane * hor;

        if (moveDir.magnitude > 1)
        {
            moveDir.Normalize();
        }
        
        var tagretSpeed = moveDir * maxSpeedRPG;
        
        currentSpeed = Vector3.SmoothDamp(currentSpeed, tagretSpeed, ref velocity, 0.3f);

        if (controller.isGrounded && currentSpeed.y < 0)
        {
            currentSpeed.y = -1;
        }
        else
        {
            currentSpeed.y += gravity * Time.deltaTime;
        }

        if (moveDir.magnitude > 0)
        {
            var currentForward = mySelf.transform.forward;
            var targetForward = moveDir;
            
            float angle = Vector3.Angle(currentForward, targetForward);
            float maxRotateSpeed = 360f;
            float rotateThisFrame = Mathf.Min(angle, maxRotateSpeed * Time.deltaTime);
            
            mySelf.transform.rotation = Quaternion.RotateTowards(
                mySelf.transform.rotation, 
                Quaternion.LookRotation(targetForward), 
                rotateThisFrame
            );
        }
        
        controller.Move(currentSpeed * Time.deltaTime);
    }
}
