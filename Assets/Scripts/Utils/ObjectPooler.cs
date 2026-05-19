using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generic object pooler for efficient memory management
/// Used for coins, particles, and UI elements
/// </summary>
public class ObjectPooler<T> where T : Component
{
    private Queue<T> pooledObjects = new Queue<T>();
    private Stack<T> activeObjects = new Stack<T>();
    private GameObject prefab;
    private Transform parent;
    private int initialPoolSize;

    public ObjectPooler(GameObject prefab, int initialSize = 50, Transform parentTransform = null)
    {
        this.prefab = prefab;
        this.parent = parentTransform;
        this.initialPoolSize = initialSize;

        // Pre-populate pool
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, parent);
            obj.SetActive(false);
            T component = obj.GetComponent<T>();
            pooledObjects.Enqueue(component);
        }
    }

    /// <summary>
    /// Get an object from the pool
    /// </summary>
    public T GetPooledObject()
    {
        T obj;

        if (pooledObjects.Count > 0)
        {
            obj = pooledObjects.Dequeue();
        }
        else
        {
            GameObject newObj = Object.Instantiate(prefab, parent);
            obj = newObj.GetComponent<T>();
        }

        obj.gameObject.SetActive(true);
        activeObjects.Push(obj);
        return obj;
    }

    /// <summary>
    /// Return an object to the pool
    /// </summary>
    public void ReturnPooledObject(T obj)
    {
        obj.gameObject.SetActive(false);
        pooledObjects.Enqueue(obj);
    }

    /// <summary>
    /// Clear entire pool
    /// </summary>
    public void ClearPool()
    {
        while (pooledObjects.Count > 0)
        {
            T obj = pooledObjects.Dequeue();
            Object.Destroy(obj.gameObject);
        }
    }
}
