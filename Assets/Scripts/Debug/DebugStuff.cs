using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugStuff : MonoBehaviour
{
    [SerializeField] private CustomEnemyController _enemyPrefab;
    [SerializeField] private CustomPlayerController _player;
    [SerializeField] private Transform _spawnTranform;
    [SerializeField] private int _numberOfEnemies = 1;
    [SerializeField] private List<CustomEnemyController>  _enemies;
    [SerializeField] private bool _shouldSpawn = false;

    private void Awake()
    {
        _player = FindFirstObjectByType<CustomPlayerController>();
    }

    private void Update()
    {
        SpawnEnemy();
        ToggleAutoSpawn();
        TogglePlayerInvincibility();
        ToggleEnemyInvincibility();
        IncrementEnemies();
    }

    private void IncrementEnemies()
    {
        if(Input.GetKeyDown(KeyCode.Keypad5))
        {
            _numberOfEnemies++;
        }
    }

    private void TogglePlayerInvincibility()
    {
        if(Input.GetKeyDown(KeyCode.Keypad3))
        {
            _player.GetComponent<Health>().Invincibility();
        }
    }

    private void ToggleEnemyInvincibility()
    {
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            foreach(CustomEnemyController enemyController in _enemies)
            {
                enemyController.GetComponent<Health>().Invincibility();
            }
        }
    }

    private void ToggleAutoSpawn()
    {
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            _shouldSpawn = !_shouldSpawn;
        }
    }

    private void SpawnEnemy()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1) || (_enemies.Count < _numberOfEnemies && _shouldSpawn))
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
