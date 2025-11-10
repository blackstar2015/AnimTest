using UnityEngine;

public class WeaponMelee : Weapon
{
    private Vector3 _aimPosition;
    private Vector3 _attackOrigin;
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
        MeleeComboData comboData = MeleeData.ComboData[attackIndex-1];

        // calculate aim direction
        Vector3 origin = _instigator.transform.position;
        Vector3 aimDirection = (_aimPosition - origin).normalized;

        // find all possible targets in range
        // WE'RE USING AN OVERLAPSPHERE, NOT A SPHERECAST
        // overlapshere is a stationary instantaneous radius check
        Collider[] hits = new Collider[10];
        var size = Physics.OverlapSphereNonAlloc(origin, comboData.Range,hits, MeleeData.HitMask);

        // iterate through all hits
        foreach (Collider hit in hits)
        {
            if(hit == null) return;
            // check for self
            if (hit.gameObject == _instigator) continue; // don't punch self in face
            // optional check for friendly fire
            if(!hit.gameObject.TryGetComponent(out Targetable target)) continue;
            if(target.Team == _team) continue;

            // filter hits by angle
            Vector3 targetDir = (hit.transform.position - origin).normalized;
            float angleToHit = Vector3.Angle(targetDir, aimDirection);
            if (angleToHit > comboData.Angle / 2f) continue;

            // damage the target
            if (hit.TryGetComponent(out IDamageable targetHealth))
            {
                targetHealth.Damage(new DamageInfo(comboData.Damage, DamageType.None, false, hit.gameObject, gameObject, _instigator, MeleeData.KnockbackForce));
            }
        }
    }


}
