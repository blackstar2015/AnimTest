using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "WeaponSO/New Ranged Weapon")]
public class WeaponRangedProjectileData : WeaponData
{
    [field: Header("Weapon Ranged Projectile")]
    [field: SerializeField] public RangedWeaponType WeaponType {  get; private set; }
    [field: SerializeField, ShowIf("WeaponType", RangedWeaponType.HitScan)] public LayerMask HitScanMask { get; private set; }
    [field: SerializeField, ShowIf("WeaponType", RangedWeaponType.Projectile)] public Projectile BulletPrefab { get; private set; }
    [field: SerializeField] public int AmmoCount { get; private set; } = 20;
    [field: SerializeField] public int ShotCount { get; private set; } = 6;
    [field: SerializeField] public float Inaccuracy { get; private set; } = 10f;
    [field: SerializeField] public float BulletSpeed { get; private set; } = 30f;
}
public enum RangedWeaponType
{
    Projectile,
    HitScan
}