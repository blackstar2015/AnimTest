using System.Collections.Generic;
using UnityEngine;

namespace LazyObjectPooler
{
    public class PoolSystem : MonoBehaviour
    {
        // self-creating singleton
        private static PoolSystem _instance;
        public static PoolSystem Instance
        {
            get
            {
                // initialize on first reference
                if(_instance == null )
                {
                    Debug.Log("Initializing PoolSystem");
                    _instance = new GameObject("PoolSystem").AddComponent<PoolSystem>();
                }

                return _instance;
            }
        }

        // dictionaries use a KVP (key-value-pair) to find items
        // the key can be any type, not just an int like array/list
        // each key HAS TO BE UNIQUE
        // please don't try to serialize dictonaries, it's messy and likely there's a better way
        private Dictionary<PooledObject, PoolHandler> _poolHandlers = new Dictionary<PooledObject, PoolHandler>();

        /// <summary>
        /// Returns first item from pool, initializes pool if not yet created.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public PooledObject Get(PooledObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            // initialize PoolHandler if not created yet
            if(!_poolHandlers.ContainsKey(prefab))
            {
                PoolHandler poolHandler = new PoolHandler(prefab, transform, new Vector3(0f, -1000f, 0f));
                _poolHandlers.Add(prefab, poolHandler);
            }

            PooledObject pooledObject = _poolHandlers[prefab].Pool.Get();
            pooledObject.transform.SetPositionAndRotation(position, rotation);
            pooledObject.transform.SetParent(parent);
            return pooledObject;
        }

        // overload for Get without parent parameter
        public PooledObject Get(PooledObject prefab, Vector3 position, Quaternion rotation)
        {
            return Get(prefab, position, rotation, null);
        }
    }
}