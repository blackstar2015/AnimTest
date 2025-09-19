// using System;
// using UnityEngine;
//
// public class DamageTrigger : MonoBehaviour
// {
//     [SerializeField] private int _weaponIndex;
//     private float _damage;
//     private float _knockbackForce;
//     private DamageType _damageType;
//     private GameObject _instigator;
//     private CustomController _player;
//     private WeaponMeleeData _weaponData;
//     private void Start()
//     {
//         _player = GetComponentInParent<CustomController>();
//         _weaponData = _player.Weapons[_weaponIndex].Data as WeaponMeleeData;
//     }
//
//     private void Update()
//     {
//         if(_weaponIndex != _player.CurrentWeaponIndex) return;
//         _damageType = _weaponData.DamageType;
//         _instigator = _player.gameObject;
//         _damage = _weaponData.ComboData[_player.CurrentActionIndex-1].Damage; 
//         _knockbackForce = _weaponData.KnockbackForce;
//         //_knockbackForce = _weaponData.ComboData[_player.CurrentActionIndex-1].KnockbackForce;
//     }
//
//     private void OnTriggerEnter(Collider other)
//     {
//         if(_weaponIndex != _player.CurrentWeaponIndex) return;
//         if(other.TryGetComponent(out IDamageable targetHealth))
//         {
//             if (!targetHealth.IsAlive) return;
//             DamageInfo damageInfo = new DamageInfo(_damage, _damageType, false, other.gameObject, gameObject, _instigator,_knockbackForce);
//             targetHealth.Damage(damageInfo);
//         }
//     }
// }
