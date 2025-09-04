using System;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    [SerializeField] private int _weaponIndex;
    private float _damage;
    private DamageType _damageType;
    private GameObject _instigator;
    private CustomPlayerController _player;
    private WeaponMeleeData _weaponData;
    private void Awake()
    {
        _player = GetComponentInParent<CustomPlayerController>();
        _weaponData = _player.Weapons[_weaponIndex].Data as WeaponMeleeData;
    }

    private void Update()
    {
        if(_weaponIndex != _player.CurrentWeaponIndex) return;
        _damageType = _weaponData.DamageType;
        _instigator = _player.gameObject;
        _damage = _weaponData.ComboData[_player.CurrentActionIndex-1].Damage; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out IDamageable targetHealth))
        {
            DamageInfo damageInfo = new DamageInfo(_damage, _damageType, false, other.gameObject, gameObject, _instigator);
            targetHealth.Damage(damageInfo);
            Debug.Log(_player.CurrentActionIndex);
        }
    }
}
