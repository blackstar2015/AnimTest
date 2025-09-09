using System;
using UnityEngine;

public class DebugStuff : MonoBehaviour
{
    [SerializeField] private CustomEnemyController _enemyPrefab;
    [SerializeField] private Transform _spawnTranform;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            Instantiate(_enemyPrefab, _spawnTranform.position, _spawnTranform.rotation);
        }
    }
}
