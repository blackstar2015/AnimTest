using GameEvents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;
using Sirenix.Utilities;

public class Spawner : MonoBehaviour
{
    [SerializeField] private TransformEventAsset _playerTransform;
    [SerializeField] private CustomEnemyController _enemyPrefab;
    [SerializeField] private CustomPlayerController _player;
    [SerializeField] private List<SpawnPoint> _spawnTranforms;
    [SerializeField] private List<CustomEnemyController>  _enemies;
    [SerializeField] private CustomEnemyController _lastAttackingEnemy;
    [SerializeField] private bool _shouldSpawn = false;
    [SerializeField] private float _spawnDelay = 2f;
    [field: SerializeField] private int _numberOfEnemies { get; set; } = 0;
    [SerializeField] private AnimationCurve _numEnemyCurve;
    [field: SerializeField, HideInEditorMode, ReadOnly] private int num { get; set; } 
    
    private void Start()
    {
        FindPlayer();
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
        AssignAttackingEnemy();
    }

    private void AssignAttackingEnemy()
    {
        if(_enemies.Count <=0) return;
        List<CustomEnemyController> AttackingEnemies = _enemies;
        foreach (CustomEnemyController enemy in _enemies)
        {
            if(enemy.CanAttackPlayer) return;
        }
        // if (_lastAttackingEnemy != null && AttackingEnemies.Contains(_lastAttackingEnemy))
        // {
        //     AttackingEnemies.Remove(_lastAttackingEnemy);
        // }
        int rand = Random.Range(0, AttackingEnemies.Count);
        AttackingEnemies[rand].CanAttackPlayer = true;
        _lastAttackingEnemy = AttackingEnemies[rand];
    }

    private void FindPlayer()
    {
        _player = _playerTransform.CurrentValue.gameObject.GetComponent<CustomPlayerController>();
    }

    public void AddSpawnPoints(SpawnPoint spawnPoint)
    {
        if(!_spawnTranforms.Contains(spawnPoint))  _spawnTranforms.Add(spawnPoint);
    }

    public void RemoveSpawnPoints(SpawnPoint spawnPoint)
    {
        if (_spawnTranforms.Contains(spawnPoint))  _spawnTranforms.Remove(spawnPoint);
    }

    private int CalculateNumberOfEnemies()
    {
        _numEnemyCurve.postWrapMode = WrapMode.ClampForever;
        num = _numberOfEnemies + Mathf.FloorToInt(_numEnemyCurve.Evaluate(Time.time));
        return num;
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
        if(_spawnTranforms.IsNullOrEmpty()) return;
        if (Input.GetKeyDown(KeyCode.Keypad1) || (_enemies.Count < CalculateNumberOfEnemies() && _shouldSpawn))
        {
            StartCoroutine(SpawnEnemyRoutine());
        }
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        SpawnPoint spawnPoint = GetRandomSpawnPoint();
        CustomEnemyController enemy = Instantiate(_enemyPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
        _enemies.Add(enemy);
        enemy.GetComponent<Health>().OnDeath.AddListener(RemoveEnemy);
        yield return new WaitForSeconds(_spawnDelay);
    }

    private void RemoveEnemy(DamageInfo damageInfo)
    {
        _enemies.Remove(damageInfo.Victim.GetComponent<CustomEnemyController>());
    }

    private SpawnPoint GetRandomSpawnPoint()
    {
        int rand = Random.Range(0, _spawnTranforms.Count);

         return _spawnTranforms[rand];

    }
}
