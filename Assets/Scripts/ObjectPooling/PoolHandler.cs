using UnityEngine;
using UnityEngine.Pool;

namespace LazyObjectPooler
{
    public class PoolHandler
    {
        public LinkedPool<PooledObject> Pool { get; private set; }

        private Transform _parent;
        private Vector3 _hidePosition;
        private PooledObject _prefab;

        public PoolHandler(PooledObject prefab, Transform grandParent, Vector3 hidePosition)
        {
            _prefab = prefab;
            _parent = new GameObject($"{_prefab.gameObject.name} Pool").transform;
            _parent.SetParent(grandParent);

            Pool = new LinkedPool<PooledObject>(OnCreateItem, OnTakeItem, OnReturnItem, OnDestroyItem, true, prefab.PoolSize);
        }

        // called when adding NEW objec to pool
        private PooledObject OnCreateItem()
        {
            PooledObject instantiated = GameObject.Instantiate(_prefab, _hidePosition, Quaternion.identity, _parent);
            instantiated.Pool = Pool;
            return instantiated;
        }

        // object removed from pool for use
        private void OnTakeItem(PooledObject item)
        {
            if (item.IsDisabledInPool) item.gameObject.SetActive(true);
            item.IsInPool = false;
            item.OnTake();
        }

        private void OnReturnItem(PooledObject item)
        {
            if (item.IsDisabledInPool) item.gameObject.SetActive(false);
            item.IsInPool = true;
            item.transform.position = _hidePosition;
            item.transform.SetParent(_parent);
            item.OnReturn();
        }

        private void OnDestroyItem(PooledObject item)
        {
            if (item == null) return;
            GameObject.Destroy(item.gameObject);
        }
    }
}
