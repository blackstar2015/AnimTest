using CharacterMovement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomPlayerController : CustomController
{
    [field: SerializeField] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
    private float _lastDashTime = Mathf.NegativeInfinity;
    private float _lastAttackTime = Mathf.NegativeInfinity;
    private bool _isAttacking;

    protected Vector2 MoveInput { get; set; }

    protected override void OnValidate()
    {
        base.OnValidate();
        
    }

    protected override void Awake()
    {
        base.Awake();
        Cursor.lockState = CursorMode;
    }

    public void OnWeaponSwitch()
    {
        if (_weaponIndex >= Weapons.Length - 1)
        {
            _weaponIndex = 0;
        }
        else
        {
            _weaponIndex++;
        }
        Animator.SetInteger("WeaponIndex", _weaponIndex);
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
        IsBlocking = _canBlock ? value.isPressed : false;
    }
    protected virtual void Update()
    {
        base.Update();
         Animator.SetBool("IsBlocking", IsBlocking);
         Animator.SetBool("CanBlock", _canBlock);
        if (Movement == null) return;
        // find correct right/forward directions based on main camera rotation
        Vector3 up = Vector3.up;
        Vector3 right = Camera.main.transform.right;
        Vector3 forward = Vector3.Cross(right, up);
        Vector3 moveInput = forward * MoveInput.y + right * MoveInput.x;

        // send player input to character movement
        Movement.SetMoveInput(moveInput);
        Movement.SetLookDirection(moveInput);
        LookInCameraDirection = !Movement.IsDashing;
        if (LookInCameraDirection) Movement.SetLookDirection(Camera.main.transform.forward);
        if (_isAttacking) HandleAttack();
    }
    private void HandleAttack()
    {
        Weapons equippedWeapon = Weapons[_weaponIndex];
        float nextAttackTime = _lastAttackTime + 1/equippedWeapon.Data.AttackRate;
        
        if (Time.time < nextAttackTime) return;
        
        //equippedWeapon.TryAttack();
        Animator.SetTrigger(equippedWeapon.Data.AttackAnimName);
        Animator.SetInteger("Action", _actionIndex);
        _actionIndex++;
        WeaponsMelee melee = equippedWeapon as WeaponsMelee;
        if (melee == null)  return;
        if (_actionIndex > melee?.MeleeData.ComboData.Length) _actionIndex = 1;
        _lastAttackTime =  Time.time;
    }
}
