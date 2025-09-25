using GameEvents;
using System;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private TransformEventAsset _player;
    [SerializeField] private float _minDistance = 1f;
    private float _distanceToPlayer;
    [SerializeField] public bool IsSpawning;

    private void Update()
    {
        CalculateDistanceToPlayer();
        SpawnCheck();
        UpdateSpawnerList();
    }

    private void UpdateSpawnerList()
    {
        Spawner spawner = GetComponentInParent<Spawner>();
        if (IsSpawning)
        {
            Debug.DrawLine(transform.position, _player.CurrentValue.position, Color.rebeccaPurple);
            spawner.AddSpawnPoints(this);
        }
        else spawner.RemoveSpawnPoints(this);
    }

    private bool SpawnCheck()
    {
        return IsSpawning = _distanceToPlayer < _minDistance;
    }

    private float CalculateDistanceToPlayer()
    {
        return _distanceToPlayer = Vector3.Distance(_player.CurrentValue.position, transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, _minDistance);
    }
}
