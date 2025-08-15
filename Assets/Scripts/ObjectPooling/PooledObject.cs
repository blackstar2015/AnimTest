using UnityEngine;
using UnityEngine.Pool;

namespace LazyObjectPooler
{
    public class PooledObject : MonoBehaviour
    {
        [field: Header("Pooling")]
        [field: SerializeField] public int PoolSize { get; set; } = 10;
        [field: SerializeField] public bool IsDisabledInPool { get; set; } = false;
        [field: SerializeField] public bool LogCreation { get; set; } = true;

        // underlying collection of pooled objects of one type (bullet, enemy, explosionVFX, etc.)
        // "linked" collections reference the object before/after them in the collection
        // but not a specific position, we can't do linkedList[5], but they retrieve items faster than an array/list
        public LinkedPool<PooledObject> Pool { get; set; }
        public bool IsInPool { get; set; }

        // stop using object and place back in pool
        public void ReturnToPool()
        {
            if (IsInPool) return;

            // Release() puts the object back into the pool - dumb name
            Pool.Release(this);
        }

        // OnTake/OnReturn will be called when object is removed from and then placed back in pool
        public virtual void OnTake()
        {

        }

        public virtual void OnReturn()
        {

        }
    }
}
