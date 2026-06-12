using UnityEngine;
using System.Collections.Generic;
using PeopleFlow.Gameplay;

namespace PeopleFlow.Core
{
    /// <summary>
    /// Simple singleton pool manager for recycling minions and other GameObjects.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

       [SerializeField] private List<MinionRowAgent> _minionRowAgents = new List<MinionRowAgent>();

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

        public MinionRowAgent TryWithraw()
        {
            if (_minionRowAgents.Count>0)
            {
                var row= _minionRowAgents[0];
                _minionRowAgents.Remove(row);
                return row;
            }

            return null;
        }

        public void Deposit(MinionRowAgent agent)
        {
            _minionRowAgents.Add(agent);
        }
        // public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        // {
        //     if (!_pools.ContainsKey(prefab))
        //         _pools[prefab] = new Queue<GameObject>();
        //
        //     GameObject go;
        //     if (_pools[prefab].Count > 0)
        //     {
        //         go = _pools[prefab].Dequeue();
        //         go.transform.SetPositionAndRotation(position, rotation);
        //         go.transform.SetParent(parent);
        //         go.SetActive(true);
        //     }
        //     else
        //     {
        //         go = Instantiate(prefab, position, rotation, parent);
        //     }
        //     return go;
        // }
        //
        // public void Return(GameObject prefab, GameObject go)
        // {
        //     if (go == null) return;
        //     
        //     go.SetActive(false);
        //     if (!_pools.ContainsKey(prefab))
        //         _pools[prefab] = new Queue<GameObject>();
        //     
        //     _pools[prefab].Enqueue(go);
        // }
    }
}