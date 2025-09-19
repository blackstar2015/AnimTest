using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode]public bool IsBlockedAttack => isBlockedAttack;
    protected bool isBlocking { get; set; }
    protected bool canBlock  { get; set; }
    protected bool isHitReacting { get; set; }
    protected bool isAlive { get; set; }
    protected bool isBlockedAttack { get; set; }
    
    public int CurrentWeaponIndex => weaponIndex;
    public int CurrentActionIndex => actionIndex;

    protected virtual void OnValidate()
    {
        if(Movement == null) Movement = GetComponent<CustomCharacterMovement>();
        if(Animator == null) Animator = GetComponent<Animator>();
        if(Targetable == null) Targetable = GetComponent<Targetable>();
        if(Vision == null) Vision = GetComponent<Vision>();
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
        Health.OnDeath.AddListener(Death);
        Health.OnDamage.AddListener(Knockback);
        // foreach(Weapons weapon in Weapons)
        // {
        //     Health.OnDeath.AddListener(weapon.DisableWeaponColliders);
        // }
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
        yield return new WaitForEndOfFrame();
        
        WeaponMeleeData data = damageInfo.Victim.GetComponent<CustomController>().Weapons[CurrentWeaponIndex].Data as WeaponMeleeData;
        if (data != null)
        {
            rb.AddForce(damageInfo.KnockBackForce * (data.ComboData[CurrentActionIndex - 1].KnockbackDirection + rb.transform.forward), ForceMode.Impulse);
            //Debug.DrawRay(rb.position +new Vector3(0,1,0), (data.ComboData[CurrentActionIndex - 1].KnockbackDirection + rb.transform.forward) * 1000, Color.red, Mathf.Infinity, false);
            //Debug.DrawRay(rb.position +new Vector3(0,1,0), rb.transform.forward * 1000, Color.blue, Mathf.Infinity, false);
            //Debug.DrawRay(rb.position +new Vector3(0,1,0), data.ComboData[CurrentActionIndex - 1].KnockbackDirection * 1000, Color.green, Mathf.Infinity, false);
        }
        else rb.AddForce(damageInfo.KnockBackForce * -damageInfo.Victim.transform.forward, ForceMode.Impulse);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        //Animator.applyRootMotion = true;
        agent.enabled = true;
        yield return new WaitForEndOfFrame();
        agent.ResetPath();
        yield return null;
    }
    private void Death(DamageInfo arg0)
    {
        isAlive = false;
        Movement.Stop();
        Movement.CanMove = false;
        enabled = false; 
    }

    private void BlockedAttack(DamageInfo damageInfo)
    {
        StartCoroutine(BlockedAttackRoutine(damageInfo));
    }

    private IEnumerator BlockedAttackRoutine(DamageInfo damageInfo)
    {
        CustomCharacterMovement victimMovement = damageInfo.Victim.GetComponent<CustomCharacterMovement>();
        Rigidbody victimRb = victimMovement.Rigidbody;
        NavMeshAgent victimAgent = victimMovement.NavMeshAgent;
        Animator.applyRootMotion = false;
        victimAgent.enabled = false;
        victimRb.linearVelocity = Vector3.zero;
        isBlockedAttack  = true;
        
        yield return new WaitForEndOfFrame();
        victimRb.AddForce(damageInfo.KnockBackForce * 4 * -damageInfo.Victim.transform.forward, ForceMode.Impulse);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        
        isBlockedAttack =  false;
        victimAgent.enabled = true;
        yield return new WaitForEndOfFrame();
        victimAgent.ResetPath();
        yield return null;
        
        
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

    public void MeleeHitAnimEvent(int attackIndex)
    {
        WeaponsMelee meleeweapon = Weapons[weaponIndex] as WeaponsMelee;
        if (meleeweapon == null) return;
        meleeweapon.MeleeHitAnimEvent(attackIndex);
        
    }
}
