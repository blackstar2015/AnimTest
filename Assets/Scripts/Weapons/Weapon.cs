using System;
using System.Collections.Generic;
using GameEvents;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [field: SerializeField,TabGroup("WeaponData"), Required, InlineEditor] public WeaponData Data { get; private set; }
    [field: SerializeField,TabGroup("WeaponData")] public GameObject WeaponMesh  { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string IdleHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string MovementHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string AirborneJumpHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string AirborneFlipHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string AirborneFallHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string AirborneLandHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string AirborneDashHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string BlockHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string DodgeHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string DamageHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string StunnedHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string SwitchToHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string SwitchFromHash { get; set; }
    [field: SerializeField, TabGroup("AnimHashes")] public string LockedDodgeHash { get; set; }
    private float _lastAttackTime = -100000f;
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponentInParent<Animator>();
    }

// attempt attack while respecting cooldown or other limiting factors
    public bool TryAttack(Vector3 aimPosition, GameObject instigator, int team)
    {
        // common simple cooldown pattern
        float cooldown = 1f / Data.AttackRate;
        float nextAttackTime = _lastAttackTime + cooldown;
        if(Time.time >= nextAttackTime)
        {
            _lastAttackTime = Time.time;
            Attack(aimPosition, instigator, team);
            return true;
        }
        return false;
    }

    protected virtual void Attack(Vector3 aimPosition, GameObject instigator, int team)
    {
        
    }
    
}
