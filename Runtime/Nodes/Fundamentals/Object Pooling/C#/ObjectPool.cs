using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("CustomObjectPool")]
    [RenamedFrom("Unity.VisualScripting.Community.CustomObjectPool")]
    public class ObjectPool : MonoBehaviour
    {
        private Queue<GameObject> objectPoolQueue = new Queue<GameObject>();
        private List<GameObject> activeObjects = new List<GameObject>();
        private GameObject prefab;

        public void Initialize(GameObject prefab, int initialPoolSize)
        {
            this.prefab = prefab;

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateObjectInPool();
            }
        }

        private GameObject CreateObjectInPool()
        {
            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            var poolObject = obj.AddComponent<PoolObject>();
            poolObject.pool = this;
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            objectPoolQueue.Enqueue(obj);
            return obj;
        }

        public GameObject RetrieveObjectFromPool()
        {
            if (objectPoolQueue.Count == 0)
            {
                CreateObjectInPool();
            }

            var obj = objectPoolQueue.Dequeue();
            obj.SetActive(true);

            activeObjects.Add(obj);

            EventBus.Trigger<PoolData>(CommunityEvents.OnRetrieved, new PoolData(this, obj));

            return obj;
        }

        public void ReturnObjectToPool(GameObject obj)
        {
            obj.SetActive(false);

            activeObjects.Remove(obj);
            objectPoolQueue.Enqueue(obj);

            EventBus.Trigger<PoolData>(CommunityEvents.OnReturned, new PoolData(this, obj));
        }

        public ReadOnlyCollection<GameObject> GetActiveObjects()
        {
            return activeObjects.AsReadOnly();
        }

        public static ObjectPool CreatePool(int size, GameObject prefab, GameObject parent = null)
        {
            if (parent == null)
                parent = new GameObject("ObjectPoolParent");

            var objectPool = parent.AddComponent<ObjectPool>();
            objectPool.Initialize(prefab, size);

            // Set all pooled objects as children of the pool's parent
            var children = parent.GetComponentsInChildren<PoolObject>();
            foreach (var child in children)
            {
                if (child.pool == objectPool && child.transform != parent.transform)
                {
                    child.transform.SetParent(parent.transform);
                }
            }

            return objectPool;
        }

        public static void ReturnObject(ObjectPool pool, GameObject obj)
        {
            if (pool != null && obj != null)
            {
                if (obj.TryGetComponent<PoolObject>(out var @object))
                {
                    if (@object.pool == pool)
                    {
                        pool.ReturnObjectToPool(obj);
                    }
                    else
                    {
                        throw new InvalidOperationException($"The object '{obj.name}' does not belong to the specified pool '{pool.name}'.");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"The object '{obj.name}' is not managed by the pooling system (missing PoolObject component).");
                }
            }
        }

        public static void ReturnAllObjects(ObjectPool pool)
        {
            if (pool != null)
            {
                List<GameObject> activeObjectsCopy = new List<GameObject>(pool.GetActiveObjects());

                foreach (var obj in activeObjectsCopy)
                {
                    pool.ReturnObjectToPool(obj);
                }
            }
        }
    }

    public struct PoolData
    {
        public ObjectPool pool;
        public GameObject arg;

        public PoolData(ObjectPool Pool, GameObject args)
        {
            pool = Pool;
            arg = args;
        }
    }

}