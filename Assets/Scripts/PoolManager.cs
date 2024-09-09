using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public GameObject seedPrefab;   
    public GameObject plantPrefab;  
    public Transform seedParent;    
    public Transform plantParent;  

    public ObjectPool seedPool;
    public ObjectPool plantPool;

    void Start()
    {
        seedPool.InitializePool(seedPrefab, seedParent);
        plantPool.InitializePool(plantPrefab, plantParent);
    }

    public GameObject GetSeed(Vector3 position, Quaternion rotation)
    {
        return seedPool.GetObject(position, rotation);
    }

    public void ReturnSeed(GameObject seed)
    {
        seedPool.ReturnObject(seed);
    }

    public GameObject GetPlant(Vector3 position, Quaternion rotation)
    {
        return plantPool.GetObject(position, rotation);
    }

    public void ReturnPlant(GameObject plant)
    {
        plantPool.ReturnObject(plant);
    }
}
