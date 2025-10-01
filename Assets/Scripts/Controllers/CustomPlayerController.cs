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
        _stateMachine.IsAttacking = value.isPressed;
    }

    public virtual void OnBlock(InputValue value)
    {
        _stateMachine.isBlocking = _stateMachine.CanBlock && value.isPressed;
    }
    protected virtual void Update()
    {
        _currentStateName = _stateMachine.CurrentState.ToString();
        CurrentSpeed = Mathf.Ceil(_stateMachine.rb.linearVelocity.magnitude);
        //PlayerSpeed.Invoke(CurrentSpeed.ToString());
    }
   
}
