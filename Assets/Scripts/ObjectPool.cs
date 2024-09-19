using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject objectPrefab; 
    public int initialPoolSize = 100;  // Initial number of objects to pre-create
    private Transform parentTransform; // keeps active objects for debug purposes

    private Queue<GameObject> pool = new Queue<GameObject>();
    public void InitializePool(GameObject prefab, Transform parent)
    {
        objectPrefab = prefab;
        parentTransform = parent;

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(objectPrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue(); 
        }
        else
        {
            obj = Instantiate(objectPrefab, parentTransform);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.parent = parentTransform;
        obj.SetActive(true);
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false); 
        pool.Enqueue(obj);    
        obj.transform.parent = transform; // back to the pool hierarchy for debbuging purposes
    }
}
