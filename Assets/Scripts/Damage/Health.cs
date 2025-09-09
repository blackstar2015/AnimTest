using UnityEngine;
using Sirenix.OdinInspector;    // namespace for all Odin stuff
using UnityEngine.Events;
using System;

public class Health : MonoBehaviour, IDamageable
{
    // fields
    [field: SerializeField, TabGroup("Stats")] public float Current { get; private set; } = 100f;
    [field: SerializeField, TabGroup("Stats")] public float Max { get; private set; } = 100f;

    // death
    [TabGroup("Death"), SerializeField] private string _deathLayer = "Corpse";

    // properties
    [TabGroup("Debug"), ShowInInspector] public float CurrentHealth => Current;
    [TabGroup("Debug"), ShowInInspector] public float CurrentPercentage => Current / Max;
    [TabGroup("Debug"), ShowInInspector] public float MissingHealth => Max - Current;
    [TabGroup("Debug"), ShowInInspector] public bool IsAlive => Current >= 1f;

    [TabGroup("Events")]public UnityEvent<DamageInfo> OnDamage;
    [TabGroup("Events")] public UnityEvent<DamageInfo> OnDeath;
    
    public void Damage(DamageInfo damageInfo)
    {
        if (!IsAlive) return;                       
        if (damageInfo.Amount < 1f) return;         
        
        // reduce health current value
        Current -= damageInfo.Amount;
        Current = Mathf.Clamp(Current, 0f, Max);

        // invoke the damage event
        OnDamage.Invoke(damageInfo);   
                                                   
        // handle death
        if (!IsAlive)
        {
            OnDeath.Invoke(damageInfo);
            gameObject.layer = LayerMask.NameToLayer(_deathLayer);
        }
    }

    [Button("Damage Test 10%")]
    public void DamageTest()
    {
        float amount = Max * 0.1f;
        DamageInfo damageInfo = new DamageInfo(amount, DamageType.Physical, false, gameObject, gameObject, gameObject);
        Damage(damageInfo);
    }
}