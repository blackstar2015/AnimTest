using UnityEngine;

[CreateAssetMenu(menuName = "WeaponSO/New Melee Weapon")]
public class WeaponMeleeData : WeaponData
{
    [field: Header("Weapon Melee")]
    [field: SerializeField] public MeleeComboData[] ComboData { get; private set; }
    [field: SerializeField] public LayerMask HitMask { get; private set; }
}

[System.Serializable]
public class MeleeComboData
{
    [field: SerializeField, Tooltip("Overrides base weapon damage")] public float Damage { get; private set; } = 40f;
    [field: SerializeField] public float Angle { get; private set; } = 120f;
    [field: SerializeField] public float Range { get; private set; } = 1.5f;
}