using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomPlayerController : CustomController
{
    [field: SerializeField, TabGroup("Properties")] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
     [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private float _lastDashTime = Mathf.NegativeInfinity;
     [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private float _lastAttackTime = Mathf.NegativeInfinity;
     [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] private bool _isAttacking = false;
     [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] protected Vector2 MoveInput { get; set; }

    [field: SerializeField, TabGroup("Events")] public Action JumpAction;
    [field: SerializeField, TabGroup("Events")] public Action DodgeAction;
    [field: SerializeField, TabGroup("Events")] public Action BlockAction;
    [field: SerializeField, TabGroup("Events")] public Action AttackAction;
    protected override void Awake()
    {
        base.Awake();
        Cursor.lockState = CursorMode;
    }

    public void OnWeaponSwitch()
    {
        if (weaponIndex >= Weapons.Length - 1)
        {
            weaponIndex = 0;
        }
        else
        {
            weaponIndex++;
        }
        Animator.SetInteger("WeaponIndex", weaponIndex);
    }
    public virtual void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    public virtual void OnJump(InputValue value)
    {
        Movement?.TryJump();
    }

    public virtual void OnDash(InputValue value)
    {
        if(!Movement.CanMove) return;
        float nextDashTime = _lastDashTime + Movement.DashCooldown;
        if (Time.time > nextDashTime)
        {
            Movement?.Dash(Animator.GetCurrentAnimatorStateInfo(0).length);
            Animator?.SetTrigger("Dash");
            _lastDashTime = Time.time;
        }
    }
    public virtual void OnAttack(InputValue value)
    {
        _isAttacking = value.isPressed;
    }

    public virtual void OnBlock(InputValue value)
    {
        isBlocking = CanBlock && value.isPressed;
    }
    protected virtual void Update()
    {
        base.Update();
        if (Movement == null) return;
        // find correct right/forward directions based on main camera rotation
        Vector3 up = Vector3.up;
        Vector3 right = Camera.main.transform.right;
        Vector3 forward = Vector3.Cross(right, up);
        Vector3 moveInput = forward * MoveInput.y + right * MoveInput.x;

        // send player input to character movement
        Movement.SetMoveInput(moveInput);
        Movement.SetLookDirection(moveInput);
        HandleAttack();
        LookInCameraDirection = !Movement.IsDashing;
        if (LookInCameraDirection) Movement.SetLookDirection(Camera.main.transform.forward);
    }
    private void HandleAttack()
    {
        if (!_isAttacking) return;
        Weapon equippedWeapon = Weapons[weaponIndex];
        float nextAttackTime = _lastAttackTime + 1/equippedWeapon.Data.AttackRate;
        
        if (Time.time < nextAttackTime) return;
        
        equippedWeapon.TryAttack(transform.position + transform.forward * 5,gameObject,Targetable.Team);
        Animator.SetTrigger(equippedWeapon.Data.AttackAnimName);
        Animator.SetInteger("Action", actionIndex);
        WeaponMelee melee = equippedWeapon as WeaponMelee;
        if (melee == null)  return;
        actionIndex++;
        if (actionIndex > melee?.MeleeData.ComboData.Length-1) actionIndex = 0;
        _lastAttackTime =  Time.time;
    }
}
