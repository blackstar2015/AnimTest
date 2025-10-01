using CharacterMovement;
using GameEvents;
using Sirenix.OdinInspector;
using System.Collections;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

[RequireComponent(typeof(CustomPlayerController))]
public class PlayerStateMachine : StateMachine
{
    [field: SerializeField, TabGroup("Properties")] public float PlayerMaxWalkSpeed = 5f;

    [field: SerializeField, TabGroup("Properties")] public CustomPlayerController PlayerController => Controller as CustomPlayerController;
    [field: SerializeField, TabGroup("Properties")] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private float _lastDashTime = Mathf.NegativeInfinity;
    [field: SerializeField, TabGroup("Properties")] public bool debugStateTransitions = false;

    [ShowInInspector, TabGroup("Movement","Dashing")] public float DashSpeed = 1000f;
    [ShowInInspector, TabGroup("Movement","Dashing")] public float DashCooldown { get;  set; } = 2f;

    [ShowInInspector, TabGroup("Movement","Dashing")] private Vector3 _dashDirection;
    


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
    public void Dodge()
    {
        if (!CanMove) return;
        float nextDashTime = _lastDashTime + DashCooldown;
        if (Time.time > nextDashTime)
        {
            float DashAnimLength = Animator.GetCurrentAnimatorClipInfo(0).Length;
            StartCoroutine(DodgeCoroutine(DashAnimLength));
            _lastDashTime = Time.time;
        }
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

    public override void TryJump()
    {
        if(JumpCounter < MaxJumps)
        {
            Jump();
            JumpCounter++;
        }
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
