using UnityEngine;

public class Replicator : MonoBehaviour, IDamageable
{
    public float CurrentPercentage { get; }
    public bool IsAlive { get; }

    public void Damage(DamageInfo damageInfo)
    {
        Instantiate(gameObject, transform.position, transform.rotation);
    }
}