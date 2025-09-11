using UnityEngine;

public class Replicator : MonoBehaviour, IDamageable
{
    public float CurrentHealthPercentage { get; }
    public bool IsAlive { get; }

    public void Damage(DamageInfo damageInfo)
    {
        Instantiate(gameObject, transform.position, transform.rotation);
    }
}