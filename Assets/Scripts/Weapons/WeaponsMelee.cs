using Unity.VisualScripting;
using UnityEngine;

public class WeaponsMelee : Weapons
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
}
