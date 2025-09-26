using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    [field: FoldoutGroup("Components"), ReadOnly, HideInEditorMode, SerializeField] public Rigidbody rb { get; protected set; }
    [field: FoldoutGroup("Components"), ReadOnly, HideInEditorMode, SerializeField] public CustomPlayerController Controller { get; protected set; }
    [field: FoldoutGroup("Components"), ReadOnly, HideInEditorMode, SerializeField] public Animator Animator { get; protected set; }
    [field: FoldoutGroup("Components"), ReadOnly, HideInEditorMode, SerializeField] public CapsuleCollider Collider { get; protected set; }

    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 Momentum => rb.linearVelocity;
    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 MoveInput { get; set; }
    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 LocalMoveInput { get; set; }
    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 LookDirection { get; protected set; }
    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 SurfaceVelocity { get; set; }
    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public bool HasMoveInput { get; set; }

    [field: FoldoutGroup("Jumping Properties"), SerializeField] public bool IsJumping { get; set; }
    [field: FoldoutGroup("Jumping Properties"), SerializeField] public float JumpForce = 10f;
    [field: FoldoutGroup("Jumping Properties"), SerializeField] public float DoubleJumpForce = 5f;
    [field: FoldoutGroup("Jumping Properties"), SerializeField] public float AirControl = .9f;
    [field: FoldoutGroup("Jumping Properties"), SerializeField] public int JumpCounter { get; internal set; } = 1;
    [field: FoldoutGroup("Jumping Properties"), SerializeField] public int MaxJumps = 2;



    [FoldoutGroup("Walking Properties"), SerializeField] public float PlayerMaxWalkSpeed = 10f;
    [FoldoutGroup("Walking Properties"), SerializeField] public float WalkDeccelerationFactor = 1.0f;
    [FoldoutGroup("Walking Properties"), SerializeField] public float WalkAccelerationFactor = 1.0f;

    [FoldoutGroup("Running Properties"), SerializeField] public float PlayerMaxRunSpeed = 20f;
    [FoldoutGroup("Running Properties"), SerializeField] public float RunDeccelerationFactor = 1.0f;
    [FoldoutGroup("Running Properties"), SerializeField] public float RunAccelerationFactor = 1.0f;

    [FoldoutGroup("Grounding"), SerializeField] private float _groundCheckDistance = .4f;
    [FoldoutGroup("Grounding"), SerializeField] private float _groundCheckOffset = .1f;
    [FoldoutGroup("Grounding"), SerializeField] protected float _maxSlopeAngle = 40f; 
    [FoldoutGroup("Grounding"), SerializeField] private bool _parentToSurface;
    [FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] private LayerMask _groundMask;
    [FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] public bool IsGrounded { get; set; }
    [FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] private Vector3 _groundCheckStart => transform.position + transform.up * _groundCheckOffset;

    [field: FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] public GameObject SurfaceObject { get; protected set; }
    [field: FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 LastGroundPosition { get; protected set; }
    [field: FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] public Vector3 GroundNormal { get; set; }
    [field: FoldoutGroup("Grounding"), ReadOnly, HideInEditorMode, SerializeField] public float LastGroundTime { get; protected set; }

    [FoldoutGroup("WallRun"), SerializeField] public float WallRunCheckDistance = 2f;
    [FoldoutGroup("WallRun"), SerializeField] public float WallRunCheckRadius = 2f;
    [FoldoutGroup("WallRun"), SerializeField] public LayerMask WallRunLayer;
    [FoldoutGroup("Running Properties"), SerializeField] public float WallRunAccelerationFactor = 1.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Controller = GetComponent<CustomPlayerController>();
        Animator = GetComponent<Animator>();
        Collider = GetComponent<CapsuleCollider>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        LookDirection = transform.forward;
        SwitchState(new PlayerIdleState(this));
    }

    // sets character look direction, flattening y-value
    protected void SetLookDirection(Vector3 direction)
    {
        LookDirection = new Vector3(direction.x, 0f, direction.z).normalized;
    }

    protected void SetLookPosition(Vector3 position)
    {
        Vector3 direction = Vector3.ClampMagnitude(position - transform.position, 1f);
        SetLookDirection(direction);
    }

    public void SetMoveInput(Vector3 input)
    {
        input = Vector3.ClampMagnitude(input, 1f);
        HasMoveInput = input.magnitude > 0.1f;
        input.x = HasMoveInput ? input.x : 0;
        input.y = IsJumping ? JumpForce : 0;
        input.z = HasMoveInput ? input.z : 0;
        Vector3 flattened = new Vector3(input.x, 0, input.z);
        MoveInput = flattened;
        LocalMoveInput = transform.InverseTransformDirection(MoveInput);

    }

    public bool CheckGrounded()
    {
        bool hit = Physics.Raycast(_groundCheckStart, -transform.up, out RaycastHit hitInfo, _groundCheckDistance, _groundMask);

        GroundNormal = Vector3.up;
        SurfaceVelocity = Vector3.zero;

        if(!hit) return false;

        if(hitInfo.rigidbody != null)
        {
            SurfaceVelocity = hitInfo.rigidbody.linearVelocity;
        }

        bool validAngle = Vector3.Angle(transform.up,hitInfo.normal) < _maxSlopeAngle;

        if(validAngle)
        {
            LastGroundTime = Time.timeSinceLevelLoad;
            GroundNormal = hitInfo.normal;
            LastGroundPosition = transform.position;
            SurfaceObject = hitInfo.collider.gameObject;
            if(_parentToSurface) transform.SetParent(SurfaceObject.transform);  
            return true;
        }
        SurfaceObject = null;
        if(_parentToSurface) transform.SetParent(null);
        return false;
    }

    public override void Update()
    {
        base.Update();
        IsGrounded = CheckGrounded();
        if (IsGrounded) JumpCounter = 1;
        Vector3 up = Vector3.up;
        Vector3 right = Camera.main.transform.right;
        Vector3 forward = Vector3.Cross(right, up);
        Vector3 moveInput = forward * Controller.MoveInput2D.y + right * Controller.MoveInput2D.x; 
        SetMoveInput(moveInput);
        SetLookDirection(moveInput);
        if (Controller.LookInCameraDirection) SetLookDirection(Camera.main.transform.forward);
        transform.rotation = Quaternion.LookRotation(LookDirection);
    }
}
