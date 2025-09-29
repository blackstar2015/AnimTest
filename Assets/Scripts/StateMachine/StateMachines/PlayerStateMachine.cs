using GameEvents;
using Sirenix.OdinInspector;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    //[field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 Momentum => rb.linearVelocity;
    //[field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 MoveInput { get; set; }
    //[field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 LocalMoveInput { get; set; }
    //[field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 LookDirection { get; protected set; }
    //[field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 SurfaceVelocity { get; set; }
    //[field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public bool HasMoveInput { get; set; }

    //[field: FoldoutGroup("Jumping Properties"), SerializeField] public bool IsJumping { get; set; }
    //[field: FoldoutGroup("Jumping Properties"), SerializeField] public float JumpForce = 10f;
    //[field: FoldoutGroup("Jumping Properties"), SerializeField] public float DoubleJumpForce = 5f;
    //[field: FoldoutGroup("Jumping Properties"), SerializeField] public float AirControl = .9f;
    //[field: FoldoutGroup("Jumping Properties"), SerializeField] public int JumpCounter { get; internal set; } = 1;
    //[field: FoldoutGroup("Jumping Properties"), SerializeField] public int MaxJumps = 2;



    //[FoldoutGroup("Walking Properties"), SerializeField] public float PlayerMaxWalkSpeed = 10f;
    //[FoldoutGroup("Walking Properties"), SerializeField] public float WalkDeccelerationFactor = 1.0f;
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

    [field: SerializeField, TabGroup("Properties")] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private float _lastDashTime = Mathf.NegativeInfinity;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private float _lastAttackTime = Mathf.NegativeInfinity;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private bool _isAttacking = false;
    [SerializeField, TabGroup("Dashing")] private float _dashSpeed = 1000f;
    [ShowInInspector, TabGroup("Dashing")] public bool IsDashing { get; private set; } = false;
    [ShowInInspector, TabGroup("Dashing")] public float DashCooldown { get; private set; } = 2f;
    private Vector3 _dashDirection;

    public override void Awake()
    {
        base.Awake();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SwitchState(new PlayerIdleState(this));
    }
    public void Dash(float DashAnimLength)
    {
        if (!CanMove) return;
        StartCoroutine(DashCoroutine(DashAnimLength));
    }

    private IEnumerator DashCoroutine(float DashAnimLength)
    {
        if (!CanMove) yield break;
        IsDashing = true;
        if (LocalMoveInput == Vector3.zero) _dashDirection = -1 * transform.forward;
        else _dashDirection = LocalMoveInput.normalized;
        SetLookDirection(_dashDirection);
        rb.AddForce(_dashDirection * _dashSpeed);

        yield return new WaitForSeconds(DashAnimLength);

        IsDashing = false;
        yield return null;
    }
}
