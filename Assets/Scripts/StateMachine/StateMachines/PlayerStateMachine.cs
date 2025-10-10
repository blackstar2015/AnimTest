using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CustomPlayerController))]
public class PlayerStateMachine : StateMachine
{
    [field: SerializeField, TabGroup("Properties")] public float PlayerMaxWalkSpeed = 5f;

    [field: SerializeField, TabGroup("Properties")] public CustomPlayerController PlayerController => Controller as CustomPlayerController;
    [field: SerializeField, TabGroup("Properties")] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] public float LastDashTime = Mathf.NegativeInfinity;
    [field: SerializeField, TabGroup("Properties")] public bool debugStateTransitions = false;

    [ShowInInspector, TabGroup("Movement","Dashing")] public float DashSpeed = 1000f;
    [ShowInInspector, TabGroup("Movement","Dashing")] public float DashCooldown { get;  set; } = 2f;
    [ShowInInspector, TabGroup("Movement","Dashing")] public Vector3 DashDirection;

    [ShowInInspector, TabGroup("Movement", "Airborne")] public float LandingGravity = 10f;
    [ShowInInspector, TabGroup("Movement", "Airborne")] public float AirDashMultiplier = 10f;

    [ShowInInspector, TabGroup("Movement", "AnimHashes")] public static string Idle = "Idle";
    [ShowInInspector, TabGroup("Movement", "AnimHashes")] public static string Movement = "Movement";
    [ShowInInspector, TabGroup("Movement", "AnimHashes")] public static string AirborneJump = "AirborneJump";
    [ShowInInspector, TabGroup("Movement", "AnimHashes")] public static string AirborneFlip = "AirborneFlip";
    [ShowInInspector, TabGroup("Movement", "AnimHashes")] public static string AirborneFall = "AirborneFall";
    [ShowInInspector, TabGroup("Movement", "AnimHashes")] public static string AirborneLand = "AirborneLand";
    [ShowInInspector, TabGroup("Movement", "AnimHashes")] public static string AirborneDash = "AirborneDash";
    [ShowInInspector, TabGroup("Movement", "AnimHashes")] public static string Dash = "Dodge";
    [ShowInInspector, TabGroup("Movement", "AnimHashes")] public static string Block = "Block";
    [ShowInInspector, TabGroup("Movement", "AnimHashes")] public static string Attack = "Attack";
    [TabGroup("Movement", "AnimHashes")] public readonly int IdleHash = Animator.StringToHash(Idle);
    [TabGroup("Movement", "AnimHashes")] public readonly int MovementHash = Animator.StringToHash(Movement);
    [TabGroup("Movement", "AnimHashes")] public readonly int AirborneJumpHash = Animator.StringToHash(AirborneJump);
    [TabGroup("Movement", "AnimHashes")] public readonly int AirborneFlipHash = Animator.StringToHash(AirborneFlip);
    [TabGroup("Movement", "AnimHashes")] public readonly int AirborneFallHash = Animator.StringToHash(AirborneFall);
    [TabGroup("Movement", "AnimHashes")] public readonly int AirborneLandHash = Animator.StringToHash(AirborneLand);
    [TabGroup("Movement", "AnimHashes")] public readonly int AirborneDashHash = Animator.StringToHash(AirborneDash);
    [TabGroup("Movement", "AnimHashes")] public readonly int DodgeHash = Animator.StringToHash(Dash);
    [TabGroup("Movement", "AnimHashes")] public readonly int BlockHash = Animator.StringToHash(Block);
    [TabGroup("Movement", "AnimHashes")] public readonly int AttackHash = Animator.StringToHash(Attack);

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

        // match look direction to current facing
        LookDirection = transform.forward;
        PlayerController.SetStateMachine(this);
        SwitchState(new PlayerIdleState(this));
        
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


    public void SwitchToMovement()
    {
        if (rb.linearVelocity.magnitude >= .1f)
        {
            SwitchState(new PlayerWalkingState(this));
        }
        else
        {
            SwitchState(new PlayerIdleState(this));
        }
    }

    public void ContinueAttack()
    {
        SwitchState(new PlayerAttackState(this));
    }
}
