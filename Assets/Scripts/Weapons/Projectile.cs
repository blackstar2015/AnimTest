using LazyObjectPooler;
using UnityEngine;

public class Projectile : PooledObject
{
    private Rigidbody _rigidbody;
    private Vector3 _spawnPosition;
    private float _damage;
    private float _range;
    private DamageType _damageType;
    private GameObject _instigator;
    private int _team;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Launch(float damage, float range, float speed, DamageType damageType, GameObject instigator, int team)
    {
        _spawnPosition = transform.position;
        _damage = damage;
        _range = range;
        _rigidbody.position = transform.position;
        _rigidbody.rotation = transform.rotation;
        _rigidbody.linearVelocity = transform.forward * speed;
        _damageType = damageType;
        _instigator = instigator;
        _team = team;
    }

    private void FixedUpdate()
    {
        if (IsInPool) return;

        // clean up if travelled past max distance
        float distanceTraveled = Vector3.Distance(_spawnPosition, transform.position);
        if(distanceTraveled > _range)
        {
            Cleanup();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(IsInPool) return;

        // ignore shooting self
        if (other.gameObject == _instigator) return;

        // otherwise attempt to damage collider
        if(other.TryGetComponent(out IDamageable targetHealth))
        {
            DamageInfo damageInfo = new DamageInfo(_damage, _damageType, false, other.gameObject, gameObject, _instigator);
            targetHealth.Damage(damageInfo);
        }

        Cleanup();
    }

    public override void OnTake()
    {
        base.OnTake();

        _rigidbody.isKinematic = false;
    }

    public override void OnReturn()
    {
        base.OnReturn();

        _rigidbody.isKinematic = true;
    }

    private void Cleanup()
    {
        ReturnToPool();
    }
}
