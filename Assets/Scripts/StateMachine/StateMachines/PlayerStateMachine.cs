using GameEvents;
using Sirenix.OdinInspector;
using System.Collections;
using CharacterMovement;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CustomPlayerController))]
public class PlayerStateMachine : StateMachine
{
    [FoldoutGroup("Walking Properties"), SerializeField] public float PlayerMaxWalkSpeed = 5f;
    [FoldoutGroup("Walking Properties"), SerializeField] public float WalkDeccelerationFactor = 1.0f;
    //[FoldoutGroup("Walking Properties"), SerializeField] public float WalkAccelerationFactor = 1.0f;

    //[FoldoutGroup("Running Properties"), SerializeField] public float PlayerMaxRunSpeed = 20f;
    //[FoldoutGroup("Running Properties"), SerializeField] public float RunDeccelerationFactor = 1.0f;
    //[FoldoutGroup("Running Properties"), SerializeField] public float RunAccelerationFactor = 1.0f;

    //[FoldoutGroup("Grounding"), SerializeField] private float _groundCheckDistance = .4f;
    //[FoldoutGroup("Grounding"), SerializeField] private float _groundCheckOffset = .1f;
    //[FoldoutGroup("Grounding"), SerializeField] protected float _maxSlopeAngle = 40f; 
    //[FoldoutGroup("Grounding"), SerializeField] private bool _parentToSurface;
    //[FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] private LayerMask _groundMask;
    //[FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] public bool IsGrounded { get; set; }
    //[FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] private Vector3 _groundCheckStart => transform.position + transform.up * _groundCheckOffset;

    //[field: FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] public GameObject SurfaceObject { get; protected set; }
    //[field: FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 LastGroundPosition { get; protected set; }
    //[field: FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 GroundNormal { get; set; }
    //[field: FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] public float LastGroundTime { get; protected set; }

    //[FoldoutGroup("WallRun"), SerializeField] public float WallRunCheckDistance = 2f;
    //[FoldoutGroup("WallRun"), SerializeField] public float WallRunCheckRadius = 2f;
    //[FoldoutGroup("WallRun"), SerializeField] public LayerMask WallRunLayer;
    //[FoldoutGroup("Running Properties"), SerializeField] public float WallRunAccelerationFactor = 1.0f;

    [field: SerializeField, TabGroup("Properties")] public CustomPlayerController PlayerController => Controller as CustomPlayerController;
    [field: SerializeField, TabGroup("Properties")] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private float _lastDashTime = Mathf.NegativeInfinity;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private float _lastAttackTime = Mathf.NegativeInfinity;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private bool _isAttacking = false;
    [SerializeField, TabGroup("Dashing")] public float DashSpeed = 1000f;
    [ShowInInspector, TabGroup("Dashing")] public bool IsDashing { get;  set; } = false;
    [ShowInInspector, TabGroup("Dashing")] public float DashCooldown { get;  set; } = 2f;
    [ShowInInspector, TabGroup("Dashing")] private Vector3 _dashDirection;
    
    [field: TabGroup("Airborne"), SerializeField] public bool IsJumping { get; set; }
    [field: TabGroup("Airborne"), SerializeField] public float JumpForce = 10f;
    [field: TabGroup("Airborne"), SerializeField] public float DoubleJumpForce = 5f;
    [field: TabGroup("Airborne"), SerializeField] public float AirControl = .9f;
    [field: TabGroup("Airborne"), SerializeField] public int JumpCounter { get; internal set; } = 1;
    [field: TabGroup("Airborne"), SerializeField] public int MaxJumps = 2;

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
    public void Dodge(float DashAnimLength)
    {
        if (!CanMove) return;
        StartCoroutine(DodgeCoroutine(DashAnimLength));
    }

    private IEnumerator DodgeCoroutine(float DashAnimLength)
    {
        if (!CanMove) yield break;
        IsDashing = true;
        if (LocalMoveInput == Vector3.zero) _dashDirection = -1 * transform.forward;
        else _dashDirection = LocalMoveInput.normalized;
        SetLookDirection(_dashDirection);
        rb.AddForce(_dashDirection * DashSpeed);

        yield return new WaitForSeconds(DashAnimLength);

        IsDashing = false;
        yield return null;
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
}
