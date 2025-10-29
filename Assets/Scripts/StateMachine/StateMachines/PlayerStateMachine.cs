using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomPlayerController))]
public class PlayerStateMachine : StateMachine
{
    [field: SerializeField, TabGroup("Properties")] public CustomPlayerController PlayerController => Controller as CustomPlayerController;
    [field: SerializeField, TabGroup("Properties")] protected CursorLockMode CursorMode = CursorLockMode.Locked;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] public float LastDashTime = Mathf.NegativeInfinity;
    [field: SerializeField, TabGroup("Properties")] public bool debugStateTransitions;
    [field: SerializeField, TabGroup("Movement", "Speed")] public float PlayerWalkSpeedMultiplier = 1f;
    [field: SerializeField, TabGroup("Movement", "Speed")] public float PlayerAttackSpeedMultiplier = .5f;
    [field: SerializeField, TabGroup("Movement", "Speed")] public float PlayerBlockSpeedMultiplier = .1f;
    [field: SerializeField, TabGroup("Movement", "Speed")] public float PlayerDashSpeedMultiplier = .1f;
    [field: SerializeField, TabGroup("Movement", "Speed")] public float PlayerAirSpeedMultiplier = 1f;
    [ShowInInspector, TabGroup("Movement", "Basic")] public float CrossFadeDuration = 0f;
    [ShowInInspector, TabGroup("Movement","Dashing")] public float DashSpeed = 1000f;
    [ShowInInspector, TabGroup("Movement","Dashing")] public float DashCooldown { get;  set; } = 2f;
    [ShowInInspector, TabGroup("Movement","Dashing")] public Vector3 DashDirection;
    [ShowInInspector, TabGroup("Movement", "Airborne")] public float LandingGravity = 10f;
    [ShowInInspector, TabGroup("Movement", "Airborne")] public float AirDashMultiplier = 10f;

    public override void Awake()
    {
        base.Awake();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        Vector3 moveInput = forward * PlayerController.MoveInput.y + right * PlayerController.MoveInput.x;
        MoveInput = moveInput;
        SetMoveInput(moveInput);
        SetLookDirection(moveInput);
        if (LookInCameraDirection) SetLookDirection(Camera.main.transform.forward);
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
        if (Vision.CurrentVisibleIndex == targets.Count - 1)
        {
            Vision.CurrentVisibleIndex = 0;
        }
        else Vision.CurrentVisibleIndex++;
    }

    public void DecrementVisibleTarget()
    {
        Debug.Log("--");
        List<Targetable> targets = Vision.GetVisibleTargets(0);
        if (Vision.CurrentVisibleIndex == 0)
        {
            Vision.CurrentVisibleIndex = targets.Count -1;
        }
        else Vision.CurrentVisibleIndex--;
    }
}
