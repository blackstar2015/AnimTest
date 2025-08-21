using CharacterMovement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomPlayerController : MonoBehaviour
{
    // initial cursor state
    [field: SerializeField] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
    // make character look in Camera direction instead of MoveDirection
    [field: SerializeField] public bool LookInCameraDirection { get; set; }

    [field: Header("Components")]
    [field: SerializeField] protected CustomCharacterMovement Movement { get; set; }
    [field: SerializeField] protected Animator Animator { get; set; }

    [field: SerializeField] protected int ActionList = 6;

    public Health Health { get; private set; }
    public Targetable Targetable { get; private set; }
    public Vision Vision { get; private set; }

    public bool CanShoot { get; set; } = true;
    public bool CanMelee { get; set; } = true;

    private float _lastDashTime = Mathf.NegativeInfinity;
    private float _lastAttackTime = Mathf.NegativeInfinity;
    private int _actionIndex = 1;
    private int WeaponIndex = 0;
    
    private bool _isAttacking;

    // array of current weapons
    // InlineButton appears beside the property/field in the inspector
    [field: SerializeField, InlineButton(nameof(FindWeapons), "Find")] public Weapons[] Weapons { get; private set; }
    protected Vector2 MoveInput { get; set; }

    protected virtual void OnValidate()
    {
        if(Movement == null) Movement = GetComponent<CustomCharacterMovement>();
        if(Animator == null) Animator = GetComponent<Animator>();
    }

    protected virtual void Awake()
    {
        Cursor.lockState = CursorMode;
        Movement = GetComponent<CustomCharacterMovement>();
        Health = GetComponent<Health>();
        Targetable = GetComponent<Targetable>();
        Vision = GetComponent<Vision>();
    }

    private void FindWeapons()
    {
        Weapons = GetComponentsInChildren<Weapons>();
    }

    private void OnWeaponSwitch()
    {
        if (WeaponIndex >= Weapons.Length) WeaponIndex = 0;
        else WeaponIndex++;
        
        Animator.SetInteger("WeaponIndex", WeaponIndex);
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

    protected virtual void Update()
    {
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
        Weapons equippedWeapon = Weapons[WeaponIndex];
        float nextAttackTime = _lastAttackTime + 1/equippedWeapon.Data.AttackRate;
        
        if (Time.time < nextAttackTime) return;
        
        //equippedWeapon.TryAttack();
        Animator.SetTrigger(equippedWeapon.Data.AttackAnimName);
        Animator.SetInteger("Action", _actionIndex);
        _actionIndex++;
        WeaponsMelee melee = equippedWeapon as WeaponsMelee;
        if (_actionIndex > melee?.MeleeData.ComboData.Length) _actionIndex = 1;
        _lastAttackTime =  Time.time;
    }
}
