using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

[RequireComponent(typeof(CustomPlayerController))]
public class PlayerStateMachine : StateMachine
{

    [SerializeField] public CustomPlayerController PlayerController => Controller as CustomPlayerController;

    [field: SerializeField, FoldoutGroup("General"), TabGroup("General/Tabs", "Properties")] public CursorLockMode CursorMode = CursorLockMode.Locked;
    [field: SerializeField, FoldoutGroup("General"), TabGroup("General/Tabs", "Properties"), HideInEditorMode, ReadOnly] public float LastDashTime = Mathf.NegativeInfinity;
    [field: SerializeField, FoldoutGroup("General"), TabGroup("General/Tabs", "Properties"), HideInEditorMode, ReadOnly] public float LastScrollTime = Mathf.NegativeInfinity;
    
    [field: SerializeField, FoldoutGroup("General"), TabGroup("General/Tabs", "Debug")] public bool debugStateTransitions;
    
    
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Basic")] public float PlayerWalkSpeedMultiplier = 1f;
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Basic")] public float PlayerAttackSpeedMultiplier = .5f;
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Basic")] public float PlayerBlockSpeedMultiplier = .1f;
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Basic")] public float PlayerDashSpeedMultiplier = .1f;
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Basic")] public float PlayerAirSpeedMultiplier = 1f;
    
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Dash")] public float DashCooldown { get;  set; } = 2f;
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Dash")] public float DashDistance;
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Dash")] public float DashDuration;
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Dash")] public Vector3 DashDirection;
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Dash")] public float LockDashArc = 30f;
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Dash")] public bool IsDashing { get; set; } = false;
    
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Airborne")] public float LandingGravity = 10f;
    [field: SerializeField, FoldoutGroup("States"), TabGroup("States/Tabs", "Airborne")] public float AirDashMultiplier = 10f;

    [field: SerializeField, FoldoutGroup("General"), TabGroup("General/Tabs","Camera")] public CinemachineCamera TargetLockCam;
    [field: SerializeField, FoldoutGroup("General"), TabGroup("General/Tabs","Camera")] public CinemachineCamera FreeLookCam;
    [field: SerializeField, FoldoutGroup("General"), TabGroup("General/Tabs", "Camera")] public CinemachineTargetGroup TargetGroup;
    [field: SerializeField, FoldoutGroup("General"), TabGroup("General/Tabs", "Weapons")] public GameObject BulletPrefab;

    public override void Awake()
    {
        base.Awake();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //TargetGroup.Targets.Capacity = 2;

        // assign frictionless physic material
#if UNITY_6000_0_OR_NEWER
        Collider.material = new PhysicsMaterial("NoFriction") { staticFriction = 0f, dynamicFriction = 0f, frictionCombine = PhysicsMaterialCombine.Minimum };
#else
            CapsuleCollider.material = new PhysicMaterial("NoFriction") { staticFriction = 0f, dynamicFriction = 0f, frictionCombine = PhysicMaterialCombine.Minimum };
#endif

        // disable NavMeshAgent movement
        NavAgent.updatePosition = false;
        NavAgent.updateRotation = false;
        
        // match-look direction to current facing
        LookDirection = transform.forward;
        PlayerController.SetStateMachine(this);
        SwitchState(new PlayerIdleState(this, true));
        
    }
    public override void Update()
    {
        base.Update();
        IsGrounded = CheckGrounded();
        if (IsGrounded) JumpCounter = 1;
        Vector3 up = Vector3.up;
        Vector3 right = Camera.main.transform.right;
        Vector3 forward = Vector3.Cross(right, up);
        Vector3 moveInput = (forward * PlayerController.MoveInput.y + right * PlayerController.MoveInput.x).normalized;
        MoveInput = moveInput;
        SetMoveInput(moveInput);
        SetLookDirection(moveInput);
        if (LookInCameraDirection)
        {
            if(CurrentTarget == null || !IsTargeting )
            {
                SetLookDirection(Camera.main.transform.forward);
            }
            else
            {             
                LocalMoveInput = transform.InverseTransformDirection(MoveInput).normalized;
                Vector3 targetDir = (CurrentTarget.transform.position - transform.position).normalized;
                targetDir.y = 0f;

                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                Vector3 inputDir = new Vector3(LocalMoveInput.x, 0, LocalMoveInput.z);
                inputDir = targetRot * inputDir;
                inputDir.y = 0;
                inputDir.Normalize();
                MoveInput = new Vector3(inputDir.x,0, inputDir.z);

                SetLookPosition(CurrentTarget.transform.position);
            }
        }
        transform.rotation = Quaternion.LookRotation(LookDirection);
        _currentState?.Tick(Time.deltaTime);
    }


    public void SwitchToMovement(bool shouldFade)
    {
        if (HasMoveInput)
        {
            SwitchState(new PlayerWalkingState(this, shouldFade));
        }
        else
        {
            SwitchState(new PlayerIdleState(this, shouldFade));
        }
    }
    public IEnumerator SwitchToMovementWithDelay(bool shouldFade, float delay)
    {
        yield return new WaitForSeconds(delay);
        SwitchToMovement(shouldFade);
    }
    public void ContinueAttack()
    {
        SwitchState(new PlayerAttackState(this, true));
    }

    public void IncrementVisibleTarget()
    {
        Debug.Log("++");
        List<Targetable> targets = Vision.GetVisibleTargets(0);
        if (CurrentVisibleIndex == targets.Count)
        {
            CurrentVisibleIndex = 0;
        }
        else CurrentVisibleIndex++;
    }

    public void DecrementVisibleTarget()
    {
        Debug.Log("--");
        List<Targetable> targets = Vision.GetVisibleTargets(0);
        if (CurrentVisibleIndex == 0)
        {
            CurrentVisibleIndex = targets.Count;
        }
        else CurrentVisibleIndex--;
    }
}
