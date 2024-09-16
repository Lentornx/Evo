using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour
{
    public float movementSpeed = 1.0f; // bedzie sparametryzowane i zale¿ne od genow
    public GameObject plant;
    private bool hasSpawnedPlant = false;
    // later on the seed will have all the genes of the plant it will spawn

    public float baseNutrientConsumption = 1.0f;
    public float growthSurplusThreshold = 0.05f;
    public int heightCeiling = 20;
    public float metabolism = 1.0f;
    public float seedCost = 0.1f;
    public float startingGrowth = 0.1f;
    private Vector3 bias; // po to by nasiona porusza³y siê nieco bardziej losowo
    public GameObject plantsParent;

    private static World world;
    private static PoolManager poolManager; // Reference to the PoolManager

    void Start()
    {
        if (world == null)
            world = FindObjectOfType<World>();
        if (poolManager == null)
            poolManager = FindObjectOfType<PoolManager>(); // Find the PoolManager in the scene
        InvokeRepeating("ChangeBias", 0.0f, 1.0f);
    }

    void ChangeBias()
    {
        bias = new Vector3(
            Random.Range(-0.1f, 0.1f),
            0,
            Random.Range(-0.1f, 0.1f)
        );
    }

    void Update()
    {
        transform.position += (world.currentWind + bias) * movementSpeed * Time.deltaTime;
        if (transform.position.y < -1) // Failsafe in case the seed falls out of bounds
        {
            poolManager.ReturnSeed(gameObject); // Return seed to pool if it falls
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasSpawnedPlant && other.gameObject.GetComponent<GroundSegment>() != null)
        {
            hasSpawnedPlant = true; // w innym przypadku umie zespawnic kilka drzew w jednym miejscu, bo skoliduje z kilkoma fragmentami naraz
            GameObject newObject = poolManager.GetPlant(transform.position, Quaternion.identity);
            Plant newPlant = newObject.GetComponent<Plant>();
            Seed s = gameObject.GetComponent<Seed>();
            newPlant.SetupVariables(s.baseNutrientConsumption, s.growthSurplusThreshold, s.heightCeiling, s.metabolism, seedCost, startingGrowth);
            poolManager.ReturnSeed(gameObject);
        }
    }
    public void SetupVariables(float v1, float v2, int v3, float v4, float v5, float v6)
    {
        hasSpawnedPlant = false;
        baseNutrientConsumption = v1;
        growthSurplusThreshold = v2;
        heightCeiling = v3;
        metabolism = v4;
        seedCost = v5;
        startingGrowth = v6;
    }
}