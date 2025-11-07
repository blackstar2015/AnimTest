using GameEvents;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(PlayerInput))]
public class CustomPlayerController : CustomController
{
    private PlayerStateMachine stateMachine;
    protected CursorLockMode CursorMode => stateMachine.CursorMode;
    [field: SerializeField, HideInEditorMode, ReadOnly] public Vector2 MoveInput { get; set; }

    [field: SerializeField, TabGroup("Events")] public Action JumpAction;
    [field: SerializeField, TabGroup("Events")] public Action DodgeAction;
    [field: SerializeField, TabGroup("Events")] public Action<bool> BlockAction;
    [field: SerializeField, TabGroup("Events")] public Action<bool> AttackAction;
    [field: SerializeField, TabGroup("Events")] public Action<bool> SprintAction;
    [field: SerializeField, TabGroup("Events")] public Action WeaponSwitchAction;
    [field: SerializeField, TabGroup("Events")] public Action TargetLockAction;

    [ReadOnly, HideInEditorMode, SerializeField] private string _currentStateName { get; set; }
    [ReadOnly, HideInEditorMode, SerializeField] public static float CurrentSpeed;

    public CinemachineCamera TargetLockCam => stateMachine.TargetLockCam; 
    public CinemachineCamera FreeLookCam => stateMachine.FreeLookCam;
    public CinemachineTargetGroup TargetGroup => stateMachine.TargetGroup;

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

        TargetGroup.Targets.Capacity = 2;
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
        if(!stateMachine.CanMove || stateMachine.IsDashing) MoveInput = Vector2.zero;
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
    public void OnTargetLockScroll(InputValue value)
    {
        float lastScrollTime = stateMachine.LastScrollTime + .5f;
        if (Time.time > lastScrollTime)
        {
            if(value.Get<Vector2>().y >= .1f)
            {
                //stateMachine.IncrementVisibleTarget();
            }
            else if(value.Get<Vector2>().y < -.1f)
            {
                //stateMachine.DecrementVisibleTarget();
            }
            stateMachine.LastScrollTime = Time.time;
            //TargetLockAction?.Invoke();
        }
    }
    public void OnTargetLock()
    {
        TargetLockAction?.Invoke();
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
