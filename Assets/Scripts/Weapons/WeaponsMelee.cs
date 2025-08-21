using Unity.VisualScripting;
using UnityEngine;

public class WeaponsMelee : Weapons
{
    private Vector3 _aimPosition;
    private GameObject _instigator;
    private int _team;
    private int _attackIndex;

    // casts Data from parent to WeaponMeleeData
    public WeaponMeleeData MeleeData => (WeaponMeleeData)Data;

    protected override void Attack(Vector3 aimPosition, GameObject instigator, int team)
    {
        base.Attack(aimPosition, instigator, team);

        _aimPosition = aimPosition;
        _instigator = instigator;
        _team = team;
    }

    public void MeleeHitAnimEvent(int attackIndex)
    {
        // get specific combo attack data
        MeleeComboData comboData = MeleeData.ComboData[attackIndex];
        _attackIndex = attackIndex;
        // calculate aim direction
        Vector3 origin = this.transform.position;
        Vector3 aimDirection = (_aimPosition - origin).normalized;

        // find all possible targets in range
        
        Collider[] hits = Physics.OverlapSphere(origin, comboData.Range, MeleeData.HitMask);

        // iterate through all hits
        foreach (Collider hit in hits)
        {
            // optional check for friendly fire

            // check for self
            if (hit.gameObject == _instigator) continue; // don't punch self in face

            // filter hits by angle
            Vector3 targetDir = (hit.transform.position - origin).normalized;
            float angleToHit = Vector3.Angle(targetDir, aimDirection);
            if (angleToHit > comboData.Angle / 2f) continue;

            // damage the target
            if (hit.TryGetComponent(out IDamageable targetHealth))
            {
                targetHealth.Damage(new DamageInfo(comboData.Damage, DamageType.Physical, false, hit.gameObject, gameObject, _instigator));
            }
        }
    }

    // private void OnDrawGizmos()
    // {
    //     MeleeComboData comboData = MeleeData.ComboData[_attackIndex];
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(_aimPosition, comboData.Range);
    // }
}
