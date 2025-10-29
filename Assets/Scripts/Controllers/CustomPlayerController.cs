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
    [field: SerializeField, TabGroup("Events")] public Action WeaponSwitchAction;
    [field: SerializeField, TabGroup("Events")] public Action TargetLockAction;

    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] private string _currentStateName { get; set; }
    [field: FoldoutGroup("Properties"), ReadOnly, HideInEditorMode, SerializeField] public static float CurrentSpeed;
    [SerializeField] public StringEventAsset PlayerSpeed;
    public override void Awake()
    {
        base.Awake();
        Cursor.lockState = CursorMode;
        foreach (Weapon weapon in stateMachine.Weapons)
        {
            if (weapon.Data.WeaponIndex == stateMachine.weaponIndex)
            {
                weapon.WeaponMesh.SetActive(true);
            }
            else
            {
                weapon.WeaponMesh.SetActive(false);
            }
        }
    }

    public void SetStateMachine(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    public void OnWeaponSwitch()
    {
        WeaponSwitchAction?.Invoke();
    }
    public virtual void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
        if(!stateMachine.CanMove) MoveInput = Vector2.zero;
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
            DodgeAction?.Invoke();
            stateMachine.LastDashTime = Time.time;
        }
    }
    public virtual void OnAttack(InputValue value)
    {
        AttackAction?.Invoke(value.isPressed);
    }
    public void OnTargetLock(InputValue value)
    {
        TargetLockAction?.Invoke();
        
        if(value.Get<Vector2>().x > 0)
        {
            stateMachine.IncrementVisibleTarget();
        }
        else if(value.Get<Vector2>().x < 0)
        {
            stateMachine.DecrementVisibleTarget();
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
    public override void Update()
    {
        base.Update();  
        _currentStateName = stateMachine.CurrentState.ToString();
        CurrentSpeed = Mathf.Ceil(stateMachine.rb.linearVelocity.magnitude);
        //PlayerSpeed.Invoke(CurrentSpeed.ToString());
    }
   
}
