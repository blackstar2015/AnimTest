using GameEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Random = UnityEngine.Random;

public class DebugStuff : MonoBehaviour
{
    [SerializeField] private TransformEventAsset _playerTransform;
    [SerializeField] private CustomEnemyController _enemyPrefab;
    [SerializeField] private CustomPlayerController _player;
    [SerializeField] private Transform[] _spawnTranforms;
    [SerializeField] private List<CustomEnemyController>  _enemies;
    [SerializeField] private bool _shouldSpawn = false;
    [SerializeField] private float _spawnDelay = 2f;
    [field: SerializeField] private int _numberOfEnemies { get;  set; }
    [SerializeField] private AnimationCurve _numEnemyCurve;


    private void Start()
    {
        _player = _playerTransform.CurrentValue.gameObject.GetComponent<CustomPlayerController>();
    }

    private void Update()
    {
        CalculateNumberOfEnemies();
        SpawnEnemy();
        ToggleAutoSpawn();
        TogglePlayerInvincibility();
        ToggleEnemyInvincibility();
        IncrementEnemies();
        KillAllEnemies();
    }

    private int CalculateNumberOfEnemies()
    {
        _numberOfEnemies = Mathf.FloorToInt(_numEnemyCurve.Evaluate(Time.time));
        return _numberOfEnemies;
    }

    private void KillAllEnemies()
    {
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            List<CustomEnemyController> enemies = _enemies;
            foreach (CustomEnemyController enemy in enemies)
            {
                DamageInfo damageInfo = new DamageInfo(1000,DamageType.None,false,enemy.gameObject, this.gameObject,_player.gameObject,0);
                enemy.GetComponent<Health>().OnDeath.Invoke(damageInfo);
            }
        }
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
        if (Input.GetKeyDown(KeyCode.Keypad1) || (_enemies.Count < CalculateNumberOfEnemies() && _shouldSpawn))
        {
            StartCoroutine(SpawnEnemyRoutine());
        }
    }

    private IEnumerator SpawnEnemyRoutine()
    {
            CustomEnemyController enemy = Instantiate(_enemyPrefab, GetRandomSpawnPoint().position, GetRandomSpawnPoint().rotation);
            _enemies.Add(enemy);
            enemy.GetComponent<Health>().OnDeath.AddListener(RemoveEnemy);
        yield return new WaitForSeconds(_spawnDelay);
    }

    private void RemoveEnemy(DamageInfo damageInfo)
    {
        _enemies.Remove(damageInfo.Victim.GetComponent<CustomEnemyController>());
    }

    private Transform GetRandomSpawnPoint()
    {
        int rand = Random.Range(0, _spawnTranforms.Length);

        return _spawnTranforms[rand];

    }
}
