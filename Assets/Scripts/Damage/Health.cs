using UnityEngine;
using Sirenix.OdinInspector;    // namespace for all Odin stuff
using UnityEngine.Events;
using System.Collections;
using UnityEngine.AI;
using System;

public class Health : MonoBehaviour, IDamageable
{
    // fields
    [field: SerializeField, TabGroup("Stats")] private float _currentHealth { get;  set; } = 100f;
    [field: SerializeField, TabGroup("Stats")] private float _currentStamina { get;  set; } = 100f;
    [field: SerializeField, TabGroup("Stats")] private float _maxHealth { get;  set; } = 100f;
    [field: SerializeField, TabGroup("Stats")] private float _maxStamina { get;  set; } = 100f;
    [field: SerializeField, TabGroup("Stats")] private float _staminaRegenDuration { get;  set; } = 5f;

    // death
    [TabGroup("Death"), SerializeField] private string _deathLayer = "Corpse";
    [TabGroup("Death"), SerializeField] private bool _isInvincible;

    // properties
    [TabGroup("Properties"), ShowInInspector] public float CurrentHealth => _currentHealth;
    [TabGroup("Properties"), ShowInInspector] public float MissingHealth => _maxHealth - _currentHealth;
    [TabGroup("Properties"), ShowInInspector] public float CurrentHealthPercentage => _currentHealth / _maxHealth; 
    [TabGroup("Properties"), ShowInInspector] public bool IsAlive => _currentHealth >= 1f;
    [TabGroup("Properties"), ShowInInspector] public float CurrentStamina => _currentStamina;
    [TabGroup("Properties"), ShowInInspector] public float MissingStamina => _maxStamina - _currentStamina;
    [TabGroup("Properties"), ShowInInspector] public float CurrentStaminaPercentage => _currentStamina / _maxStamina;
    [TabGroup("Properties"), ShowInInspector ] public bool CanBlock => _currentStamina >= 1f;
    [TabGroup("Properties"), ShowInInspector ] public bool IsPerfectBlocking { get; set; }
    [TabGroup("Properties"), ShowInInspector] public bool IsBlocking{ get; set; }
    [TabGroup("Properties"), ShowInInspector] public bool IsHitReacting { get; set; }
    
    //Events
    [TabGroup("Events")]public UnityEvent<DamageInfo> OnDamage;
    [TabGroup("Events")] public UnityEvent<DamageInfo> OnDeath;
    [TabGroup("Events")] public UnityEvent<DamageInfo> OnBlock;
    [TabGroup("Events")] public UnityEvent OnUpdateStamina;
    //[TabGroup("Events")] public UnityEvent<DamageInfo> OnBlockedAttack;


    private void Awake()
    {
        OnDamage.AddListener(HitReact);
        OnDeath.AddListener(Death);
    }

    private void HitReact(DamageInfo damageInfo)
    {        
        StartCoroutine(HitReactRoutine(damageInfo));
    }

    private IEnumerator HitReactRoutine(DamageInfo damageInfo)
    {
        if (!IsAlive) yield break;
        Animator animator = damageInfo.Victim.gameObject.GetComponent<Animator>();
        NavMeshAgent agent =  damageInfo.Victim.gameObject.GetComponent<NavMeshAgent>();
        IsHitReacting = true;
        agent.enabled = false;
        animator.applyRootMotion = false;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        //yield return new WaitForEndOfFrame();
        IsHitReacting = false;
        animator.applyRootMotion = true;
        agent.enabled = true;
        if(agent.isOnNavMesh) agent.ResetPath();
        yield return null;        
    }
    public void Damage(DamageInfo damageInfo)
    {
        if (!IsAlive || _isInvincible) return;                       
        if (damageInfo.Amount < 1f) return;
        GameObject victomGO = damageInfo.Victim.gameObject;
        if(victomGO.GetComponent<StateMachine>().IsBlocking)
        {
            HandleBlock(damageInfo);
            return;
        }
        
        // reduce health current value
        _currentHealth -= damageInfo.Amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth);

        // invoke the damage event
        OnDamage.Invoke(damageInfo);
                                                   
        // handle death
        if (!IsAlive)
        {
            OnDeath.Invoke(damageInfo);
        }
    }

    private void HandleBlock(DamageInfo damageInfo)
    {
        if(!IsAlive || !CanBlock) return;
        if (damageInfo.Amount < 1f) return;

        _currentStamina -= damageInfo.Amount;
        _currentStamina = Mathf.Clamp(_currentStamina, 0f, _maxStamina);

        if(_currentStamina <= 0)
        {
            damageInfo.Amount = damageInfo.Amount / 2f;
            OnBlock.Invoke(damageInfo);
            return ;
        }
        OnBlock.Invoke(damageInfo);
        //damageInfo.Instigator.GetComponent<Health>().OnBlockedAttack.Invoke(damageInfo);
    }
    public void Death(DamageInfo damageInfo)
    {
        //gameObject.layer = LayerMask.NameToLayer(_deathLayer);
        OnDamage.RemoveListener(HitReact);
        Transform[] children = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            child.gameObject.layer = LayerMask.NameToLayer(_deathLayer);
        }
        
    }
    private void BreakBlock(DamageInfo damageInfo)
    {
        
    }

    [TabGroup("Stats") , Button("Damage Test 10%")]
    public void DamageTest()
    {
        float amount = _maxHealth * 0.1f;
        DamageInfo damageInfo = new DamageInfo(amount, DamageType.Physical, false, gameObject, gameObject, gameObject, 0);
        Damage(damageInfo);
    }

    private void Update()
    {
        RegenStamina();
    }

    private void RegenStamina()
    {
        if(IsBlocking) return;
        _currentStamina += 1/_staminaRegenDuration * Time.deltaTime;
        _currentStamina = Mathf.Clamp(_currentStamina,0, _maxStamina);
        OnUpdateStamina.Invoke();
    }

    public void Invincibility()
    {
        _isInvincible = !_isInvincible;
    }
}