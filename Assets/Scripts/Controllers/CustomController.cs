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

    [field: SerializeField, InlineButton(nameof(FindWeapons), "Find"), TabGroup("Weapons")] public Weapon[] Weapons { get; private set; }

    [field: SerializeField, TabGroup("Properties")] public bool LookInCameraDirection { get; set; }
    [field: SerializeField, TabGroup("Properties")] protected int actionIndex = 0;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly] protected int weaponIndex = 0;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly]public bool CanShoot { get; set; } = true;
    [field: SerializeField, TabGroup("Properties"), HideInEditorMode, ReadOnly]public bool CanMelee { get; set; } = true;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly]public bool IsBlocking => isBlocking;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly]public bool IsAlive => isAlive;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly]public bool CanBlock => canBlock;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly]public bool IsHitReacting => isHitReacting;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly] public bool IsBlockedAttack => isBlockedAttack;
    [TabGroup("Properties"), ShowInInspector, HideInEditorMode, ReadOnly] public bool IsPerfectBlocking =>  isPerfectBlocking;
    protected bool isBlocking { get; set; }
    protected bool canBlock  { get; set; }
    protected bool isHitReacting { get; set; }
    protected bool isAlive { get; set; }
    protected bool isBlockedAttack { get; set; }
    protected bool isPerfectBlocking { get; set; }
    
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
        Health.OnBlock.AddListener(BlockedAttack);
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
        yield return new WaitForEndOfFrame();

        CustomController instigatorController = damageInfo.Instigator.GetComponent<CustomController>();
        WeaponMeleeData data = instigatorController.Weapons[instigatorController.CurrentWeaponIndex].Data as WeaponMeleeData;
        if (data != null)
        {
            Vector3 knockbackDirection = (data.ComboData[instigatorController.CurrentActionIndex].KnockbackDirection).normalized;
            rb.AddForce(damageInfo.KnockBackForce * (knockbackDirection + damageInfo.Instigator.transform.forward), ForceMode.Impulse);
            AnimatorClipInfo[] currentClipInfo = instigatorController.Animator.GetCurrentAnimatorClipInfo(0);
            Debug.Log(instigatorController.CurrentActionIndex + " " 
                + currentClipInfo[0].clip.name + " "
                + data.ComboData[instigatorController.CurrentActionIndex].KnockbackDirection);
        }
        else rb.AddForce(damageInfo.KnockBackForce * -damageInfo.Victim.transform.forward, ForceMode.Impulse);
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length/2);
        //Animator.applyRootMotion = true;
        rb.linearVelocity = Vector3.zero;
        agent.enabled = true;
        yield return new WaitForEndOfFrame();
        if(agent.isOnNavMesh) agent.ResetPath();
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
        CustomController controller = damageInfo.Instigator.GetComponent<CustomController>();
        WeaponMeleeData data = controller.Weapons[controller.CurrentWeaponIndex].Data as WeaponMeleeData;
        CustomCharacterMovement movement = controller.Movement;
        Rigidbody rb = movement.Rigidbody;
        NavMeshAgent agent = movement.NavMeshAgent;
        Animator.applyRootMotion = false;
        agent.enabled = false;
        rb.linearVelocity = Vector3.zero;
        isBlockedAttack  = true;
        
        yield return new WaitForEndOfFrame();
        
        if (isPerfectBlocking && data != null)
        {
            Vector3 knockbackDirection = (data.ComboData[controller.CurrentActionIndex].KnockbackDirection).normalized;
            rb.AddForce(damageInfo.KnockBackForce * (knockbackDirection + damageInfo.Instigator.transform.forward), ForceMode.Impulse);
            Debug.DrawLine(transform.position, damageInfo.Instigator.transform.position, Color.red);
        }
        else rb.AddForce(damageInfo.KnockBackForce * -damageInfo.Victim.transform.forward, ForceMode.Impulse);
        
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length);
        
        isBlockedAttack =  false;
        rb.linearVelocity = Vector3.zero;
        agent.enabled = true;
        yield return new WaitForEndOfFrame();
        if(agent.isOnNavMesh) agent.ResetPath();
        yield return null;
    }

    private IEnumerator PerfectBlockRoutine()
    {
        isPerfectBlocking = true;
        
        yield return new WaitForSeconds(Weapons[CurrentWeaponIndex].Data.PerfectBlockTime);
        
        isPerfectBlocking = false;
    }
    private void FindWeapons()
    {
        Weapons = GetComponentsInChildren<Weapon>();
    }

    protected virtual void Update()
    {
        isAlive = Health.IsAlive;
        canBlock  = Health.CanBlock;
        isHitReacting = Health.IsHitReacting;
        Health.IsPerfectBlocking = isPerfectBlocking;    }

    #region AnimEvents
    public void MeleeHitAnimEvent(int attackIndex)
    {
        WeaponMelee meleeweapon = Weapons[weaponIndex] as WeaponMelee;
        if (meleeweapon == null) return;
        meleeweapon.MeleeHitAnimEvent(attackIndex);
    }

    public void PerfectBlock()
    {
        StartCoroutine(PerfectBlockRoutine());
    }
    #endregion
}
