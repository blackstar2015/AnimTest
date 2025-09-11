using UnityEngine;
using Sirenix.OdinInspector;    // namespace for all Odin stuff
using UnityEngine.Events;
using System;
using System.Net.Sockets;

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

    // properties
    [TabGroup("Properties"), ShowInInspector] public float CurrentHealth => _currentHealth;
    [TabGroup("Properties"), ShowInInspector] public float MissingHealth => _maxHealth - _currentHealth;
    [TabGroup("Properties"), ShowInInspector] public float CurrentHealthPercentage => _currentHealth / _maxHealth; 
    [TabGroup("Properties"), ShowInInspector] public bool IsAlive => _currentHealth >= 1f;
    [TabGroup("Properties"), ShowInInspector] public float CurrentStamina => _currentStamina;
    [TabGroup("Properties"), ShowInInspector] public float MissingStamina => _maxStamina - _currentStamina;
    [TabGroup("Properties"), ShowInInspector] public float CurrentStaminaPercentage => _currentStamina / _maxStamina;
    [TabGroup("Properties"), ShowInInspector] public bool CanBlock => _currentStamina >= 1f;
    [TabGroup("Properties"), ShowInInspector] public bool IsBLocking;

    [TabGroup("Events")]public UnityEvent<DamageInfo> OnDamage;
    [TabGroup("Events")] public UnityEvent<DamageInfo> OnDeath;
    [TabGroup("Events")] public UnityEvent<DamageInfo> OnBlock;
    [TabGroup("Events")] public UnityEvent OnUpdateStamina;

    public void Damage(DamageInfo damageInfo)
    {
        if (!IsAlive) return;                       
        if (damageInfo.Amount < 1f) return;
        if(damageInfo.Victim.GetComponent<CustomController>().IsBlocking)
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
            gameObject.layer = LayerMask.NameToLayer(_deathLayer);
        }
    }

    private void HandleBlock(DamageInfo damageInfo)
    {
        if(!IsAlive || !CanBlock) return;
        if (damageInfo.Amount < 1f) return;

        _currentStamina -= damageInfo.Amount;
        _currentStamina = Mathf.Clamp(_currentStamina, 0f, _maxStamina);

        if(_currentStamina <= 0) damageInfo.Amount = damageInfo.Amount / 2f;

        OnBlock.Invoke(damageInfo);
    }

    private void BreakBlock(DamageInfo damageInfo)
    {
        
    }

    [Button("Damage Test 10%")]
    public void DamageTest()
    {
        float amount = _maxHealth * 0.1f;
        DamageInfo damageInfo = new DamageInfo(amount, DamageType.Physical, false, gameObject, gameObject, gameObject);
        Damage(damageInfo);
    }

    private void Update()
    {
        RegenStamina();
    }

    private void RegenStamina()
    {
        if(IsBLocking) return;
        _currentStamina += 1/_staminaRegenDuration * Time.deltaTime;
        _currentStamina = Mathf.Clamp(_currentStamina,0, _maxStamina);
        OnUpdateStamina.Invoke();
    }
}