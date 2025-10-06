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
    [field: SerializeField, TabGroup("Events")] public Action<bool> BlockAction;
    [field: SerializeField, TabGroup("Events")] public Action<bool> AttackAction;
    
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
        JumpAction?.Invoke();
    }

    public virtual void OnDash(InputValue value)
    {
        DodgeAction?.Invoke();
    }
    public virtual void OnAttack(InputValue value)
    {
        AttackAction?.Invoke(value.isPressed);
    }

    public virtual void OnBlock(InputValue value)
    {
        BlockAction?.Invoke(value.isPressed);
    }
    protected virtual void Update()
    {
        _currentStateName = _stateMachine.CurrentState.ToString();
        CurrentSpeed = Mathf.Ceil(_stateMachine.rb.linearVelocity.magnitude);
        //PlayerSpeed.Invoke(CurrentSpeed.ToString());
    }
   
}
