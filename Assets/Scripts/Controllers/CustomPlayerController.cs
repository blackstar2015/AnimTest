using Sirenix.OdinInspector;
using System;
using GameEvents;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CustomPlayerController : CustomController
{
    private PlayerStateMachine _stateMachine;
    [field: SerializeField, TabGroup("Properties")] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
     [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private float _lastDashTime = Mathf.NegativeInfinity;
     [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private float _lastAttackTime = Mathf.NegativeInfinity;
     [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private bool _isAttacking = false;
     [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] public Vector2 MoveInput { get; set; }

    [field: SerializeField, TabGroup("Events")] public Action JumpAction;
    [field: SerializeField, TabGroup("Events")] public Action DodgeAction;
    [field: SerializeField, TabGroup("Events")] public Action BlockAction;
    [field: SerializeField, TabGroup("Events")] public Action AttackAction;
    
    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] private string _currentStateName { get; set; }
    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public static float CurrentSpeed;
    [SerializeField] public StringEventAsset PlayerSpeed;
    public override void Awake()
    {
        base.Awake();
        Cursor.lockState = CursorMode;
    }

    public void SetStateMachine(PlayerStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
    public void OnWeaponSwitch()
    {
        if (_stateMachine.CurrentWeaponIndex >= _stateMachine.Weapons.Length - 1)
        {
            _stateMachine.weaponIndex = 0;
        }
        else
        {
            _stateMachine.weaponIndex++;
        }
        _stateMachine.Animator.SetInteger("WeaponIndex", _stateMachine.weaponIndex);
    }
    public virtual void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public virtual void OnJump(InputValue value)
    {
        _stateMachine?.TryJump();
    }

    public virtual void OnDash(InputValue value)
    {
        if(!_stateMachine.CanMove) return;
        float nextDashTime = _lastDashTime + _stateMachine.DashCooldown;
        if (Time.time > nextDashTime)
        {
            _stateMachine?.Dodge(_stateMachine.Animator.GetCurrentAnimatorStateInfo(0).length);
            _stateMachine.Animator?.SetTrigger("Dash");
            _lastDashTime = Time.time;
        }
    }
    public virtual void OnAttack(InputValue value)
    {
        _isAttacking = value.isPressed;
    }

    public virtual void OnBlock(InputValue value)
    {
        _stateMachine.isBlocking = _stateMachine.CanBlock && value.isPressed;
    }
    protected virtual void Update()
    {
        // base.Update();
        // if (Movement == null) return;
        // // find correct right/forward directions based on main camera rotation
        // Vector3 up = Vector3.up;
        // Vector3 right = Camera.main.transform.right;
        // Vector3 forward = Vector3.Cross(right, up);
        // Vector3 moveInput = forward * MoveInput.y + right * MoveInput.x;
        //
        // // send player input to character movement
        // Movement.SetMoveInput(moveInput);
        // Movement.SetLookDirection(moveInput);
        // HandleAttack();
        // LookInCameraDirection = !Movement.IsDashing;
        // if (LookInCameraDirection) Movement.SetLookDirection(Camera.main.transform.forward);
        _currentStateName = _stateMachine.CurrentState.ToString();
        CurrentSpeed = _stateMachine.rb.linearVelocity.magnitude;
        Mathf.Ceil(CurrentSpeed);
        PlayerSpeed.Invoke(CurrentSpeed.ToString());
    }
    private void HandleAttack()
    {
        if (!_isAttacking) return;
        Weapon equippedWeapon = _stateMachine.Weapons[_stateMachine.weaponIndex];
        float nextAttackTime = _lastAttackTime + 1/equippedWeapon.Data.AttackRate;
        
        if (Time.time < nextAttackTime) return;
        
        equippedWeapon.TryAttack(transform.position + transform.forward * 5,gameObject,_stateMachine.Targetable.Team);
        _stateMachine.Animator.SetTrigger(equippedWeapon.Data.AttackAnimName);
        _stateMachine.Animator.SetInteger("Action", _stateMachine.actionIndex);
        WeaponMelee melee = equippedWeapon as WeaponMelee;
        if (melee == null)  return;
        _stateMachine.actionIndex++;
        if (_stateMachine.actionIndex > melee?.MeleeData.ComboData.Length-1) _stateMachine.actionIndex = 0;
        _lastAttackTime =  Time.time;
    }
}
