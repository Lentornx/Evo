using System.Collections.Generic;
using UnityEngine;

public class PlantPool : MonoBehaviour
{
    public GameObject plantPrefab;      // The prefab of the plant object
    public int initialPoolSize = 50;    // Initial number of plants to pre-create
    public GameObject plantParent;
    private Transform plantParentTransform;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        plantParentTransform = plantParent.transform;
        // Pre-populate the pool with inactive Plant objects
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(plantPrefab, transform);
            obj.SetActive(false);       // Disable it so it's not visible in the scene
            pool.Enqueue(obj);          // Add it to the pool
        }
    }

    // Get a Plant object from the pool
    public GameObject GetPlant(Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();       // Reuse an object from the pool
        }
        else
        {
            obj = Instantiate(plantPrefab);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.parent = plantParentTransform;
        obj.SetActive(true);            // Activate the plant
        return obj;
    }

    // Return a Plant object back to the pool
    public void ReturnPlant(GameObject obj)
    {
        obj.SetActive(false);           // Disable the plant
        pool.Enqueue(obj);              // Add it back to the pool
        obj.transform.parent = transform; 
    }
}
