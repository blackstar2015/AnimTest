using CharacterMovement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomPlayerController : CustomController
{
    // initial cursor state
    [field: SerializeField] protected CursorLockMode CursorMode { get; set; } = CursorLockMode.Locked;
    // make character look in Camera direction instead of MoveDirection
    [field: SerializeField] public bool LookInCameraDirection { get; set; }

    public bool CanShoot { get; set; } = true;
    public bool CanMelee { get; set; } = true;
    public int CurrentActionIndex => _actionIndex;
    public int CurrentWeaponIndex => _weaponIndex;

    private float _lastDashTime = Mathf.NegativeInfinity;
    private float _lastAttackTime = Mathf.NegativeInfinity;
    private int _actionIndex = 1;
    private int _weaponIndex = 0;
    private bool _isAttacking;
    private bool _isBlocking = false;
    // array of current weapons
    [field: SerializeField, InlineButton(nameof(FindWeapons), "Find")] public Weapons[] Weapons { get; private set; }
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
    private void Start()
    {
        //ActivateCurrentWeapon();        
    }
    private void ActivateCurrentWeapon()
    {
        foreach (Weapons weapon in Weapons)
        {
            weapon.AssignWeaponMesh();
            // if (weapon.Data.WeaponIndex != _weaponIndex)
            // {
            //     weapon.Data.WeaponMesh.gameObject.SetActive(false);
            // }
            // else weapon.Data.WeaponMesh.gameObject.SetActive(true);

        }
    }

    private void FindWeapons()
    {
        Weapons = GetComponentsInChildren<Weapons>();
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
        _isBlocking = value.isPressed;
    }
    protected virtual void Update()
    {
         Animator.SetBool("IsBlocking", _isBlocking);
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
    
    #region AnimationEvents
    public void Sheath(int index)
    {
        Debug.Log("ASD");
        GameObject weaponMesh = Weapons[index].Data.WeaponMesh;
        weaponMesh.SetActive(false);
    }

    public void UnSheath(int index)
    {
        Debug.Log("ASDA");
        GameObject weaponMesh = Weapons[index].Data.WeaponMesh;
        weaponMesh.SetActive(true);
    }

    public void DisableTrigger(int index)
    {
        foreach (Collider collider in Weapons[index].Data.WeaponColliders)
        {
            collider.enabled = false;
        }
    }

    public void EnableTrigger(int index)
    {
        foreach (Collider collider in Weapons[index].Data.WeaponColliders)
        {
            collider.enabled = true;
        }
    }
    #endregion
}
