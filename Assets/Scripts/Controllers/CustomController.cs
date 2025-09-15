using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;

public class CustomController : MonoBehaviour
{
    [field: SerializeField] protected CustomCharacterMovement Movement { get; set; }
    [field: SerializeField] protected Animator Animator { get; set; }
    public Health _health { get; private set; }
    public Targetable Targetable { get; private set; }
    public Vision Vision { get; private set; }
    [field: SerializeField, InlineButton(nameof(FindWeapons), "Find")] public Weapons[] Weapons { get; private set; }
    public int CurrentWeaponIndex => _weaponIndex;
    public int CurrentActionIndex => _actionIndex;
    protected int _actionIndex = 1;
    protected int _weaponIndex = 0;
    public bool CanShoot { get; set; } = true;
    public bool CanMelee { get; set; } = true;
    [field: SerializeField] public bool LookInCameraDirection { get; set; }
    public bool IsBlocking { get; internal set; }
    public bool IsAlive => _health.IsAlive;
    public bool _canBlock => _health.CanBlock;
    public bool CanBlock => _canBlock;

    protected virtual void OnValidate()
    {
        if(Movement == null) Movement = GetComponent<CustomCharacterMovement>();
        if(Animator == null) Animator = GetComponent<Animator>();
    }
    
    protected virtual void Awake()
    {
        //Cursor.lockState = CursorMode;
        Movement = GetComponent<CustomCharacterMovement>();
        _health = GetComponent<Health>();
        Targetable = GetComponent<Targetable>();
        Vision = GetComponent<Vision>();
        _health.OnBlockedAttack.AddListener(BlockedAttack);
        foreach(Weapons weapon in Weapons)
        {
            DamageInfo damageInfo = new DamageInfo(0, DamageType.Physical, false, gameObject, gameObject, gameObject);
            _health.OnDeath.AddListener(weapon.DisableWeaponColliders);
        }
        _health.OnDeath.AddListener(Death);
    }

    private void Death(DamageInfo arg0)
    {
        this.enabled = false;
    }

    private void BlockedAttack()
    {
        StartCoroutine(BlockedAttackRoutine());
    }

    private IEnumerator BlockedAttackRoutine()
    {
        Animator.SetBool("BlockedAttack", true);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length/2);
        Animator.SetBool("BlockedAttack", false);
    }

    private void FindWeapons()
    {
        Weapons = GetComponentsInChildren<Weapons>();
    }

    protected virtual void Update()
    {
        if(!_health.IsAlive)
        {
            Movement.Stop();
            return;
        }
        _health.IsBlocking = IsBlocking;
    }
    
}
