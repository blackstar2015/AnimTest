using UnityEngine;

public class WeaponData : ScriptableObject
{
    [field: Header("Weapon")]
    [field: SerializeField] public float Damage { get; private set; } = 5f;
    [field: SerializeField] public float AttackRate { get; private set; } = 3f;
    [field: SerializeField] public float Duration { get; private set; } = 1f;
    [field: SerializeField] public float CritChance { get; private set; } = 5f;
    [field: SerializeField] public float Range { get; private set; } = 5f;
    [field: SerializeField] public float EffectiveRange { get; private set; } = 4f;
    [field: SerializeField] public int WeaponIndex { get; private set; } = 0;
    [field: SerializeField] public DamageType DamageType { get; private set; } = DamageType.Physical;
    
    [field: SerializeField] public GameObject WeaponMesh  { get; private set; }

    // add feedback
    [field: SerializeField] public string AttackAnimName { get; private set; } = "";
    //[field: SerializeField] public EventReference SFX { get; private set; }
}