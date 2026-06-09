using UnityEngine;
using System.Collections.Generic;

namespace PeopleFlow.Core
{
    /// <summary>
    /// Simple singleton pool manager for recycling minions and other GameObjects.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad(gameObject); // Optional based on project needs
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!_pools.ContainsKey(prefab))
                _pools[prefab] = new Queue<GameObject>();

            GameObject go;
            if (_pools[prefab].Count > 0)
            {
                go = _pools[prefab].Dequeue();
                go.transform.SetPositionAndRotation(position, rotation);
                go.transform.SetParent(parent);
                go.SetActive(true);
            }
            else
            {
                go = Instantiate(prefab, position, rotation, parent);
            }
            return go;
        }

        public void Return(GameObject prefab, GameObject go)
        {
            if (go == null) return;
            
            go.SetActive(false);
            if (!_pools.ContainsKey(prefab))
                _pools[prefab] = new Queue<GameObject>();
            
            _pools[prefab].Enqueue(go);
        }
    }
}