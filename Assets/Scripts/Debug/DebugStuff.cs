using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DebugStuff : MonoBehaviour
{
    [SerializeField] private CustomEnemyController _enemyPrefab;
    [SerializeField] private Transform _spawnTranform;
    [SerializeField] private int _numberOfEnemies = 1;
    [SerializeField] private List<CustomEnemyController>  _enemies;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1) || _enemies.Count < _numberOfEnemies)
        {
            CustomEnemyController enemy = Instantiate(_enemyPrefab, _spawnTranform.position, _spawnTranform.rotation);
            _enemies.Add(enemy);
            enemy.GetComponent<Health>().OnDeath.AddListener(RemoveEnemy);
        }
    }

    private void RemoveEnemy(DamageInfo damageInfo)
    {
        _enemies.Remove(damageInfo.Victim.GetComponent<CustomEnemyController>());
    }
}
