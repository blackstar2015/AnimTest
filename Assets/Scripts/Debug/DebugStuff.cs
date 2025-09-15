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
    [SerializeField] private bool _shouldSpawn = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1) || (_enemies.Count < _numberOfEnemies && _shouldSpawn))
        {
            CustomEnemyController enemy = Instantiate(_enemyPrefab, _spawnTranform.position, _spawnTranform.rotation);
            _enemies.Add(enemy);
            enemy.GetComponent<Health>().OnDeath.AddListener(RemoveEnemy);
        }
        if(Input.GetKeyDown(KeyCode.Keypad2))
        {
            _shouldSpawn = !_shouldSpawn;
        }
    }

    private void RemoveEnemy(DamageInfo damageInfo)
    {
        _enemies.Remove(damageInfo.Victim.GetComponent<CustomEnemyController>());
    }
}
