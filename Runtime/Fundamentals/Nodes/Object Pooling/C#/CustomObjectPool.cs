using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class CustomObjectPool : MonoBehaviour
{
    private Queue<GameObject> objectPoolQueue = new Queue<GameObject>();
    private List<GameObject> activeObjects = new List<GameObject>(); // New list for active objects
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
        obj.SetActive(false);
        obj.transform.SetParent(transform); // Set the object as a child of the pool's parent
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

        // Keep track of active objects
        activeObjects.Add(obj);

        EventBus.Trigger<PoolData>(ObjectPoolEvents.OnRetrieved, new(this, obj));

        return obj;
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);

        // Remove from active objects and add back to the queue
        activeObjects.Remove(obj);
        objectPoolQueue.Enqueue(obj);

        EventBus.Trigger<PoolData>(ObjectPoolEvents.OnReturned, new(this, obj));
    }

    public List<GameObject> GetActiveObjects()
    {
        return activeObjects;
    }
}

public static class ObjectPoolEvents 
{
    public static string OnRetrieved = "Retrieved";
    public static string OnReturned = "Returned";
}

public struct PoolData 
{
    public CustomObjectPool pool;
    public GameObject arg;

    public PoolData(CustomObjectPool Pool, GameObject args) 
    {
        pool = Pool;
        arg = args;
    }
}
