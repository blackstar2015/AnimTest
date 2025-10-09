using Sirenix.OdinInspector;
using System;
using GameEvents;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CustomPlayerController : CustomController
{
    private PlayerStateMachine stateMachine;
    [field: SerializeField, TabGroup("Properties")] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
     [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private float _lastDashTime = Mathf.NegativeInfinity;
     [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private float _lastAttackTime = Mathf.NegativeInfinity;
     [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] public Vector2 MoveInput { get; set; }

    [field: SerializeField, TabGroup("Events")] public Action JumpAction;
    [field: SerializeField, TabGroup("Events")] public Action DodgeAction;
    [field: SerializeField, TabGroup("Events")] public Action<bool> BlockAction;
    [field: SerializeField, TabGroup("Events")] public Action<bool> AttackAction;
    [field: SerializeField, TabGroup("Events")] public Action<bool> SprintAction;
    
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
        this.stateMachine = stateMachine;
    }
    public void OnWeaponSwitch()
    {
        if (stateMachine.CurrentWeaponIndex >= stateMachine.Weapons.Length - 1)
        {
            stateMachine.weaponIndex = 0;
        }
        else
        {
            stateMachine.weaponIndex++;
        }
        stateMachine.Animator.SetInteger("WeaponIndex", stateMachine.weaponIndex);
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
        if (!stateMachine.CanMove) return;
        float nextDashTime = stateMachine.LastDashTime + stateMachine.DashCooldown;

        if (Time.time > nextDashTime)
        {
            float DashAnimLength = stateMachine.Animator.GetCurrentAnimatorClipInfo(0).Length;
            DodgeAction?.Invoke();
            stateMachine.LastDashTime = Time.time;
        }
    }
    public virtual void OnAttack(InputValue value)
    {
        Weapon equippedWeapon = stateMachine.Weapons[stateMachine.weaponIndex];
        float nextAttackTime = stateMachine.LastAttackTime + 2;

        if (Time.time > nextAttackTime)
        {
            AttackAction?.Invoke(value.isPressed);
            WeaponMelee melee = equippedWeapon as WeaponMelee;
            if (melee == null) return;
            stateMachine.actionIndex++;
            if (stateMachine.actionIndex > melee?.MeleeData.ComboData.Length - 1) stateMachine.actionIndex = 1;
            stateMachine.LastAttackTime = Time.time;
        }
    }

    public virtual void OnBlock(InputValue value)
    {
        BlockAction?.Invoke(value.isPressed);
    }

    public virtual void OnSprint(InputValue value)
    {
        SprintAction?.Invoke(value.isPressed);
    }
    protected virtual void Update()
    {
        _currentStateName = stateMachine.CurrentState.ToString();
        CurrentSpeed = Mathf.Ceil(stateMachine.rb.linearVelocity.magnitude);
        //PlayerSpeed.Invoke(CurrentSpeed.ToString());
    }
   
}
