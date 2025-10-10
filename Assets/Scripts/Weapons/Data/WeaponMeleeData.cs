using UnityEngine;

[CreateAssetMenu(menuName = "WeaponSO/New Melee Weapon")]
public class WeaponMeleeData : WeaponData
{
    [field: SerializeField] public LayerMask HitMask { get; private set; }
    [field: SerializeField] public float KnockbackForce { get; private set; } = 100f;
    [field: SerializeField] public MeleeComboData[] ComboData { get; private set; }
    
}

[System.Serializable]
public class MeleeComboData
{
    [field: SerializeField] public string AttackHashName { get; set; }
    [field: SerializeField, Tooltip("Overrides base weapon damage")] public float Damage { get; private set; } = 40f;
    [field: SerializeField] public float Angle { get; private set; } = 120f;
    [field: SerializeField] public float Range { get; private set; } = 1.5f;
    [field: SerializeField] public float KnockbackMultiplier { get; private set; } = 1f;
    [field: SerializeField] public Vector3 KnockbackDirection { get; private set; } = new Vector3(0f, 0f, -1f);
}