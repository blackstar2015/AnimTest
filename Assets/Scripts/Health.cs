using UnityEngine;
using Sirenix.OdinInspector;    // namespace for all Odin stuff
using UnityEngine.Events;
using System;

public class Health : MonoBehaviour, IDamageable
{
    // fields
    [BoxGroup("Stats"), SerializeField] private float _current = 100f;
    [BoxGroup("Stats"), SerializeField] private float _max = 100f;

    // death
    [BoxGroup("Death"), SerializeField] private string _deathLayer = "Corpse";

    // properties
    [BoxGroup("Debug"), ShowInInspector] public float CurrentHealth => _current;
    [BoxGroup("Debug"), ShowInInspector] public float CurrentPercentage => _current / _max;
    [BoxGroup("Debug"), ShowInInspector] public float MissingHealth => _max - _current;
    [BoxGroup("Debug"), ShowInInspector] public bool IsAlive => _current >= 1f;

    public UnityEvent<DamageInfo> OnDamage;
    public UnityEvent<DamageInfo> OnDeath;
    
    public void Damage(DamageInfo damageInfo)
    {
        if (!IsAlive) return;                       
        if (damageInfo.Amount < 1f) return;         
        
        // reduce health current value
        _current -= damageInfo.Amount;
        _current = Mathf.Clamp(_current, 0f, _max);

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
        float amount = _max * 0.1f;
        DamageInfo damageInfo = new DamageInfo(amount, DamageType.Physical, false, gameObject, gameObject, gameObject);
        Damage(damageInfo);
    }
}