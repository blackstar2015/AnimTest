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
    public void Dodge(float DashAnimLength, Vector3 dodgeDirection, int DodgeHash)
    {
        StartCoroutine(DodgeCoroutine(DashAnimLength, dodgeDirection, DodgeHash));
    }

    public IEnumerator DodgeCoroutine(float DashAnimLength, Vector3 dodgeDirection, int DodgeHash)
    {
        if (!CanMove) yield break;
        IsDashing = true;
        Animator.applyRootMotion = true;
        LookInCameraDirection = false;

        SetLookDirection(dodgeDirection);
        Animator.CrossFadeInFixedTime(DodgeHash, 0.1f);
        yield return new WaitForEndOfFrame();
        rb.AddForce(dodgeDirection * DashSpeed, ForceMode.Impulse);
        //yield return new WaitForSeconds(DashAnimLength);
        
        Animator.applyRootMotion = false;
        LookInCameraDirection = true;
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


    public void SwitchToMovement()
    {
        if (rb.linearVelocity.magnitude <= .1f)
        {
            SwitchState(new PlayerIdleState(this));
        }
        else
        {
            SwitchState(new PlayerWalkingState(this));
        }
    }
}
