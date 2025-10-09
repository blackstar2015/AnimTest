using RPGCharacterAnims.Actions;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Splines;
using static Sirenix.OdinInspector.Editor.Internal.FastDeepCopier;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CustomCharacterAnimations))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Targetable))]
[RequireComponent(typeof(Vision))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
public class StateMachine : MonoBehaviour
{
    protected State _currentState {  get; set; }
    public string CurrentState => _currentState.ToString();

    #region Components
    [field: SerializeField, TabGroup("Components")] protected CustomController Controller { get; set; }
    [field: SerializeField, TabGroup("Components")] public Animator Animator { get; set; }
    [field: SerializeField, TabGroup("Components")] public Health Health { get; set; }
    [field: SerializeField, TabGroup("Components")] public Targetable Targetable { get; set; }
    [field: SerializeField, TabGroup("Components")] public Vision Vision { get; set; }
    [field: SerializeField, TabGroup("Components")] public Rigidbody rb { get; set; }
    [field: SerializeField, TabGroup("Components")] public CapsuleCollider Collider { get; set; }
    [field: SerializeField, TabGroup("Components")] public NavMeshAgent NavAgent { get; set; }
    #endregion
    #region Weapons
    [field: SerializeField, InlineButton(nameof(FindWeapons), "Find"), TabGroup("Weapons")] public Weapon[] Weapons { get; private set; }
    #endregion
    #region Movement
    [field: SerializeField, TabGroup("Movement","Basic")] public float Speed { get; set; } = 5f;
    [field: SerializeField, TabGroup("Movement", "Basic")] public float Acceleration { get; set; } = 10f;
    [field: SerializeField, TabGroup("Movement", "Basic")] public float TurnSpeed { get; set; } = 15f;
    [field: SerializeField, TabGroup("Movement", "Basic")] public bool OnlyTurnWithInput { get; set; } = true;
    [field: SerializeField, TabGroup("Movement", "Basic")] public float StoppingDistance { get; set; } = 0.25f;
    [field: SerializeField, TabGroup("Movement", "Basic")] public bool LookInMoveDirection { get; set; } = true;
    [field: SerializeField, TabGroup("Movement", "Basic")] public bool ControlRotation { get; set; } = true;       // character turns towards movement direction
    [field: SerializeField, TabGroup("Movement", "Basic")] public bool Fix3DSpriteRotation { get; set; } = false;
    [field: SerializeField, TabGroup("Movement", "Basic")] public bool ParentToSurface { get; set; } = false;
    [field: SerializeField, TabGroup("Movement", "Idle")] public AnimationCurve IdleSpeedCurve;
    [field: SerializeField, TabGroup("Movement", "Idle")] public float WalkDeccelerationFactor = 1.0f;
    #endregion
    #region Airborne
    [field: SerializeField, TabGroup("Movement","Airborne")] public float Gravity { get; set; } = -20f;             // custom gravity value
    [field: SerializeField, TabGroup("Movement","Airborne")] public float JumpHeight { get; set; } = 2.25f;         // peak height of jump  
    [field: SerializeField, TabGroup("Movement", "Airborne")] public bool AirTurning { get; set; } = true;
    [field: TabGroup("Movement", "Airborne"), SerializeField] public bool IsJumping { get; set; }
    [field: TabGroup("Movement", "Airborne"), SerializeField] public float JumpForce = 10f;
    [field: TabGroup("Movement", "Airborne"), SerializeField] public float DoubleJumpForce = 5f;
    [field: TabGroup("Movement", "Airborne"), SerializeField] public float AirControl = .9f;
    [field: TabGroup("Movement", "Airborne"), SerializeField] public int JumpCounter { get; internal set; } = 1;
    [field: TabGroup("Movement", "Airborne"), SerializeField] public int MaxJumps = 2;// character can turn while airborne
    #endregion
    #region Size
    [field: SerializeField, TabGroup("Size")] public float Height { get; set; } = 1.8f;
    [field: SerializeField, TabGroup("Size")] public float Radius { get; set; } = 0.3f;
    #endregion
    #region Grounding
    [field: SerializeField, TabGroup("Movement","Grounding")] public float GroundCheckOffset { get; set; } = 0.1f;         // height inside character where grounding ray starts
    [field: SerializeField, TabGroup("Movement","Grounding")] public float GroundCheckDistance { get; set; } = 0.4f;       // distance down from offset position
    [field: SerializeField, TabGroup("Movement","Grounding")] public float MaxSlopeAngle { get; set; } = 40f;              // maximum climbable slope, character will slip on anything higher
    [field: SerializeField, TabGroup("Movement","Grounding")] public float CoyoteMaxJumpDistance { get; set; } = 0.5f;     // max distance allowed after leaving ground when doing a coyote jump
    [field: SerializeField, TabGroup("Movement","Grounding")] public LayerMask GroundMask { get; set; } = 1 << 0;          // mask for layers considered the ground
    [field: SerializeField, TabGroup("Movement", "Grounding")] public float MinGroundedVelocity { get; set; } = 5f;
    [ShowInInspector, TabGroup("Movement", "Dashing")] public bool IsDashing { get; set; } = false;
    #endregion
    #region Events
    [TabGroup("Events")] public UnityEvent<GameObject> OnGrounded;
    [TabGroup("Events")] public UnityEvent<GameObject> OnFootstep;
    #endregion
    #region Properties
    // public properties
#if UNITY_6000_0_OR_NEWER
    [TabGroup("Properties")] public Vector3 Velocity { get => rb.linearVelocity;  set => rb.linearVelocity = value; }
#else
        [ShowInInspector, TabGroup("Properties")] public override Vector3 Velocity { get => Rigidbody.velocity; protected set => Rigidbody.velocity = value; }
#endif
    [TabGroup("Properties")] public float MoveSpeedMultiplier { get; set; } = 1f;
    [TabGroup("Properties")] public bool CanCoyoteJump => LastGroundedDistance < CoyoteMaxJumpDistance;
    [TabGroup("Properties")] public float LastGroundedDistance => Vector3.Distance(transform.position, LastGroundedPosition);
    [TabGroup("Properties")] public Vector3 FlattenedVelocity => new Vector3(Velocity.x, 0f, Velocity.z);
    [TabGroup("Properties")] public float NormalizedSpeed => FlattenedVelocity.magnitude / Speed;
    [TabGroup("Properties")] public Vector3 MoveInput { get; set; }
    [TabGroup("Properties")] public Vector3 LocalMoveInput { get;  set; }
    [TabGroup("Properties")] public Vector3 LookDirection { get;  set; }
    [TabGroup("Properties")] public bool HasMoveInput { get;  set; }
    [TabGroup("Properties")] public bool HasTurnInput { get; set; }
    [TabGroup("Properties")] public bool IsGrounded { get; set; }
    [TabGroup("Properties")] public GameObject SurfaceObject { get;    set; }
    [TabGroup("Properties")] public Vector3 SurfaceVelocity { get; set; }
    [TabGroup("Properties")] public bool CanMove { get; set; } = true;
    [TabGroup("Properties")] public bool CanTurn { get; set; } = true;
    [TabGroup("Properties")] public Vector3 GroundNormal { get; set;  } = Vector3.up;
    [TabGroup("Properties")] public float LastGroundedTime { get; set; }
    [TabGroup("Properties")] public Vector3 LastGroundedPosition { get; set; }
    [TabGroup("Properties")] public float TurnSpeedMultiplier { get; set; } = 1f;
    [TabGroup("Properties")] public Vector3 GroundCheckStart => transform.position + transform.up * GroundCheckOffset;
    [TabGroup("Properties")] public Vector3 SplineLookDirection { get; set; }
    [TabGroup("Properties")] public bool HasPath => NavAgent.hasPath;
    [TabGroup("Properties")] public bool HasCompletePath => NavAgent.hasPath && Vector3.Distance(NavAgent.path.corners[NavAgent.path.corners.Length - 1], NavAgent.destination) < StoppingDistance;
    [field: SerializeField, TabGroup("Properties")] public bool LookInCameraDirection { get; set; }
    [field: SerializeField, TabGroup("Properties")] public int actionIndex = 1;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] public int weaponIndex = 0;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] public bool CanShoot { get; set; } = true;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] public bool CanMelee { get; set; } = true;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] public float LastAttackTime = Mathf.NegativeInfinity;
    //[field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private bool _isAttacking = false;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] public bool IsAttacking { get; internal set; }
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly] public bool IsBlocking => isBlocking;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly] public bool IsAlive => isAlive;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly] public bool CanBlock => canBlock;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly] public bool IsHitReacting => isHitReacting;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly] public bool IsBlockedAttack => isBlockedAttack;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly] public bool IsPerfectBlocking => isPerfectBlocking;
    public bool isBlocking { get; set; }
    public bool canBlock { get; set; }
    public bool isHitReacting { get; set; }
    public bool isAlive { get; set; }
    public bool isBlockedAttack { get; set; }
    public bool isPerfectBlocking { get; set; }

    public int CurrentWeaponIndex => weaponIndex;
    public int CurrentActionIndex => actionIndex;
    #endregion
    #region SetHeight, Splines and avoidance
    // step height fields
    [field: SerializeField, TabGroup("Step Height")] protected float StepHeight { get; set; } = 0.3f;
    [field: SerializeField, TabGroup("Step Height")] protected float StepHeightAllowance { get; set; } = 0.1f;
    [field: SerializeField, TabGroup("Step Height")] protected float StepHeightForwardOffset { get; set; } = 0.05f;

    // all avoidance fields
    [field: Header("Avoidance")]
    [field: SerializeField, Range(0f, 1f), TabGroup("Avoidance")] protected float SpeedVariation { get; set; } = 0.5f;
    [field: SerializeField, TabGroup("Avoidance")] public bool EnableAvoidance { get; set; } = false;
    [field: SerializeField, TabGroup("Avoidance")] public float NeighborDistance { get; set; } = 3f;
    [field: SerializeField, TabGroup("Avoidance")] public float CornerNeighborDistance { get; set; } = 1f;
    [field: SerializeField, TabGroup("Avoidance")] public LayerMask NeighborMask { get; set; }
    [field: SerializeField, TabGroup("Avoidance")] public int MaxNeighbors { get; set; } = 8;
    [field: SerializeField, TabGroup("Avoidance")] public bool IsClampedToNavMesh { get; set; } = true;
    [field: SerializeField, TabGroup("Avoidance")] public float ClampLookAheadTime { get; set; } = 0.25f;
    [field: SerializeField, TabGroup("Avoidance")] public float ClampSearchRadius { get; set; } = 1f;
    [TabGroup("Avoidance")] protected float _variationNoiseOffset;
    [TabGroup("Avoidance")] protected Collider[] _neighborHits;

    [field: SerializeField, TabGroup("Spline Constraint")] public bool EnableSplineConstraint { get; set; }
    [field: SerializeField, TabGroup("Spline Constraint")] public SplineContainer SplineContainer { get; set; }
    [field: SerializeField, TabGroup("Spline Constraint")] public float SplineGravitation { get; set; } = 20f;
    #endregion

    public virtual void Awake()
    {
        Controller = GetComponent<CustomController>();
        rb = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();
        Collider = GetComponent<CapsuleCollider>();
        NavAgent = GetComponent<NavMeshAgent>();
        Health = GetComponent<Health>();
        Targetable = GetComponent<Targetable>();
        Vision = GetComponent<Vision>();
        isAlive = Health.IsAlive;
        canBlock = Health.CanBlock;
        isHitReacting = Health.IsHitReacting;
        Health.OnBlock.AddListener(BlockedAttack);
        Health.OnDeath.AddListener(Death);
        Health.OnDamage.AddListener(Knockback);
    }

    public void SwitchState(State newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    public virtual void FootstepAnimEvent(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && IsGrounded && NormalizedSpeed > 0.05f) OnFootstep.Invoke(SurfaceObject);
    }

    public void SetMoveInput(Vector3 input)
    {
        if (!CanMove)
        {
            MoveInput = Vector3.zero;
            return;
        }

        input = Vector3.ClampMagnitude(input, 1f);
        // set input to 0 if small incoming value
        HasMoveInput = input.magnitude > 0.1f;
        input = HasMoveInput ? input : Vector3.zero;
        // remove y component of movement but retain overall magnitude
        Vector3 flattened = new Vector3(input.x, 0f, input.z);
        flattened = flattened.normalized * input.magnitude;
        MoveInput = flattened;
        // finds movement input as local direction rather than world direction
        LocalMoveInput = transform.InverseTransformDirection(MoveInput);
    }

    public void SetLookDirection(Vector3 direction)
    {
        if (!CanTurn || direction.magnitude < 0.1f)
        {
            HasTurnInput = false;
            return;
        }
        HasTurnInput = true;
        LookDirection = new Vector3(direction.x, 0f, direction.z).normalized;
    }

    public void SetLookPosition(Vector3 position)
    {
        Vector3 direction = Vector3.ClampMagnitude(position - transform.position, 1f);
        SetLookDirection(direction);
    }

    public virtual void TryJump()
    {
        if (!CanMove || !CanCoyoteJump) return;
        Jump();
    }

    public void Jump()
    {
        // calculate jump velocity from jump height and gravity
        float jumpVelocity = Mathf.Sqrt(2f * -Gravity * JumpHeight);
        // override current y velocity but maintain x/z velocity
        Velocity = new Vector3(Velocity.x, jumpVelocity, Velocity.z);
    }

    public virtual void MoveTo(Vector3 destination)
    {
        if (!NavAgent.isActiveAndEnabled || !NavAgent.isOnNavMesh) return;
        NavAgent.SetDestination(destination);
    }

    public virtual void Stop()
    {
        SetMoveInput(Vector3.zero);
        if (!NavAgent.isActiveAndEnabled || !NavAgent.isOnNavMesh) return;
        NavAgent.ResetPath();
    }

    public virtual void Update()
    {
        IsGrounded = CheckGrounded();
        
        // rotates character towards movement direction
        if (ControlRotation && (HasTurnInput || !OnlyTurnWithInput) && (IsGrounded || AirTurning))
        {
            Quaternion rotation = rb.rotation;
            if (!Fix3DSpriteRotation)
            {
                if (EnableSplineConstraint && HasMoveInput) LookDirection = SplineLookDirection;
                Quaternion targetRotation = Quaternion.LookRotation(LookDirection);
                rotation = Quaternion.Slerp(transform.rotation, targetRotation, TurnSpeed * TurnSpeedMultiplier * Time.deltaTime);
            }   // rotate sprite character properly
            else if (Fix3DSpriteRotation && Mathf.Abs(MoveInput.x) > 0.2f)
            {
                float spriteAngle = LookDirection.x > 0 ? 0f : 180f;
                rotation = Quaternion.Euler(0f, spriteAngle, 0f);
            }
            rb.MoveRotation(rotation);
            transform.rotation = rotation;
        }
        // overrides current input with pathing direction if MoveTo has been called
        if (NavAgent.hasPath && NavAgent.pathStatus != NavMeshPathStatus.PathInvalid)
        {
            Vector3 nextPathPoint = NavAgent.steeringTarget;
            Vector3 lastPathPoint = NavAgent.path.corners[NavAgent.path.corners.Length - 1];
            float lastPointDistance = Vector3.Distance(lastPathPoint, transform.position);
            bool pathEndReached = lastPointDistance < StoppingDistance;
            Vector3 pathDir = (nextPathPoint - transform.position).normalized;
            // override direction if avoidance is enabled
            if (EnableAvoidance)
            {
                float neighborDistance = NeighborDistance;
                if (NavAgent.path.corners.Length > 2) neighborDistance = CornerNeighborDistance;
                pathDir = GetAvoidanceDirection(nextPathPoint, neighborDistance);

                if (IsClampedToNavMesh)
                {
                    Vector3 pathPoint = transform.position + pathDir * Speed * ClampLookAheadTime;
                    Vector3 clampedPathPoint = ClampToNavMesh(pathPoint, ClampSearchRadius);
                    pathDir = (clampedPathPoint - transform.position).normalized;
                }
            }
            SetMoveInput(pathDir);
            if (LookInMoveDirection) SetLookDirection(pathDir);

            bool destinationReached = Vector3.Distance(NavAgent.destination, transform.position) < StoppingDistance;
            // stop off destination reached
            if (pathEndReached || (StoppingDistance > 0f && destinationReached))
            {
                SetLookPosition(NavAgent.destination);
                Stop();
            }
        }

        // syncs navmeshagent position with character position
        NavAgent.nextPosition = transform.position;
        NavAgent.Warp(transform.position);

        // find flattened movement vector based on ground normal
        Vector3 input = MoveInput;
        Vector3 right = Vector3.Cross(transform.up, input);
        Vector3 forward = Vector3.Cross(right, GroundNormal);

        // move character along spline
        if (EnableSplineConstraint && SplineContainer != null)
        {
            // spline closest point and tangent
            Spline spline = SplineContainer.Spline;
            Vector3 splineRelativePosition = SplineContainer.transform.InverseTransformPoint(transform.position);
            SplineUtility.GetNearestPoint(spline, splineRelativePosition, out float3 nearest, out float t);
            Vector3 splineWorldPosition = SplineContainer.transform.TransformPoint(nearest);
            Vector3 splineTangent = SplineUtility.EvaluateTangent(spline, t);
            splineTangent.y = 0f;
            splineTangent.Normalize();

            // float direction to closest point
            Vector3 dirToSplineCenter = splineWorldPosition - transform.position;
            dirToSplineCenter.y = 0f;
            float splineFlatDistance = dirToSplineCenter.magnitude;
            dirToSplineCenter.Normalize();

            // force bringing character back to spline center
            float gravitationDot = Vector3.Dot(splineTangent, dirToSplineCenter);
            float gravitationCorrection = 1f - Math.Abs(gravitationDot);
            float sideInput = Vector3.Dot(MoveInput, splineTangent);
            rb.AddForce(gravitationCorrection * Mathf.Clamp01(splineFlatDistance) * SplineGravitation * dirToSplineCenter);

            // correct movement direction along spline
            forward = MoveInput.magnitude * sideInput * splineTangent;
            SplineLookDirection = splineTangent * Mathf.Sign(sideInput);
        }

        // vary character speed when using avoidance
        float speed = Speed;
        if (EnableAvoidance)
        {
            float noise = Mathf.PerlinNoise(Time.time, _variationNoiseOffset) * 2f - 1f;
            speed = Speed * (1f + noise * SpeedVariation);
        }

        // calculates desirection movement velocity
        Vector3 targetVelocity = forward * (speed * MoveSpeedMultiplier);
        if (!CanMove) targetVelocity = Vector3.zero;
        // adds velocity of surface under character, if character is stationary
        targetVelocity += SurfaceVelocity * (1f - Mathf.Abs(MoveInput.magnitude));
        // calculates acceleration required to reach desired velocity and applies air control if not grounded
        Vector3 velocityDiff = targetVelocity - Velocity;
        velocityDiff.y = 0f;
        float control = IsGrounded ? 1f : AirControl;
        Vector3 acceleration = velocityDiff * (Acceleration * control);
        // zeros acceleration if airborne and not trying to move (allows for nice jumping arcs)
        if (!IsGrounded && !HasMoveInput) acceleration = Vector3.zero;
        // add gravity
        acceleration += GroundNormal * Gravity;

        rb.AddForce(acceleration * rb.mass);

        StepCheck();
        isAlive = Health.IsAlive;
        canBlock = Health.CanBlock;
        isHitReacting = Health.IsHitReacting;
        Health.IsPerfectBlocking = isPerfectBlocking;
    }

    protected virtual void NotFixedUpdate()
    {
        // check for the ground
        IsGrounded = CheckGrounded();

        // overrides current input with pathing direction if MoveTo has been called
        if (NavAgent.hasPath && NavAgent.pathStatus != NavMeshPathStatus.PathInvalid)
        {
            Vector3 nextPathPoint = NavAgent.steeringTarget;
            Vector3 lastPathPoint = NavAgent.path.corners[NavAgent.path.corners.Length - 1];
            float lastPointDistance = Vector3.Distance(lastPathPoint, transform.position);
            bool pathEndReached = lastPointDistance < StoppingDistance;
            Vector3 pathDir = (nextPathPoint - transform.position).normalized;
            // override direction if avoidance is enabled
            if (EnableAvoidance)
            {
                float neighborDistance = NeighborDistance;
                if (NavAgent.path.corners.Length > 2) neighborDistance = CornerNeighborDistance;
                pathDir = GetAvoidanceDirection(nextPathPoint, neighborDistance);

                if (IsClampedToNavMesh)
                {
                    Vector3 pathPoint = transform.position + pathDir * Speed * ClampLookAheadTime;
                    Vector3 clampedPathPoint = ClampToNavMesh(pathPoint, ClampSearchRadius);
                    pathDir = (clampedPathPoint - transform.position).normalized;
                }
            }
            SetMoveInput(pathDir);
            if (LookInMoveDirection) SetLookDirection(pathDir);

            bool destinationReached = Vector3.Distance(NavAgent.destination, transform.position) < StoppingDistance;
            // stop off destination reached
            if (pathEndReached || (StoppingDistance > 0f && destinationReached))
            {
                SetLookPosition(NavAgent.destination);
                Stop();
            }
        }

        // syncs navmeshagent position with character position
        NavAgent.nextPosition = transform.position;
        NavAgent.Warp(transform.position);

        // find flattened movement vector based on ground normal
        Vector3 input = MoveInput;
        Vector3 right = Vector3.Cross(transform.up, input);
        Vector3 forward = Vector3.Cross(right, GroundNormal);

        // move character along spline
        if (EnableSplineConstraint && SplineContainer != null)
        {
            // spline closest point and tangent
            Spline spline = SplineContainer.Spline;
            Vector3 splineRelativePosition = SplineContainer.transform.InverseTransformPoint(transform.position);
            SplineUtility.GetNearestPoint(spline, splineRelativePosition, out float3 nearest, out float t);
            Vector3 splineWorldPosition = SplineContainer.transform.TransformPoint(nearest);
            Vector3 splineTangent = SplineUtility.EvaluateTangent(spline, t);
            splineTangent.y = 0f;
            splineTangent.Normalize();

            // float direction to closest point
            Vector3 dirToSplineCenter = splineWorldPosition - transform.position;
            dirToSplineCenter.y = 0f;
            float splineFlatDistance = dirToSplineCenter.magnitude;
            dirToSplineCenter.Normalize();

            // force bringing character back to spline center
            float gravitationDot = Vector3.Dot(splineTangent, dirToSplineCenter);
            float gravitationCorrection = 1f - Math.Abs(gravitationDot);
            float sideInput = Vector3.Dot(MoveInput, splineTangent);
            rb.AddForce(gravitationCorrection * Mathf.Clamp01(splineFlatDistance) * SplineGravitation * dirToSplineCenter);

            // correct movement direction along spline
            forward = MoveInput.magnitude * sideInput * splineTangent;
            SplineLookDirection = splineTangent * Mathf.Sign(sideInput);
        }

        // vary character speed when using avoidance
        float speed = Speed;
        if (EnableAvoidance)
        {
            float noise = Mathf.PerlinNoise(Time.time, _variationNoiseOffset) * 2f - 1f;
            speed = Speed * (1f + noise * SpeedVariation);
        }

        // calculates desirection movement velocity
        Vector3 targetVelocity = forward * (speed * MoveSpeedMultiplier);
        if (!CanMove) targetVelocity = Vector3.zero;
        // adds velocity of surface under character, if character is stationary
        targetVelocity += SurfaceVelocity * (1f - Mathf.Abs(MoveInput.magnitude));
        // calculates acceleration required to reach desired velocity and applies air control if not grounded
        Vector3 velocityDiff = targetVelocity - Velocity;
        velocityDiff.y = 0f;
        float control = IsGrounded ? 1f : AirControl;
        Vector3 acceleration = velocityDiff * (Acceleration * control);
        // zeros acceleration if airborne and not trying to move (allows for nice jumping arcs)
        if (!IsGrounded && !HasMoveInput) acceleration = Vector3.zero;
        // add gravity
        acceleration += GroundNormal * Gravity;

        rb.AddForce(acceleration * rb.mass);

        StepCheck();
    }

    protected virtual bool CheckGrounded()
    {
        // raycast to find ground
        bool hit = Physics.Raycast(GroundCheckStart, -transform.up, out RaycastHit hitInfo, GroundCheckDistance, GroundMask);

        // set default ground surface normal and SurfaceVelocity
        GroundNormal = Vector3.up;
        SurfaceVelocity = Vector3.zero;

        // if ground wasn't hit, character is not grounded
        if (!hit) return false;

        // gets velocity of surface underneath character if applicable
#if UNITY_6000_0_OR_NEWER
        if (hitInfo.rigidbody != null) SurfaceVelocity = hitInfo.rigidbody.linearVelocity;
#else
            if (hitInfo.rigidbody != null) SurfaceVelocity = hitInfo.rigidbody.velocity;
#endif

        // test angle between character up and ground, angles above _maxSlopeAngle are invalid
        bool angleValid = Vector3.Angle(transform.up, hitInfo.normal) < MaxSlopeAngle;
        if (angleValid)
        {
            // record last time character was grounded and set correct floor normal direction
            LastGroundedTime = Time.timeSinceLevelLoad;
            GroundNormal = hitInfo.normal;
            LastGroundedPosition = transform.position;
            SurfaceObject = hitInfo.collider.gameObject;
            if (ParentToSurface) transform.SetParent(SurfaceObject.transform);
            return true;
        }

        SurfaceObject = null;
        if (ParentToSurface) transform.SetParent(null);
        return false;
    }

    protected void StepCheck()
    {
        if (!IsGrounded) return;

        Vector3 moveInputRight = Vector3.Cross(transform.up, MoveInput.normalized);
        Vector3 groundNormalForward = Vector3.Cross(-GroundNormal, moveInputRight);
        Ray blockingRay = new Ray(transform.position + transform.up * StepHeightAllowance, groundNormalForward);
        float blockingDistance = Radius + StepHeightForwardOffset;
        bool blockingHit = Physics.Raycast(blockingRay, blockingDistance, GroundMask);
        if (!blockingHit) return;

        Vector3 stepHeightPosition = MoveInput.normalized * (StepHeightForwardOffset + Radius) + transform.up * StepHeight + transform.position;
        Ray stepRay = new Ray(stepHeightPosition, -transform.up);
        float distance = StepHeight - StepHeightAllowance;
        bool stepHit = Physics.Raycast(stepRay, out RaycastHit stepHitInfo, distance, GroundMask);
        float groundNormalAngle = Vector3.Angle(GroundNormal, stepHitInfo.normal);
        if (!stepHit) return;

        float stepOffset = stepHitInfo.point.y - transform.position.y;
        float stepVelocity = Mathf.Sqrt(2f * -Gravity * stepOffset);
        Velocity = new Vector3(Velocity.x, stepVelocity, Velocity.z);
    }

    protected Vector3 GetAvoidanceDirection(Vector3 destination, float neighborDistance)
    {
        Vector3 position = transform.position;

        Vector3 separation = Vector3.zero;
        Vector3 alignment = transform.forward;
        Vector3 cohesion = destination;

        int hitCount = Physics.OverlapSphereNonAlloc(position, neighborDistance, _neighborHits, NeighborMask);
        int neighborCount = 0;
        for (int i = 0; i < hitCount; i++)
        {
            GameObject neighbor = _neighborHits[i].gameObject;
            if (neighbor == gameObject) continue;
            neighborCount++;
            separation += GetSeparationVector(neighbor.transform, neighborDistance);
            alignment += neighbor.transform.forward;
            cohesion += neighbor.transform.position;
        }

        float average = 1f / (neighborCount + 1);
        alignment *= average;
        cohesion *= average;
        cohesion = (cohesion - position).normalized;

        Vector3 direction = separation + alignment + cohesion;
        return Vector3.ClampMagnitude(direction, 1f);
    }

    private Vector3 GetSeparationVector(Transform target, float neighborDistance)
    {
        Vector3 diff = transform.position - target.transform.position;
        float diffLen = diff.magnitude;
        float scaler = Mathf.Clamp01(1.0f - diffLen / neighborDistance);
        return diff * (scaler / diffLen);
    }

    protected Vector3 ClampToNavMesh(Vector3 position, float searchRadius)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return position;
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        float landingCollisionMaxDistance = 0.25f;
        Vector3 point = collision.contacts[0].point;
        if (Mathf.Abs(collision.relativeVelocity.y) < MinGroundedVelocity) return;
        if (Vector3.Distance(point, transform.position) < landingCollisionMaxDistance)
        {
            OnGrounded.Invoke(collision.gameObject);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(GroundCheckStart, -transform.up * GroundCheckDistance);

        if (EnableAvoidance)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, NeighborDistance);
        }

        // step height debug
        Gizmos.color = Color.cyan;
        Vector3 stepHeightPosition = MoveInput.normalized * (StepHeightForwardOffset + Radius) + transform.up * StepHeight + transform.position;
        Ray stepRay = new Ray(stepHeightPosition, -transform.up);
        float distance = StepHeight - StepHeightAllowance;
        Gizmos.DrawRay(stepRay.origin, stepRay.direction * distance);
    }

    private void FindWeapons()
    {
        Weapons = GetComponentsInChildren<Weapon>();
    }

    private void Knockback(DamageInfo damageInfo)
    {
        StartCoroutine(KnockbackRoutine(damageInfo));
    }

    private IEnumerator KnockbackRoutine(DamageInfo damageInfo)
    {
        CustomCharacterMovement movement = damageInfo.Victim.GetComponent<CustomCharacterMovement>();
        Rigidbody rb = movement.Rigidbody;
        NavMeshAgent agent = movement.NavMeshAgent;
        Animator.applyRootMotion = false;
        agent.enabled = false;
        rb.linearVelocity = Vector3.zero;
        yield return new WaitForEndOfFrame();
    
        CustomController instigatorController = damageInfo.Instigator.GetComponent<CustomController>();
        WeaponMeleeData data = Weapons[CurrentWeaponIndex].Data as WeaponMeleeData;
        if (data != null)
        {
            Vector3 knockbackDirection = (data.ComboData[CurrentActionIndex].KnockbackDirection).normalized;
            rb.AddForce(damageInfo.KnockBackForce * (knockbackDirection + damageInfo.Instigator.transform.forward), ForceMode.Impulse);
            AnimatorClipInfo[] currentClipInfo = Animator.GetCurrentAnimatorClipInfo(0);
           
        }
        else rb.AddForce(damageInfo.KnockBackForce * -damageInfo.Victim.transform.forward, ForceMode.Impulse);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length / 2);
        //Animator.applyRootMotion = true;
        rb.linearVelocity = Vector3.zero;
        agent.enabled = true;
        yield return new WaitForEndOfFrame();
        if (agent.isOnNavMesh) agent.ResetPath();
        yield return null;
    }

    private void Death(DamageInfo arg0)
    {
        isAlive = false;
        Stop();
        CanMove = false;
        enabled = false;
    }

    private void BlockedAttack(DamageInfo damageInfo)
    {
        StartCoroutine(BlockedAttackRoutine(damageInfo));
    }

    private IEnumerator BlockedAttackRoutine(DamageInfo damageInfo)
    {
        CustomController controller = damageInfo.Instigator.GetComponent<CustomController>();
        WeaponMeleeData data = Weapons[CurrentWeaponIndex].Data as WeaponMeleeData;
        //Rigidbody rb = Movement.Rigidbody;
        //NavMeshAgent agent = Movement.NavMeshAgent;
        Animator.applyRootMotion = false;
        NavAgent.enabled = false;
        rb.linearVelocity = Vector3.zero;
        isBlockedAttack = true;
    
        yield return new WaitForEndOfFrame();
    
        if (isPerfectBlocking && data != null)
        {
            Vector3 knockbackDirection = (data.ComboData[CurrentActionIndex].KnockbackDirection).normalized;
            rb.AddForce(damageInfo.KnockBackForce * (knockbackDirection + damageInfo.Instigator.transform.forward), ForceMode.Impulse);
            Debug.DrawLine(transform.position, damageInfo.Instigator.transform.position, Color.red);
        }
        else rb.AddForce(damageInfo.KnockBackForce * -damageInfo.Victim.transform.forward, ForceMode.Impulse);
    
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
    
        isBlockedAttack = false;
        rb.linearVelocity = Vector3.zero;
        NavAgent.enabled = true;
        yield return new WaitForEndOfFrame();
        if (NavAgent.isOnNavMesh) NavAgent.ResetPath();
        yield return null;
    }

    private IEnumerator PerfectBlockRoutine()
    {
        isPerfectBlocking = true;
    
        yield return new WaitForSeconds(Weapons[CurrentWeaponIndex].Data.PerfectBlockTime);
    
        isPerfectBlocking = false;
    }

    

    #region AnimEvents
    public void MeleeHitAnimEvent(int attackIndex)
    {
        WeaponMelee meleeweapon = Weapons[weaponIndex] as WeaponMelee;
        if (meleeweapon == null) return;
        meleeweapon.MeleeHitAnimEvent(attackIndex);
    }
    public void PerfectBlock()
    {
        //StartCoroutine(PerfectBlockRoutine());
    }
    #endregion
}
