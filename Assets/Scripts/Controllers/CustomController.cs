using Sirenix.OdinInspector;
using System;
using System.Collections;
using RPGCharacterAnims.Actions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class CustomController : MonoBehaviour
{
    [field: SerializeField, TabGroup("Components")] protected CustomCharacterMovement Movement { get; set; }
    [field: SerializeField, TabGroup("Components")] protected Animator Animator { get; set; }
    [field: SerializeField, TabGroup("Components")]public Health Health { get; set; }
    [field: SerializeField, TabGroup("Components")]public Targetable Targetable { get; set; }
    [field: SerializeField, TabGroup("Components")]public Vision Vision { get; set; }
    [field: SerializeField, InlineButton(nameof(FindWeapons), "Find"), TabGroup("Weapons")] public Weapons[] Weapons { get; private set; }
    [field: SerializeField, TabGroup("Properties")] public bool LookInCameraDirection { get; set; }
    [field: SerializeField, TabGroup("Properties")] protected int actionIndex = 1;
    [field: SerializeField, TabGroup("Properties")] protected int weaponIndex = 0;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode]public bool CanShoot { get; set; } = true;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode]public bool CanMelee { get; set; } = true;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode]public bool IsBlocking => isBlocking;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode]public bool IsAlive => isAlive;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode]public bool CanBlock => canBlock;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode]public bool IsHitReacting => isHitReacting;
    protected bool isBlocking { get; set; }
    protected bool canBlock  { get; set; }
    protected bool isHitReacting { get; set; }
    protected bool isAlive { get; set; }
    
    public int CurrentWeaponIndex => weaponIndex;
    public int CurrentActionIndex => actionIndex;

    protected virtual void OnValidate()
    {
        if(Movement == null) Movement = GetComponent<CustomCharacterMovement>();
        if(Animator == null) Animator = GetComponent<Animator>();
    }
    
    protected virtual void Awake()
    {
        //Cursor.lockState = CursorMode;
        Movement = GetComponent<CustomCharacterMovement>();
        Health = GetComponent<Health>();
        Targetable = GetComponent<Targetable>();
        Vision = GetComponent<Vision>();
        isAlive = Health.IsAlive;
        canBlock  = Health.CanBlock;
        isHitReacting = Health.IsHitReacting;
        Health.OnBlockedAttack.AddListener(BlockedAttack);
        foreach(Weapons weapon in Weapons)
        {
            Health.OnDeath.AddListener(weapon.DisableWeaponColliders);
        }
        Health.OnDeath.AddListener(Death);
        Health.OnDamage.AddListener(Knockback);
    }

    private void Knockback(DamageInfo damageInfo)
    {
        StartCoroutine(KnockbackRoutine(damageInfo));
    }

    private IEnumerator KnockbackRoutine(DamageInfo damageInfo)
    {
        CustomCharacterMovement movement = damageInfo.Victim.GetComponent<CustomCharacterMovement>();
        Rigidbody rb = movement.Rigidbody;
        NavMeshAgent agent = movement.NavMeshAgent;
        Animator.applyRootMotion = false;
        agent.enabled = false;
        rb.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(1);
        rb.AddForce(damageInfo.KnockBackForce * -1  * damageInfo.Victim.transform.forward, ForceMode.Impulse);
        yield return new WaitForEndOfFrame();
        Animator.applyRootMotion = true;
        agent.enabled = true;
        agent.ResetPath();
        yield return null;
    }
    private void Death(DamageInfo arg0)
    {
        isAlive = false;
        Movement.Stop();
        Movement.CanMove = false;
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
        isAlive = Health.IsAlive;
        canBlock  = Health.CanBlock;
        isHitReacting = Health.IsHitReacting;
    }
}
